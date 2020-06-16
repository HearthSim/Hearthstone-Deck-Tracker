using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BobsBuddy;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils;
using BobsBuddy.Simulation;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal class BobsBuddyInvoker
	{
		private const int Iterations = 10_000;
		private const int StateChangeDelay = 500;
		private const int HeroPowerTriggerTimeout = 5000;
		private const int MaxTime = 1_500;
		private const int MaxTimeForComplexBoards = 3_000;
		private const int MinimumSimulationsToReportSentry = 2500;

		internal static int ThreadCount => Environment.ProcessorCount / 2;

		private readonly GameV2 _game;
		private readonly Random _rnd = new Random();

		private static BobsBuddyPanel BobsBuddyDisplay => Core.Overlay.BobsBuddyDisplay;
		private static bool ReportErrors => RemoteConfig.Instance.Data?.BobsBuddy?.SentryReporting ?? false;

		private Entity _attackingHero;
		private Entity _defendingHero;
		private TestOutput _output;
		private TestInput _input;
		private int _turn;
		private readonly List<string> _debugLog = new List<string>();

		private MinionHeroPowerTrigger _minionHeroPowerTrigger;
		private static Guid _currentGameId;
		private static readonly Dictionary<string, BobsBuddyInvoker> _instances = new Dictionary<string, BobsBuddyInvoker>();

		public static BobsBuddyInvoker GetInstance(Guid gameId, int turn)
		{
			if(_currentGameId != gameId)
			{
				Log.Debug("New GameId. Clearing instances...");
				_instances.Clear();
			}
			_currentGameId = gameId;

			var key = $"{gameId}_{turn}";
			if(!_instances.TryGetValue(key, out var instance))
			{
				instance = new BobsBuddyInvoker(key);
				_instances[key] = instance;
			}
			return instance;
		}

		public void DebugLog(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			Log.Info(msg, memberName, sourceFilePath);
			_debugLog.Add($"{DateTime.Now.ToLongTimeString()}|{memberName} >> {msg}");
		}

		private readonly string _instanceKey;
		private BobsBuddyInvoker(string key)
		{
			_game = Core.Game;
			_instanceKey = key;
		}

		private BobsBuddyErrorState _errorState = BobsBuddyErrorState.None;

		private BobsBuddyState _state = BobsBuddyState.Initial;
		private BobsBuddyState State
		{
			get => _state;
			set
			{
				_state = value;
				DebugLog($"New State: {value}");
			}
		}

		public bool ShouldRun()
		{
			if(!Config.Instance.RunBobsBuddy || !_game.IsBattlegroundsMatch)
				return false;
			if(RemoteConfig.Instance.Data?.BobsBuddy?.Disabled ?? false)
				return false;
			if(_errorState == BobsBuddyErrorState.None)
			{
				var verStr = RemoteConfig.Instance.Data?.BobsBuddy?.MinRequiredVersion;
				if(Version.TryParse(verStr, out var requiredVersion))
				{
					if(requiredVersion > Helper.GetCurrentVersion())
					{
						DebugLog($"Update to {requiredVersion} required. Not running simulations.");
						_errorState = BobsBuddyErrorState.UpdateRequired;
						BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.UpdateRequired);
					}
				}
			}
			if(_errorState == BobsBuddyErrorState.UpdateRequired)
				return false;
			return true;
		}

		internal void HeroPowerTriggered(string heroPowerId)
		{
			if(_minionHeroPowerTrigger != null && _minionHeroPowerTrigger.HeroPowerId == heroPowerId)
				_minionHeroPowerTrigger.Tsc.SetResult(null);
		}

		public async void StartCombat()
		{
			try
			{
				if(!ShouldRun())
					return;
				DebugLog(_instanceKey);
				if(State >= BobsBuddyState.Combat)
				{
					DebugLog($"{_instanceKey} already in {State} state. Exiting");
					return;
				}
				State = BobsBuddyState.Combat;
				SnapshotBoardState(_game.GetTurnNumber());
				DebugLog($"{_instanceKey} Waiting for state changes...");
				await Task.Delay(StateChangeDelay);
				if(State != BobsBuddyState.Combat)
				{
					DebugLog($"{_instanceKey} no longer in combat: State={State}. Exiting");
					return;
				}
				DebugLog($"{_instanceKey} continuing...");

				if(HasErrorState())
					return;

				DebugLog("Setting UI state to combat...");
				BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
				BobsBuddyDisplay.HidePercentagesShowSpinners();

				if(_minionHeroPowerTrigger != null)
				{
					var minion = _minionHeroPowerTrigger.Minion;
					var start = DateTime.Now;
					DebugLog($"Waiting for hero power ({_minionHeroPowerTrigger.HeroPowerId}) trigger for {minion.minionName}...");
					var completedTask = await Task.WhenAny(_minionHeroPowerTrigger.Tsc.Task, Task.Delay(HeroPowerTriggerTimeout));
					var duration = (DateTime.Now - start).TotalMilliseconds;
					if(completedTask != _minionHeroPowerTrigger.Tsc.Task)
					{
						DebugLog($"Found no hero power trigger after {duration}ms. Resetting receivedHeroPower on {minion.minionName}");
						minion.receivesLichKingPower = false;
					}
					else
						DebugLog($"Found hero power trigger for {minion.minionName} after {duration}ms");
				}

				DebugLog("Running simulation...");
				var result = await RunSimulation();
				if(result == null)
				{
					DebugLog("Simulation returned no result. Exiting.");
					return;
				}

				if(result.simulationCount <= 500 && result.myExitCondition == Simulator.ExitConditions.Time)
				{
					DebugLog("Could not perform enough simulations. Displaying error state and exiting.");
					_errorState = BobsBuddyErrorState.NotEnoughData;
					BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.NotEnoughData);
				}
				else
				{
					DebugLog("Displaying simulation results");
					BobsBuddyDisplay.ShowCompletedSimulation(
						result.winRate,
						result.tieRate,
						result.lossRate,
						result.theirDeathRate,
						result.myDeathRate
					);
				}
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _debugLog);
				return;
			}
		}

		public void StartShopping(bool validateResults)
		{
			try
			{
				if(!ShouldRun())
					return;
				DebugLog(_instanceKey);
				if(State == BobsBuddyState.Shopping)
				{
					DebugLog($"{_instanceKey} already in shopping state. Exiting");
					return;
				}
				State = BobsBuddyState.Shopping;

				if(HasErrorState())
					return;

				DebugLog("Setting UI state to shopping");
				BobsBuddyDisplay.SetState(BobsBuddyState.Shopping);

				if(validateResults)
					ValidateSimulationResult();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _debugLog);
				return;
			}
		}

		private bool HasErrorState([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			if(_errorState == BobsBuddyErrorState.None)
				return false;
			BobsBuddyDisplay.SetErrorState(_errorState);
			DebugLog($"ErrorState={_errorState}");
			return true;
		}

		internal void UpdateAttackingEntities(Entity attacker, Entity defender)
		{
			if(attacker == null || !attacker.IsHero || defender == null || !defender.IsHero)
				return;
			DebugLog($"Updating entities with attacker={attacker.Card.Name}, defender={defender.Card.Name}");
			_defendingHero = defender;
			_attackingHero = attacker;
		}

		private bool IsUnknownCard(Entity e) => e?.Card.Id == Database.UnknownCardId;

		private void SnapshotBoardState(int turn)
		{
			DebugLog("Snapshotting board state...");
			var simulator = new Simulator();
			var input = new TestInput(simulator);

			if(_game.Player.Board.Any(IsUnknownCard) || _game.Opponent.Board.Any(IsUnknownCard))
			{
				_errorState = BobsBuddyErrorState.UnkownCards;
				DebugLog("Board has unknown cards. Exiting.");
				return;
			}
			if(_game.Opponent.Secrets.Any())
			{
				_errorState = BobsBuddyErrorState.SecretsNotSupported;
				DebugLog("Opponent has secrets in play. Exiting.");
				return;
			}

			input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();

			var oppHero = _game.Opponent.Board.FirstOrDefault(x => x.IsHero);
			var playerHero = _game.Player.Board.FirstOrDefault(x => x.IsHero);
			if(oppHero == null || playerHero == null)
			{
				DebugLog("Hero(es) could not be found. Exiting.");
				return;
			}
		
			input.SetHealths(playerHero.Health, oppHero.Health);
			if(input.opponentHealth <= 0)
			{
				input.opponentHealth = 1000;
			}
			var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			var opponentTechLevel = oppHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			input.SetTiers(playerTechLevel, opponentTechLevel);

			var playerHeroPower = _game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			var opponentHeroPower = _game.Opponent.Board.FirstOrDefault(x => x.IsHeroPower);
		
			input.SetPowerID(playerHeroPower?.CardId ?? "", opponentHeroPower?.CardId ?? "");
			
			input.SetHeroPower(HeroPowerUsed(playerHeroPower), HeroPowerUsed(opponentHeroPower));

			input.SetupSecretsFromDbfidList(_game.Player.Secrets.Select(x => x.Card.DbfIf).ToList());

			foreach(var m in GetOrderedMinions(_game.Player.Board).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
				m.AddToBackOfList(input.playerSide, simulator);

			foreach(var m in GetOrderedMinions(_game.Opponent.Board).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
			{
				m.AddToBackOfList(input.opponentSide, simulator);

				if(m.receivesLichKingPower)
					_minionHeroPowerTrigger = new MinionHeroPowerTrigger(m, RebornRite);
			}

			_input = input;
			_turn = turn;

			DebugLog("Successfully snapshotted board state");
		}

		private IEnumerable<Entity> GetAttachedEntities(int entityId)
			=> _game.Entities.Values
				.Where(x => x.IsAttachedTo(entityId) && (x.IsInPlay || x.IsInSetAside || x.IsInGraveyard))
				.Select(x => x.Clone());

		private async Task<TestOutput> RunSimulation()
		{
			DebugLog("Running simulations...");
			if(_input == null)
			{
				DebugLog("No input. Exiting.");
				return null;
			}

			try
			{
				DebugLog("----- Simulation Input -----");
				DebugLog($"Player: heroPower={_input.playerPowerID}, used={_input.heroPowerInfo?.PlayerActivatedPower}");
				foreach(var minion in _input.playerSide)
					DebugLog(minion.ToString());

				DebugLog($"Opponent: heroPower={_input.opponentPowerID}, used={_input.heroPowerInfo?.OpponentActivatedPower}");
				foreach(var minion in _input.opponentSide)
					DebugLog(minion.ToString());

				if(_input.secretsAndPriorities.Count() > 0)
				{
					DebugLog("Detected the following secrets");
					foreach(var s in _input.secretsAndPriorities)
						DebugLog(s.secret.ToString());
				}
				DebugLog("----- End of Input -----");

				DebugLog($"Running simulations with MaxIterations={Iterations} and ThreadCount={ThreadCount}...");

				var start = DateTime.Now;

				int timeAlloted = _input.playerSide.Count >= 6 || _input.opponentSide.Count >= 6 ? MaxTimeForComplexBoards : MaxTime;
				_output = await new SimulationRunner().SimulateMultiThreaded(_input, Iterations, ThreadCount, timeAlloted);

				DebugLog("----- Simulation Output -----");
				DebugLog($"Duration={(DateTime.Now - start).TotalMilliseconds}ms, " +
					$"ExitCondition={_output.myExitCondition}, " +
					$"Iterations={_output.simulationCount}");
				DebugLog($"WinRate={_output.winRate * 100}% " +
					$"(Lethal={_output.theirDeathRate * 100}%), " +
					$"TieRate={_output.tieRate * 100}%, " +
					$"LossRate={_output.lossRate * 100}% " +
					$"(Lethal={_output.myDeathRate * 100}%)");
				DebugLog("----- End of Output -----");

				return _output;
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _debugLog);
				return null;
			}
		}

		private CombatResult GetLastCombatResult()
		{
			if(_attackingHero == null)
				return CombatResult.Tie;
			if(_attackingHero.IsControlledBy(_game.Player.Id))
				return CombatResult.Win;
			if(_attackingHero.IsControlledBy(_game.Opponent.Id))
				return CombatResult.Loss;
			return CombatResult.Invalid;
		}

		private LethalResult GetLastLethalResult()
		{
			if(_defendingHero == null || _attackingHero == null)
				return LethalResult.NoOneDied;
			if(_attackingHero.Attack >= _defendingHero.Health)
			{
				if(_attackingHero.IsControlledBy(_game.Player.Id))
					return LethalResult.OpponentDied;
				else
					return LethalResult.FriendlyDied;
			}
			return LethalResult.NoOneDied;
		}

		private void ValidateSimulationResult()
		{
			DebugLog("Validating results...");
			if(_output == null)
			{
				DebugLog("_lastSimulationResult is null. Exiting");
				return;
			}

			// Akazamzarak hero power - secrets are currently not supported
			if(_input.opponentPowerID == NonCollectible.Neutral.PrestidigitationTavernBrawl)
			{
				DebugLog("Opponent was Akazamarak. Currently not reporting. Exiting.");
				return;
			}

			if(_output.simulationCount < MinimumSimulationsToReportSentry)
			{
				DebugLog("Did not complete enough simulations to report terminal cases. Exiting.");
				return;
			}

			var metricSampling = RemoteConfig.Instance.Data?.BobsBuddy?.MetricSampling ?? 0;

			DebugLog($"metricSampling={metricSampling}, reportErrors={ReportErrors}");

			if(!ReportErrors && metricSampling == 0)
			{
				DebugLog("Nothign to report. Exiting.");
				return;
			}

			var result = GetLastCombatResult();
			var lethalResult = GetLastLethalResult();

			DebugLog($"result={result}, lethalResult={lethalResult}");

			if(metricSampling > 0 && _rnd.NextDouble() < metricSampling)
				Influx.OnBobsBuddySimulationCompleted(result, _output, _turn);

			if(ReportErrors)
			{
				if(IsIncorrectCombatResult(result))
					AlertWithLastInputOutput(result.ToString());
				if(IsIncorrectLethalResult(lethalResult) && !OpposingKelThuzadDied(lethalResult))
					AlertWithLastInputOutput(lethalResult.ToString());
			}
		}

		private bool IsIncorrectCombatResult(CombatResult result)
			=> result == CombatResult.Tie && _output.tieRate == 0
			|| result == CombatResult.Win && _output.winRate == 0
			|| result == CombatResult.Loss && _output.lossRate == 0;

		private bool IsIncorrectLethalResult(LethalResult result)
			=> result == LethalResult.FriendlyDied && _output.myDeathRate == 0
			|| result == LethalResult.OpponentDied && _output.theirDeathRate == 0;

		private bool OpposingKelThuzadDied(LethalResult result)
			=> result == LethalResult.OpponentDied && _input.OpponentIsKelThuzad();

		private void AlertWithLastInputOutput(string result)
		{
			DebugLog($"Queueing alert... (valid input: {_input != null})");
			if(_input != null)
				Sentry.QueueBobsBuddyTerminalCase(_input, _output, result, _turn, _debugLog);
		}
	}
}
