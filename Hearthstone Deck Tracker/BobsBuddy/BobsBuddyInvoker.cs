
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
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal class BobsBuddyInvoker
	{
		private const int Iterations = 10_000;
		private const int StateChangeDelay = 500;
		internal static int ThreadCount => Environment.ProcessorCount / 2;

		private readonly GameV2 _game;
		private readonly Random _rnd = new Random();

		private static BobsBuddyPanel BobsBuddyDisplay => Core.Overlay.BobsBuddyDisplay;

		private Entity _attackingHero;
		private Entity _defendingHero;
		private TestOutput _output;
		private TestInput _input;
		private int _turn;
		private readonly List<string> _debugLog = new List<string>();

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
			_debugLog.Add($"{memberName} >> {msg}");
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
			if(!Config.Instance.RunBobsBuddy || _game.CurrentGameMode != GameMode.Battlegrounds)
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

				DebugLog("Running simulation...");
				var result = await RunSimulation();
				if(result == null)
				{
					DebugLog("Simulation returned no result. Exiting.");
					return;
				}

				if(result.simulationCount <= 500 && result.myExitCondition == Simulator.exitConditions.Time)
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

			var oppHero = _game.Opponent.Board.FirstOrDefault(x => x.IsHero);
			var playerHero = _game.Player.Board.FirstOrDefault(x => x.IsHero);
			if(oppHero == null || playerHero == null)
			{
				DebugLog("Hero(es) could not be found. Exiting.");
				return;
			}
		
			input.setHealths(playerHero.Health, oppHero.Health);
			if(input.opponentHealth <= 0)
			{
				input.opponentHealth = 1000;
			}
			var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);	
			var opponentTechLevel = oppHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			input.setTiers(playerTechLevel, opponentTechLevel);

			var playerHeroPower = _game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			var opponentHeroPower = _game.Opponent.Board.FirstOrDefault(x => x.IsHeroPower);
			var playerUsedHeroPower = playerHeroPower?.HasTag(GameTag.EXHAUSTED) ?? false;
			var opponentUsedHeroPower = opponentHeroPower?.HasTag(GameTag.EXHAUSTED) ?? false;
			input.playerPowerID = playerHeroPower?.CardId ?? "";
			input.opponentPowerID = opponentHeroPower?.CardId ?? "";

			input.setHeroPower(playerUsedHeroPower, opponentUsedHeroPower);

			input.setupSecretsFromDbfidList(_game.Player.Secrets.Select(x => x.Card.DbfIf).ToList());

			foreach(var m in GetOrderedMinions(_game.Player.Board).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
				m.addToBackOfList(input.mySide, simulator);

			foreach(var m in GetOrderedMinions(_game.Opponent.Board).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
				m.addToBackOfList(input.theirSide, simulator);

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
				DebugLog($"Player: heroPower={_input.playerPowerID}, used={_input.heroPowerInfo?.myUsedPower}");
				_input.mySide.ForEach(LogMinionStats);
				_input.mySide.ForEach(LogMinionDeathrattles);

				DebugLog($"Opponent: heroPower={_input.opponentPowerID}, used={_input.heroPowerInfo?.theirUsedPower}");
				_input.theirSide.ForEach(LogMinionStats);
				_input.theirSide.ForEach(LogMinionDeathrattles);

				if(_input.secretsAndPriorities.Count() > 0)
				{
					DebugLog("Detected the following secrets");
					foreach(var s in _input.secretsAndPriorities)
						DebugLog(s.secret.ToString());
				}
				DebugLog("----- End of Input -----");

				DebugLog($"Running simulations with MaxIterations={Iterations} and ThreadCount={ThreadCount}...");

				var start = DateTime.Now;
				_output = await Simulator.simulateMultiThreaded(_input, Iterations, ThreadCount);
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

			var metricSampling = RemoteConfig.Instance.Data?.BobsBuddy?.MetricSampling ?? 0;
			var reportErrors = RemoteConfig.Instance.Data?.BobsBuddy?.SentryReporting ?? false;

			DebugLog($"metricSampling={metricSampling}, reportErrors={reportErrors}");

			if(!reportErrors && metricSampling == 0)
			{
				DebugLog("Nothign to report. Exiting.");
				return;
			}

			var result = GetLastCombatResult();
			var lethalResult = GetLastLethalResult();

			DebugLog($"result={result}, lethalResult={lethalResult}");

			if(metricSampling > 0 && _rnd.NextDouble() < metricSampling)
				Influx.OnBobsBuddySimulationCompleted(result, _output, _turn);

			if(reportErrors)
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
			=> result == LethalResult.OpponentDied && _input.opponentPowerID == "";

		private void AlertWithLastInputOutput(string result)
		{
			DebugLog($"Queueing alert... (valid input: {_input != null})");
			if(_input != null)
				Sentry.QueueBobsBuddyTerminalCase(_input, _output, result, _turn, _debugLog);
		}
	}
}
