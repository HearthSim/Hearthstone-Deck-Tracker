using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.BobsBuddy.BobsBuddyUtils;
using BobsBuddy.Simulation;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;
using BobsBuddy;
using NuGet;
using Hearthstone_Deck_Tracker.Controls;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	public class BobsBuddyInvoker
	{
		private const int Iterations = 10_000;
		private const int StateChangeDelay = 500;
		private const int MaxTime = 1_500;
		private const int MaxTimeForComplexBoards = 3_000;
		private const int MinimumSimulationsToReportSentry = 2500;
		private const int LichKingDelay = 2000;

		internal static int ThreadCount => Environment.ProcessorCount / 2;

		private readonly GameV2 _game;
		private readonly Random _rnd = new Random();

		private static BobsBuddyPanel BobsBuddyDisplay => Core.Overlay.BobsBuddyDisplay;
		private static bool ReportErrors => Remote.Config.Data?.BobsBuddy?.SentryReporting ?? false;

		private Input? _input;
		public Output? combatResult;
		private float? _original_win_chance;
		private int _turn;
		static int LogLinesKept = Remote.Config.Data?.BobsBuddy?.LogLinesKept ?? 100;
		public string OpponentCardId = "";
		public string PlayerCardId = "";
		private Entity? _attackingHero;
		private Entity? _defendingHero;
		public Entity? LastAttackingHero = null;
		public int LastAttackingHeroAttack;
		private static List<string> _recentHDTLog = new List<string>();
		private static List<Entity> _currentOpponentSecrets = new List<Entity>();

		private static Guid _currentGameId;
		private static readonly Dictionary<string, BobsBuddyInvoker> _instances = new Dictionary<string, BobsBuddyInvoker>();
		private static readonly Regex _debuglineToIgnore = new Regex(@"\|(Player|Opponent|TagChangeActions)\.");
		// private bool RunSimulationAfterCombat => _currentOpponentSecrets.Any();
		private bool RunSimulationAfterCombat => false;

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


		public Output? Output { get; private set; }

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

		private async Task<IEnumerable<Minion>> GetBestSetup(
			IEnumerable<Minion> minions,
			IEnumerable<Minion> bestMinions,
			int numberOfIterations = 20,
			int currentIteration = 0,
			int winChance = 0,
			int medianDamage = 0,
			int originalWinChance = 0
			)
		{
			if(_input == null)
				return bestMinions;

			if(currentIteration >= numberOfIterations)
			{
				_input.playerSide.Clear();

				_input.playerSide.AddRange(bestMinions);
				await RunAndDisplaySimulationAsync();

				return bestMinions;
			}

			_input.playerSide.Clear();

			Random random = new Random();
			minions = minions.OrderBy(x => random.Next()).ToArray();

			_input.playerSide.AddRange(minions);

			if(currentIteration == 0 && combatResult != null)
				_original_win_chance = (int)Math.Ceiling(combatResult.winRate + combatResult.tieRate);

			await RunAndDisplaySimulationAsync();

			if(combatResult == null)
				return bestMinions;

			var newWinChance = (int)Math.Ceiling(combatResult.winRate + combatResult.tieRate);
			var newMedianDamage = (int)combatResult.medianDamage;

			if(newWinChance > winChance)
			{
				bestMinions = minions;
				medianDamage = newMedianDamage;
				winChance = newWinChance;
			}
			else if(newWinChance == winChance)
			{
				if(combatResult.medianDamage > medianDamage)
				{
					bestMinions = minions;
					medianDamage = newMedianDamage;
					winChance = newWinChance;
				}
			}

			return await GetBestSetup(minions, bestMinions, numberOfIterations, currentIteration + 1, winChance, medianDamage, originalWinChance);
		}

		// add this to button to retry

		// next prio would be clicking opponents on side to sim aginst their last board state

		// this is where im changing
		public async void Rerun(int opponentId = -1)
		{
			// add this to re-run in shopping?
			if(_input == null)
				return;

			// kinda working but sometimes numbers seem weird?

			// what if i re-run with every minion arrangement to find the best?

			// expanding on that, what if i make each character icon clickable
			// - each player has a "last board" saved
			//  - re-run my setup against that
			//  - re-run all orders of minions to find highest win%
			//  - show that comp somewhere

			// add comparison of win rate and avg damage vs now? to see if actually worth swapping
			// need to figure out how to display stuff
			// also make a button instead of hijacking ?

			var playerMinions = GetOrderedMinions(_game.Player.Board)
				.Where(e => e.IsControlledBy(_game.Player.Id))
				.Select(e => GetMinionFromEntity(_input.simulator.MinionFactory, true, e, GetAttachedEntities(e.Id)));

			// maybe add this back someday
			// var playerSide = await GetBestSetup(playerMinions, playerMinions, (int)(Math.Pow(playerMinions.Count(), 2)));

			if(combatResult == null)
				return;

			_input.playerSide.Clear();

			_input.playerSide.AddRange(playerMinions);

			DebugLog($"rerunning with player id {opponentId}");

			if(opponentId >= 0)
			{

				var entity = _game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) == opponentId + 1).FirstOrDefault();
				var state = _game.GetBattlegroundsBoardStateFor(entity.CardId);


				if(state != null && state.Entities.Any())
				{
					_input.opponentSide.Clear();
					_original_win_chance = null;


					foreach(var e in state.Entities)
						if(e.CardId != null)
						{
							DebugLog($"minion to string {e.ToString()}");

							// getting closer... I am down here but it is not adding any minions

							// set opponent hero power somehow here too

							var minion = GetMinionFromEntity(_input.simulator.MinionFactory, false, e, GetAttachedEntities(e.Id));
							_input.opponentSide.Add(minion);
						}

				}

			}

			await RunAndDisplaySimulationAsync();

			DebugLog($"win+tie% before: {_original_win_chance * 100}%");
			DebugLog($"win+tie% after: {(int)Math.Ceiling(combatResult.winRate + combatResult.tieRate) * 100}%");

			// DebugLog($"best order:");

			// for(int index = 0; index < playerSide.Count(); index++)
			// 	DebugLog($"{index + 1} {playerSide.ElementAt(index).minionName}");
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
				_original_win_chance = null;
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

				if(_input != null && ((_input.PlayerHeroPower.CardId == RebornRite && _input.PlayerHeroPower.IsActivated) || (_input.OpponentHeroPower.CardId == RebornRite && _input.OpponentHeroPower.IsActivated)))
					await Task.Delay(LichKingDelay);

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
				var oldWinChance = _original_win_chance;
				_original_win_chance = result.winRate + result.tieRate;
				DebugLog($"{oldWinChance} {_original_win_chance}");

				combatResult = result;
				DebugLog($"oldCombatResult: {_original_win_chance}");
				DebugLog($"combatResult: {combatResult?.winRate + combatResult?.tieRate}");
				BobsBuddyDisplay.ShowCompletedSimulation(
					result.winRate,
					result.tieRate,
					result.lossRate,
					result.theirDeathRate,
					result.myDeathRate,
					result.damageResults.ToList(),
					oldWinChance,
					result.winRate + result.tieRate
				);
			}
		}

		public async Task StartShoppingAsync()
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

				BobsBuddyDisplay.SetInvoker(this);

				// maybe don't do this v
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
			var input = new Input(simulator);

			if(_game.Player.Board.Any(IsUnknownCard) || _game.Opponent.Board.Any(IsUnknownCard))
			{
				ErrorState = BobsBuddyErrorState.UnkownCards;
				DebugLog("Board has unknown cards. Exiting.");
				return;
			}

			input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();
			input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);

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

			input.SetHealths(playerHero.Health + playerHero.GetTag(GameTag.ARMOR), opponentHero.Health + opponentHero.GetTag(GameTag.ARMOR));

			if(input.opponentHealth <= 0)
			{
				input.opponentHealth = 1000;
			}
			var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			var opponentTechLevel = opponentHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			input.SetTiers(playerTechLevel, opponentTechLevel);

			var playerHeroPower = _game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			input.SetPlayerHeroPower(playerHeroPower?.CardId ?? "", WasHeroPowerActivated(playerHeroPower), playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0);

			var opponentHeroPower = _game.Opponent.Board.FirstOrDefault(x => x.IsHeroPower);
			input.SetOpponentHeroPower(opponentHeroPower?.CardId ?? "", WasHeroPowerActivated(opponentHeroPower), opponentHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0);

			foreach(var quest in _game.Player.Quests)
			{
				var rewardDbfId = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
				var reward = Database.GetCardFromDbfId(rewardDbfId, false);
				input.PlayerQuests.Add(new QuestData()
				{
					QuestProgress = quest.GetTag(GameTag.QUEST_PROGRESS),
					QuestProgressTotal = quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL),
					QuestCardId = quest.CardId ?? "",
					RewardCardId = reward?.Id ?? ""
				});
			}

			foreach(var reward in _game.Player.QuestRewards)
			{
				input.PlayerQuests.Add(new QuestData()
				{
					RewardCardId = reward.Info.LatestCardId ?? ""
				});
			}

			foreach(var quest in _game.Opponent.Quests)
			{
				var rewardDbfId = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
				var reward = Database.GetCardFromDbfId(rewardDbfId, false);
				input.OpponentQuests.Add(new QuestData()
				{
					QuestProgress = quest.GetTag(GameTag.QUEST_PROGRESS),
					QuestProgressTotal = quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL),
					QuestCardId = quest.CardId ?? "",
					RewardCardId = reward?.Id ?? ""
				});
			}

			foreach(var reward in _game.Opponent.QuestRewards)
			{
				input.OpponentQuests.Add(new QuestData()
				{
					RewardCardId = reward.Info.LatestCardId ?? ""
				});
			}

			input.SetPlayerHandSize(_game.Player.HandCount);

			input.SetupSecretsFromDbfidList(_game.Player.Secrets.Select(x => x.Card.DbfId).ToList(), true);

			input.SetTurn(turn);

			_currentOpponentSecrets = _game.Opponent.Secrets.ToList();

			var playerSide = GetOrderedMinions(_game.Player.Board)
				.Where(e => e.IsControlledBy(_game.Player.Id))
				.Select(e => GetMinionFromEntity(simulator.MinionFactory, true, e, GetAttachedEntities(e.Id)));
			foreach(var m in playerSide)
				input.playerSide.Add(m);

			var opponentSide = GetOrderedMinions(_game.Opponent.Board)
				.Where(e => e.IsControlledBy(_game.Opponent.Id))
				.Select(e => GetMinionFromEntity(simulator.MinionFactory, false, e, GetAttachedEntities(e.Id)));
			foreach(var m in opponentSide)
			{
				input.opponentSide.Add(m);
			}

			_input = input;
			_turn = turn;

			DebugLog("Successfully snapshotted board state");
		}

		private IEnumerable<Entity> GetAttachedEntities(int entityId)
			=> _game.Entities.Values
				.Where(x => x.IsAttachedTo(entityId) && (x.IsInPlay || x.IsInSetAside || x.IsInGraveyard))
				.Select(x => x.Clone());

		private async Task<Output?> RunSimulation()
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
					_input.SetupSecretsFromDbfidList(_currentOpponentSecrets.Where(x => x != null && !string.IsNullOrEmpty(x.CardId)).Select(x => x.Card.DbfId).ToList(), false);
					DebugLog($"Set opponent to Akazamarak with {_input.OpponentSecrets.Count} secrets.");
				}

				DebugLog("----- Simulation Input -----");
				DebugLog($"Player: heroPower={_input.PlayerHeroPower.CardId}, used={_input.PlayerHeroPower.IsActivated}");
				foreach(var minion in _input.playerSide)
					DebugLog(minion.ToString());

				foreach(var quest in _input.PlayerQuests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");

				DebugLog("---");
				DebugLog($"Opponent: heroPower={_input.OpponentHeroPower.CardId}, used={_input.OpponentHeroPower.IsActivated}");
				foreach(var minion in _input.opponentSide)
					DebugLog(minion.ToString());

				foreach(var quest in _input.OpponentQuests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");


				if(_input.PlayerSecrets.Count() > 0)
				{
					DebugLog("Detected the following player secrets");
					foreach(var s in _input.PlayerSecrets)
						DebugLog(s.ToString());
				}

				if(_input.OpponentSecrets.Count() > 0)
				{
					DebugLog("Detected the following opponent secrets");
					foreach(var s in _input.OpponentSecrets)
						DebugLog(s.ToString());
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
			var totalDefenderHealth = _defendingHero.Health + _defendingHero.GetTag(GameTag.ARMOR);
			if(_attackingHero.Attack >= totalDefenderHealth)
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

			if(lethalResult == LethalResult.FriendlyDied && (_game.CurrentGameStats?.WasConceded ?? false))
			{
				DebugLog($"Game was conceded. Not reporting.");
				return;
			}

			var terminalCase = false;

			if(IsIncorrectCombatResult(result))
			{
				terminalCase = true;
				if(ReportErrors)
					AlertWithLastInputOutput(result.ToString());
			}

			if(IsIncorrectLethalResult(lethalResult) && !OpposingKelThuzadDied(lethalResult))
			{
				// Akazamzarak hero power - secrets are supported but not for lethal.
				if(_input?.OpponentHeroPower.CardId == NonCollectible.Neutral.PrestidigitationTavernBrawl)
				{
					DebugLog("Opponent was Akazamarak. Currently not reporting lethal results. Exiting.");
					return;
				}

				// There should never be relevant lethals this early in the game.
				// These missed lethals are likely caused by some bug.
				if(_turn <= 5)
				{
					DebugLog($"There should not be missed lethals on turn ${_turn}, this is probably a bug. This won't be reported.");
					return;
				}

				terminalCase = true;
				if(ReportErrors)
					AlertWithLastInputOutput(lethalResult.ToString());
			}

			if(metricSampling > 0 && _rnd.NextDouble() < metricSampling)
				Influx.OnBobsBuddySimulationCompleted(result, Output, _turn, terminalCase);
		}

		private bool IsIncorrectCombatResult(CombatResult result)
			=> result == CombatResult.Tie && Output?.tieRate == 0
			|| result == CombatResult.Win && Output?.winRate == 0
			|| result == CombatResult.Loss && Output?.lossRate == 0;

		private bool IsIncorrectLethalResult(LethalResult result)
			=> result == LethalResult.FriendlyDied && Output?.myDeathRate == 0
			|| result == LethalResult.OpponentDied && Output?.theirDeathRate == 0;

		private bool OpposingKelThuzadDied(LethalResult result)
			=> result == LethalResult.OpponentDied && _input != null && _input.OpponentHeroPower.CardId == HeroPowerIds.KelThuzad;

		private void AlertWithLastInputOutput(string result)
		{
			DebugLog($"Queueing alert... (valid input: {_input != null})");
			if(_input != null && Output != null)
				Sentry.QueueBobsBuddyTerminalCase(_input, Output, result, _turn, _recentHDTLog, _game.CurrentRegion);
		}
	}
}
