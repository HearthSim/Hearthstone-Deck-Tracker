using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BobsBuddy;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils;
using BobsBuddy.Simulation;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.Extensions;

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
		private const int LichKingDelay = 2000;

		internal static int ThreadCount => Environment.ProcessorCount / 2;

		private readonly GameV2 _game;
		private readonly Random _rnd = new Random();

		private static BobsBuddyPanel BobsBuddyDisplay => Core.Overlay.BobsBuddyDisplay;
		private static bool ReportErrors => Remote.Config.Data?.BobsBuddy?.SentryReporting ?? false;

		private TestInput? _input;
		private int _turn;
		static int LogLinesKept = Remote.Config.Data?.BobsBuddy?.LogLinesKept ?? 100;
		public string OpponentCardId = "";
		public string PlayerCardId = "";
		private Entity? _attackingHero;
		private Entity? _defendingHero;
		public Entity? LastAttackingHero = null;
		public int LastAttackingHeroAttack;
		private static List<string> _recentHDTLog = new List<string>();
		private static Dictionary<int, Minion> _currentOpponentMinions = new Dictionary<int, Minion>();
		private static List<Entity> _currentOpponentSecrets = new List<Entity>();

		private static Guid _currentGameId;
		private static readonly Dictionary<string, BobsBuddyInvoker> _instances = new Dictionary<string, BobsBuddyInvoker>();
		private static readonly Regex _debuglineToIgnore = new Regex(@"\|(Player|Opponent|TagChangeActions)\.");
		private static bool _removedLichKingHeroPowerFromMinion = false;
		public static bool CanRemoveLichKing => true;
		private bool RunSimulationAfterCombat => _currentOpponentSecrets.Any();

		public static BobsBuddyInvoker GetInstance(Guid gameId, int turn, bool createInstanceIfNoneFound = true)
		{
			if(_currentGameId != gameId)
			{
				Log.Debug("New GameId. Clearing instances...");
				_instances.Clear();
			}
			_currentGameId = gameId;

			var key = $"{gameId}_{turn}";
			
			if(!_instances.TryGetValue(key, out var instance) && createInstanceIfNoneFound)
			{
				instance = new BobsBuddyInvoker(key);
				_instances[key] = instance;
			}
			return instance;
		}

		public void DebugLog(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			Log.Info(msg, memberName, sourceFilePath);
		}

		private readonly string _instanceKey;

		static BobsBuddyInvoker()
		{
			Log.OnLogLine += AddHDTLogLine;
		}

		static void AddHDTLogLine(string toLog)
		{
			if(_debuglineToIgnore.IsMatch(toLog))
				return;
			if(_recentHDTLog.Count >= LogLinesKept)
				_recentHDTLog.RemoveAt(0);
			_recentHDTLog.Add(toLog);
		}

		private BobsBuddyInvoker(string key)
		{
			_game = Core.Game;
			_instanceKey = key;
		}


		public TestOutput? Output { get; private set; }

		public BobsBuddyErrorState ErrorState { get; private set; }

		private BobsBuddyState _state;

		public BobsBuddyState State
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
			if(Remote.Config.Data?.BobsBuddy?.Disabled ?? false)
				return false;
			if(ErrorState == BobsBuddyErrorState.None)
			{
				var verStr = Remote.Config.Data?.BobsBuddy?.MinRequiredVersion;
				if(Version.TryParse(verStr, out var requiredVersion))
				{
					if(requiredVersion > Helper.GetCurrentVersion())
					{
						DebugLog($"Update to {requiredVersion} required. Not running simulations.");
						ErrorState = BobsBuddyErrorState.UpdateRequired;
						BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.UpdateRequired);
					}
				}
			}
			if(ErrorState == BobsBuddyErrorState.UpdateRequired)
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
				if(RunSimulationAfterCombat)
				{
					State = BobsBuddyState.CombatWithoutSimulation;
					BobsBuddyDisplay.SetState(BobsBuddyState.CombatWithoutSimulation);
				}
				else
					BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
				BobsBuddyDisplay.ResetText();

				_removedLichKingHeroPowerFromMinion = false;

				if(CanRemoveLichKing)
				{
					var lichKingMinions = new List<Minion>();
					var playerLichMinions = _input?.opponentSide.Where(x => x.receivesLichKingPower).ToList() ?? new List<Minion>();
					var opponentLichMinions = _input?.playerSide.Where(x => x.receivesLichKingPower).ToList() ?? new List<Minion>();
					lichKingMinions.AddRange(playerLichMinions);
					lichKingMinions.AddRange(opponentLichMinions);
					if(lichKingMinions.Any())
					{

						await Task.Delay(LichKingDelay);
						foreach(var minion in lichKingMinions)
						{
							if(_game.Entities.TryGetValue(minion.game_id, out var entity) && entity != null)
							{
								var attatchedEntities = GetAttachedEntities(minion.game_id);
								if(!attatchedEntities.Any(x => x.CardId == RebornRiteEnchmantment))
								{
									minion.receivesLichKingPower = false;
									_removedLichKingHeroPowerFromMinion = true;
								}

							}
						}
					}
					if(playerLichMinions.Any() && _input?.heroPowerInfo?.PlayerActivatedPower == HeroPower.None)
					{
						_removedLichKingHeroPowerFromMinion = true;
						foreach(var minion in playerLichMinions)
							minion.receivesLichKingPower = false;
					}
					if(opponentLichMinions.Any() && _input?.heroPowerInfo?.OpponentActivatedPower == HeroPower.None)
					{
						_removedLichKingHeroPowerFromMinion = true;
						foreach(var minion in opponentLichMinions)
							minion.receivesLichKingPower = false;
					}
				}

				if(!RunSimulationAfterCombat)
					RunAndDisplaySimulationAsync().Forget();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog);
				return;
			}
		}

		private async Task RunAndDisplaySimulationAsync()
		{
			_currentOpponentMinions.Clear();
			DebugLog("Running simulation...");
			BobsBuddyDisplay.HidePercentagesShowSpinners();
			var result = await RunSimulation();
			if(result == null)
			{
				DebugLog("Simulation returned no result. Exiting.");
				return;
			}

			if(result.simulationCount <= 500 && result.myExitCondition == Simulator.ExitConditions.Time)
			{
				DebugLog("Could not perform enough simulations. Displaying error state and exiting.");
				ErrorState = BobsBuddyErrorState.NotEnoughData;
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
					result.myDeathRate,
					result.result.Select(x => x.damage).ToList()
				);
			}
		}

		public async Task StartShoppingAsync(bool validateResults)
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

				BobsBuddyDisplay.SetLastOutcome(GetLastCombatDamageDealt());
				BobsBuddyDisplay.SetState(BobsBuddyState.Shopping);
				if(!RunSimulationAfterCombat)
				{
					DebugLog("Setting UI state to shopping");
				}
				else
				{
					await RunAndDisplaySimulationAsync();
				}

				if(validateResults)
					ValidateSimulationResultAsync().Forget();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog);
				return;
			}
		}

		private bool HasErrorState([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
		{
			if(ErrorState == BobsBuddyErrorState.None)
				return false;
			BobsBuddyDisplay.SetErrorState(ErrorState);
			DebugLog($"ErrorState={ErrorState}");
			return true;
		}

		private bool IsUnknownCard(Entity e) => e?.Card.Id == Database.UnknownCardId;


		internal void UpdateAttackingEntities(Entity attacker, Entity defender)
		{
			if(!attacker.IsHero || !defender.IsHero)
				return;
			DebugLog($"Updating entities with attacker={attacker.Card.Name}, defender={defender.Card.Name}");
			_defendingHero = defender;
			_attackingHero = attacker;
		}

		private void SnapshotBoardState(int turn)
		{
			DebugLog("Snapshotting board state...");
			LastAttackingHero = null;
			var simulator = new Simulator();
			var input = new TestInput(simulator);

			if(_game.Player.Board.Any(IsUnknownCard) || _game.Opponent.Board.Any(IsUnknownCard))
			{
				ErrorState = BobsBuddyErrorState.UnkownCards;
				DebugLog("Board has unknown cards. Exiting.");
				return;
			}

			input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();

			var livingHeroes = _game.Entities.Values.Where(x => x.IsHero && x.Health > 0 && !x.IsInZone(Zone.REMOVEDFROMGAME) && x.HasTag(GameTag.PLAYER_TECH_LEVEL) && (x.IsControlledBy(_game.Player.Id) || !x.IsInPlay));
			input.HeroHasDied = livingHeroes.Count() < _game.BattlegroundsHeroCount();

			var opponentHero = _game.Opponent.Board.FirstOrDefault(x => x.IsHero);
			var playerHero = _game.Player.Board.FirstOrDefault(x => x.IsHero);
			if(opponentHero == null || playerHero == null)
			{
				DebugLog("Hero(es) could not be found. Exiting.");
				return;
			}
		

			//We set OpponentCardId and PlayerCardId here so that later we can do lookups for these entites without using _game.Opponent/Player, which might be innacurate or null depending on when they're accessed.
			OpponentCardId = opponentHero.CardId ?? "";
			PlayerCardId = playerHero.CardId ?? "";

			input.SetHealths(playerHero.Health, opponentHero.Health);
			if(input.opponentHealth <= 0)
			{
				input.opponentHealth = 1000;
			}
			var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			var opponentTechLevel = opponentHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			input.SetTiers(playerTechLevel, opponentTechLevel);

			var playerHeroPower = _game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			var opponentHeroPower = _game.Opponent.Board.FirstOrDefault(x => x.IsHeroPower);
		
			input.SetPowerID(playerHeroPower?.CardId ?? "", opponentHeroPower?.CardId ?? "");
			
			input.SetHeroPower(HeroPowerUsed(playerHeroPower), HeroPowerUsed(opponentHeroPower));

			input.SetupSecretsFromDbfidList(_game.Player.Secrets.Select(x => x.Card.DbfIf).ToList());

			input.SetTurn(turn);

			_currentOpponentSecrets = _game.Opponent.Secrets.ToList();

			foreach(var m in GetOrderedMinions(_game.Player.Board).Where(e => e.IsControlledBy(_game.Player.Id)).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
				m.AddToBackOfList(input.playerSide, simulator);

			foreach(var m in GetOrderedMinions(_game.Opponent.Board).Select(e => GetMinionFromEntity(e, GetAttachedEntities(e.Id))))
			{
				m.AddToBackOfList(input.opponentSide, simulator);

				_currentOpponentMinions[m.game_id] = m;
			}

			_input = input;
			_turn = turn;

			DebugLog("Successfully snapshotted board state");
		}

		private IEnumerable<Entity> GetAttachedEntities(int entityId)
			=> _game.Entities.Values
				.Where(x => x.IsAttachedTo(entityId) && (x.IsInPlay || x.IsInSetAside || x.IsInGraveyard))
				.Select(x => x.Clone());

		private async Task<TestOutput?> RunSimulation()
		{
			DebugLog("Running simulations...");
			if(_input == null)
			{
				DebugLog("No input. Exiting.");
				return null;
			}

			try
			{
				if(RunSimulationAfterCombat)
				{
					_input.SetupSecretsFromDbfidList(_currentOpponentSecrets.Where(x => x != null && !string.IsNullOrEmpty(x.CardId)).Select(x => x.Card.DbfIf).ToList());
					_input.playerIsAkazamarak = false;
					DebugLog($"Set opponent to Akazamarak with {_input.secretsAndPriorities.Count} secrets.");
				}

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
				Output = await new SimulationRunner().SimulateMultiThreaded(_input, Iterations, ThreadCount, timeAlloted);

				DebugLog("----- Simulation Output -----");
				DebugLog($"Duration={(DateTime.Now - start).TotalMilliseconds}ms, " +
					$"ExitCondition={Output.myExitCondition}, " +
					$"Iterations={Output.simulationCount}");
				DebugLog($"WinRate={Output.winRate * 100}% " +
					$"(Lethal={Output.theirDeathRate * 100}%), " +
					$"TieRate={Output.tieRate * 100}%, " +
					$"LossRate={Output.lossRate * 100}% " +
					$"(Lethal={Output.myDeathRate * 100}%)");
				DebugLog("----- End of Output -----");

				return Output;
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog);
				return null;
			}
		}

		public void HandleNewAttackingEntity(Entity newAttacker)
		{
			if(newAttacker.IsHero)
			{
				LastAttackingHero = newAttacker;
				LastAttackingHeroAttack = newAttacker.Attack;
			}
		}

		private int GetLastCombatDamageDealt()
		{
			if(LastAttackingHero != null)
				return LastAttackingHeroAttack;
			return 0;	
		}

		private CombatResult GetLastCombatResult()
		{
			if(LastAttackingHero == null)
				return CombatResult.Tie;
			var playerHero = _game.Entities.Values.FirstOrDefault(x => x.CardId == PlayerCardId);
			var opponentHero = _game.Entities.Values.FirstOrDefault(x => x.CardId == OpponentCardId);
			if(playerHero != null && opponentHero != null)
			{
				if(LastAttackingHero.CardId == playerHero.CardId)
					return CombatResult.Win;
				if(LastAttackingHero.CardId == opponentHero.CardId)
					return CombatResult.Loss;
			}
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

		private async Task ValidateSimulationResultAsync()
		{
			DebugLog("Validating results...");
			if(Output == null)
			{
				DebugLog("_lastSimulationResult is null. Exiting");
				return;
			}

			if(Output.simulationCount < MinimumSimulationsToReportSentry)
			{
				DebugLog("Did not complete enough simulations to report terminal cases. Exiting.");
				return;
			}

			var metricSampling = Remote.Config.Data?.BobsBuddy?.MetricSampling ?? 0;

			DebugLog($"metricSampling={metricSampling}, reportErrors={ReportErrors}");

			if(!ReportErrors && metricSampling == 0)
			{
				DebugLog("Nothing to report. Exiting.");
				return;
			}
			
			//We delay checking the combat results because the tag changes can sometimes be read by the parser with a bit of delay after they're printed in the log.
			//Without this delay they can occasionally be missed.

			await Task.Delay(50);
			var result = GetLastCombatResult();
			var lethalResult = GetLastLethalResult();

			DebugLog($"result={result}, lethalResult={lethalResult}");

			var terminalCase = false;

			if (IsIncorrectCombatResult(result))
			{
				terminalCase = true;
				if (ReportErrors)
					AlertWithLastInputOutput(result.ToString());
			}

			if(IsIncorrectLethalResult(lethalResult) && !OpposingKelThuzadDied(lethalResult))
			{
				// Akazamzarak hero power - secrets are supported but not for lethal.
				if(_input?.opponentPowerID == NonCollectible.Neutral.PrestidigitationTavernBrawl)
				{
					DebugLog("Opponent was Akazamarak. Currently not reporting lethal results. Exiting.");
					return;
				}

				if(_turn > 5)
				{
					terminalCase = true;
					if(ReportErrors)
						AlertWithLastInputOutput(lethalResult.ToString());
				}
			}

			if (metricSampling > 0 && _rnd.NextDouble() < metricSampling)
				Influx.OnBobsBuddySimulationCompleted(result, Output, _turn, terminalCase, _removedLichKingHeroPowerFromMinion);
		}

		private bool IsIncorrectCombatResult(CombatResult result)
			=> result == CombatResult.Tie && Output?.tieRate == 0
			|| result == CombatResult.Win && Output?.winRate == 0
			|| result == CombatResult.Loss && Output?.lossRate == 0;

		private bool IsIncorrectLethalResult(LethalResult result)
			=> result == LethalResult.FriendlyDied && Output?.myDeathRate == 0
			|| result == LethalResult.OpponentDied && Output?.theirDeathRate == 0;

		private bool OpposingKelThuzadDied(LethalResult result)
			=> result == LethalResult.OpponentDied && _input != null && _input.OpponentIsKelThuzad();

		private void AlertWithLastInputOutput(string result)
		{
			DebugLog($"Queueing alert... (valid input: {_input != null})");
			if(_input != null && Output != null)
				Sentry.QueueBobsBuddyTerminalCase(_input, Output, result, _turn, _recentHDTLog, _game.CurrentRegion);
		}
	}
}
