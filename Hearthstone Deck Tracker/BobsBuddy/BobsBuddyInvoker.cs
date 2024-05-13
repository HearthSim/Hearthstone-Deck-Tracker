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

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal class BobsBuddyInvoker
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
		private static bool ReportErrors
		{
			get
			{
				var verStr = Remote.Config.Data?.BobsBuddy?.SentryMinRequiredVersion ?? string.Empty;
				if(Version.TryParse(verStr, out var requiredVersion))
					return Helper.GetCurrentVersion() >= requiredVersion;
				return false;
			}
		}

		private Input? _input;
		private int _turn;
		static int LogLinesKept = Remote.Config.Data?.BobsBuddy?.LogLinesKept ?? 100;
		private Entity? _attackingHero;
		private Entity? _defendingHero;
		public Entity? LastAttackingHero = null;
		public int LastAttackingHeroAttack;
		private static List<string> _recentHDTLog = new List<string>();

		private List<Entity> _opponentHand = new();
		private readonly Dictionary<Entity, Entity> _opponentHandMap = new();

		private static Guid _currentGameId;
		private static readonly Dictionary<string, BobsBuddyInvoker> _instances = new Dictionary<string, BobsBuddyInvoker>();
		private static readonly Regex _debuglineToIgnore = new Regex(@"\|(Player|Opponent|TagChangeActions)\.");

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
			if(!Config.Instance.RunBobsBuddy || !_game.IsBattlegroundsSoloMatch)
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
				BobsBuddyDisplay.SetState(BobsBuddyState.Combat);
				BobsBuddyDisplay.ResetText();

				if(_input != null && ((_input.PlayerHeroPower.CardId == RebornRite && _input.PlayerHeroPower.IsActivated) || (_input.OpponentHeroPower.CardId == RebornRite && _input.OpponentHeroPower.IsActivated)))
					await Task.Delay(LichKingDelay);

				await RunAndDisplaySimulationAsync();
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
				BobsBuddyDisplay.ShowCompletedSimulation(
					result.winRate,
					result.tieRate,
					result.lossRate,
					result.theirDeathRate,
					result.myDeathRate,
					result.damageResults.ToList()
				);
			}
		}

		public async Task StartShoppingAsync(bool isGameOver = false)
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
				if(isGameOver)
				{
					BobsBuddyDisplay.SetState(BobsBuddyState.GameOver);
					DebugLog("Setting UI state to GameOver");
				}
				else
				{
					BobsBuddyDisplay.SetState(BobsBuddyState.Shopping);
					DebugLog("Setting UI state to shopping");
				}

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

		private bool IsUnsupportedCard(Entity e) =>
			e.Card.Id == NonCollectible.Neutral.ProfessorPutricide_Festergut1 || e.Card.Id == NonCollectible.Neutral.ProfessorPutricide_Festergut2
			|| e.Card.Id == NonCollectible.Neutral.Sneed_PilotedWhirlOTron1 || e.Card.Id == NonCollectible.Neutral.Sneed_PilotedWhirlOTron2;


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
			var input = new Input();

			if(_game.Player.Board.Any(IsUnknownCard) || _game.Opponent.Board.Any(IsUnknownCard))
			{
				ErrorState = BobsBuddyErrorState.UnkownCards;
				DebugLog("Board has unknown cards. Exiting.");
				return;
			}

			if(_game.Player.Board.Any(IsUnsupportedCard) || _game.Opponent.Board.Any(IsUnsupportedCard))
			{
				ErrorState = BobsBuddyErrorState.UnsupportedCards;
				DebugLog("Board has unsupported cards. Exiting.");
				return;
			}

			if(_game.GameEntity == null)
			{
				DebugLog("GameEntity could not be found. Exiting.");
				return;
			}

			if(_game.PlayerEntity == null)
			{
				DebugLog("PlayerEntity could not be found. Exiting.");
				return;
			}

			if(_game.OpponentEntity == null)
			{
				DebugLog("OpponentEntity could not be found. Exiting.");
				return;
			}

			input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();
			if(_game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP_ENABLED) > 0)
				input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);

			var friendlyMurky = _game.Player.Board.FirstOrDefault(e => e.CardId == NonCollectible.Neutral.Murky);
			var friendlyMurkyBuff = friendlyMurky?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			input.PlayerBattlecriesPlayed = friendlyMurky != null && friendlyMurkyBuff > 0
				? friendlyMurkyBuff / (friendlyMurky.HasTag(GameTag.PREMIUM) ? 2 : 1) - 1
				: 0;

			var opponentMurky = _game.Opponent.Board.FirstOrDefault(e => e.CardId == NonCollectible.Neutral.Murky);
			var opponentMurkyBuff = opponentMurky?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			input.OpponentBattlecriesPlayed = opponentMurky != null && opponentMurkyBuff > 0
				? opponentMurkyBuff / (opponentMurky.HasTag(GameTag.PREMIUM) ? 2 : 1) - 1
				: 0;

			var opponentHero = _game.Opponent.Hero;
			var playerHero = _game.Player.Hero;
			if(opponentHero == null || playerHero == null)
			{
				DebugLog("Hero(es) could not be found. Exiting.");
				return;
			}

			input.SetHealths(playerHero.Health + playerHero.GetTag(GameTag.ARMOR), opponentHero.Health + opponentHero.GetTag(GameTag.ARMOR));

			if(input.opponentHealth <= 0)
			{
				input.opponentHealth = 1000;
			}

			input.PlayerDamageTaken = playerHero.GetTag(GameTag.DAMAGE);
			input.OpponentDamageTaken = opponentHero.GetTag(GameTag.DAMAGE);

			var playerTechLevel = playerHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			var opponentTechLevel = opponentHero.GetTag(GameTag.PLAYER_TECH_LEVEL);
			input.SetTiers(playerTechLevel, opponentTechLevel);

			var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
			var anomalyCardId = anomalyDbfId.HasValue ? Database.GetCardFromDbfId(anomalyDbfId.Value, false)?.Id : null;
			if(anomalyCardId != null)
				input.Anomaly = simulator.AnomalyFactory.Create(anomalyCardId);

			var playerHeroPower = _game.Player.Board.FirstOrDefault(x => x.IsHeroPower);
			var pHpData = playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			var pHpData2 = playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2) ?? 0;
			if(playerHeroPower?.CardId == NonCollectible.Neutral.TeronGorefiend_RapidReanimation)
			{
				var minionsInPlay = _game.Player.Board.Where(e => e.IsMinion && e.IsControlledBy(_game.Player.Id)).Select(x => x.Id);
				var attachedToEntityId = _game.Player.PlayerEntities
					.Where(x => x.CardId == NonCollectible.Neutral.TeronGorefiend_ImpendingDeath && x.IsInPlay)
					.Select(x => x.GetTag(GameTag.ATTACHED))
					.FirstOrDefault(x => minionsInPlay.Any(y => y == x));
				if(attachedToEntityId > 0)
					pHpData = attachedToEntityId;
			}
			input.SetPlayerHeroPower(playerHeroPower?.CardId ?? "", WasHeroPowerActivated(playerHeroPower), pHpData, pHpData2);

			var opponentHeroPower = _game.Opponent.Board.FirstOrDefault(x => x.IsHeroPower);
			var oHpData = opponentHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			var oHpData2 = opponentHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2) ?? 0;
			if(opponentHeroPower?.CardId == NonCollectible.Neutral.TeronGorefiend_RapidReanimation)
			{
				var minionsInPlay = _game.Opponent.Board.Where(e => e.IsMinion && e.IsControlledBy(_game.Opponent.Id)).Select(x => x.Id);
				var attachedToEntityId = _game.Opponent.PlayerEntities
					.Where(x => x.CardId == NonCollectible.Neutral.TeronGorefiend_ImpendingDeath)
					.Select(x => x.GetTag(GameTag.ATTACHED))
					.FirstOrDefault(x => minionsInPlay.Any(y => y == x));
				if(attachedToEntityId > 0)
					oHpData = attachedToEntityId;
			}
			input.SetOpponentHeroPower(opponentHeroPower?.CardId ?? "", WasHeroPowerActivated(opponentHeroPower), oHpData, oHpData2);

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

			foreach(var objective in _game.Player.Objectives)
			{
				input.PlayerObjectives.Add(GetObjectiveFromEntity(simulator.ObjectiveFactory, true, objective));
			}

			foreach(var objective in _game.Opponent.Objectives)
			{
				input.OpponentObjectives.Add(GetObjectiveFromEntity(simulator.ObjectiveFactory, false, objective));
			}

			input.SetupSecretsFromDbfidList(_game.Player.Secrets.Select(x => (int?)x.Card.DbfId).ToList(), true);

			input.SetTurn(turn);

			var playerSide = GetOrderedMinions(_game.Player.Board)
				.Where(e => e.IsControlledBy(_game.Player.Id))
				.Select(e => GetMinionFromEntity(simulator.MinionFactory, true, e, GetAttachedEntities(e.Id)));
			foreach(var m in playerSide)
				input.playerSide.Add(m);

			foreach(var e in _game.Player.Hand)
			{
				if(e.IsMinion)
				{
					var minionEntity = new MinionCardEntity(GetMinionFromEntity(simulator.MinionFactory, true, e, GetAttachedEntities(e.Id)), null, simulator)
					{
						CanSummon = !e.HasTag(GameTag.LITERALLY_UNPLAYABLE),
					};
					input.PlayerHand.Add(minionEntity);
				}
				else if(e.CardId == NonCollectible.Neutral.BloodGem1)
					input.PlayerHand.Add(new BloodGem(null, simulator));
				else if(e.IsSpell)
					input.PlayerHand.Add(new SpellCardEntity(null, simulator));
				else
					input.PlayerHand.Add(new CardEntity(e.CardId ?? "", null, simulator)); // Not Unknown
			}

			var opponentSide = GetOrderedMinions(_game.Opponent.Board)
				.Where(e => e.IsControlledBy(_game.Opponent.Id))
				.Select(e => GetMinionFromEntity(simulator.MinionFactory, false, e, GetAttachedEntities(e.Id)));
			foreach(var m in opponentSide)
				input.opponentSide.Add(m);

			_opponentHand = _game.Opponent.Hand.ToList();
			input.OpponentHand.Clear();
			input.OpponentHand.AddRange(GetOpponentHandEntities(simulator));

			var playerAttached = GetAttachedEntities(_game.PlayerEntity.Id);
			var pEternalLegion = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Invalid.EternalKnight_EternalKnightPlayerEnchant);
			if(pEternalLegion != null)
				input.PlayerEternalKnightCounter = pEternalLegion.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			var pUndeadBonus = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Neutral.NerubianDeathswarmer_UndeadBonusAttackPlayerEnchantDnt);
			if(pUndeadBonus != null)
				input.PlayerUndeadAttackBonus = pUndeadBonus.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			input.PlayerElementalPlayCounter = _game.PlayerEntity.GetTag((GameTag)2878);

			var opponentAttached = GetAttachedEntities(_game.OpponentEntity.Id);
			var oEternalLegion = opponentAttached.FirstOrDefault(x => x.CardId == NonCollectible.Invalid.EternalKnight_EternalKnightPlayerEnchant);
			if(oEternalLegion != null)
				input.OpponentEternalKnightCounter = oEternalLegion.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			var oUndeadBonus = opponentAttached.FirstOrDefault(x => x.CardId == NonCollectible.Neutral.NerubianDeathswarmer_UndeadBonusAttackPlayerEnchantDnt);
			if(oUndeadBonus != null)
				input.OpponentUndeadAttackBonus = oUndeadBonus.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			input.OpponentElementalPlayCounter = _game.OpponentEntity.GetTag((GameTag)2878);

			Log.Info($"pEternal={input.PlayerEternalKnightCounter}, pUndead={input.PlayerUndeadAttackBonus}, pElemental={input.PlayerElementalPlayCounter} | oEternal={input.OpponentEternalKnightCounter}, oUndead={input.OpponentUndeadAttackBonus}, oElemental={input.OpponentElementalPlayCounter}");

			input.PlayerBloodGemAtkBuff = _game.PlayerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFATKVALUE);
			input.PlayerBloodGemHealthBuff = _game.PlayerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFHEALTHVALUE);
			input.OpponentBloodGemAtkBuff =_game.OpponentEntity.GetTag(GameTag.BACON_BLOODGEMBUFFATKVALUE);
			input.OpponentBloodGemHealthBuff = _game.OpponentEntity.GetTag(GameTag.BACON_BLOODGEMBUFFHEALTHVALUE);

			Log.Info($"pBloodGem=+{input.PlayerBloodGemAtkBuff}/+{input.PlayerBloodGemHealthBuff}, oBloodGem=+{input.OpponentBloodGemAtkBuff}/+{input.OpponentBloodGemHealthBuff}");

			_input = input;
			_turn = turn;

			DebugLog("Successfully snapshotted board state");
		}

		private int _reRunCount;

		private async Task TryRerun()
		{
			if(_reRunCount++ <= 10)
			{
				DebugLog($"Input changed, re-running simulation! (#{_reRunCount})");
				if(ShouldRun())
				{
					var expandAfterError = ErrorState == BobsBuddyErrorState.None && Config.Instance.ShowBobsBuddyDuringCombat;
					ErrorState = BobsBuddyErrorState.None;
					BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.None, null, BobsBuddyDisplay.ResultsPanelExpanded || expandAfterError);
					await RunAndDisplaySimulationAsync();
				}
			}
			else
				DebugLog("Input changed, but the simulation already re-ran ten times");
		}

		internal async void UpdateOpponentHand(Entity entity, Entity copy)
		{
			if(_input == null || State != BobsBuddyState.Combat)
				return;

			// Only allow feathermane for now.
			if(copy.CardId != NonCollectible.Neutral.FreeFlyingFeathermane && copy.CardId != NonCollectible.Neutral.FreeFlyingFeathermane_FreeFlyingFeathermane)
				return;

			_opponentHandMap[entity] = copy;

			// Wait for attached entities to be logged. This should happen at the exact same timestamp.
			//await _game.GameTime.WaitForDuration(1);

			var entities = GetOpponentHandEntities(new Simulator()).ToList();
			if(entities.Count(x => x is MinionCardEntity) <= _input.OpponentHand.Count(x => x is MinionCardEntity))
				return;

			_input.OpponentHand.Clear();
			_input.OpponentHand.AddRange(entities);

			await TryRerun();
		}

		internal async void UpdateOpponentSecret(Entity entity)
		{
			await TryRerun();
		}

		private IEnumerable<CardEntity> GetOpponentHandEntities(Simulator simulator)
		{
			foreach(var _e in _opponentHand)
			{
				var e = _opponentHandMap.TryGetValue(_e, out var copy) ? copy : _e;
				if(e.IsMinion)
				{
					var attached = GetAttachedEntities(e.Id);
					yield return new MinionCardEntity(GetMinionFromEntity(simulator.MinionFactory, false, e, attached), null, simulator)
					{
						CanSummon = !e.HasTag(GameTag.LITERALLY_UNPLAYABLE)
					};
				}
				else if(e.CardId == NonCollectible.Neutral.BloodGem1)
					yield return new BloodGem(null, simulator);
				else if(e.IsSpell)
					yield return new SpellCardEntity(null, simulator);
				else if(!string.IsNullOrEmpty(e.CardId))
					yield return new CardEntity(e.CardId ?? "", null, simulator); // Not Unknown
				else
					yield return new UnknownCardEntity(null, simulator);
			}
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
				_input.SetupSecretsFromDbfidList(
					_game.Opponent.Secrets
						.Select(x => !string.IsNullOrEmpty(x.CardId) ? (int?)x.Card.DbfId : null)
						.Distinct(new SecretDbfIdComparer())
						.ToList(),
					false
				);
				DebugLog($"Set opponent S. with {_input.OpponentSecrets.Count} S.");

				DebugLog("----- Simulation Input -----");
				DebugLog($"Player: heroPower={_input.PlayerHeroPower.CardId}, used={_input.PlayerHeroPower.IsActivated}, data={_input.PlayerHeroPower.Data}");
				DebugLog($"Hand: {string.Join(", ",_input.PlayerHand.Select(x => x.ToString()))}");

				foreach(var minion in _input.playerSide)
					DebugLog(minion.ToString());

				foreach(var quest in _input.PlayerQuests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");

				DebugLog("---");
				DebugLog($"Opponent: heroPower={_input.OpponentHeroPower.CardId}, used={_input.OpponentHeroPower.IsActivated}, data={_input.OpponentHeroPower.Data}");
				DebugLog($"Hand: {string.Join(", ",_input.OpponentHand.Select(x => x.ToString()))}");
				foreach(var minion in _input.opponentSide)
					DebugLog(minion.ToString());

				foreach(var quest in _input.OpponentQuests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");


				if(_input.PlayerSecrets.Any())
				{
					DebugLog("Detected the following player S.");
					foreach(var s in _input.PlayerSecrets)
						DebugLog(s.ToString());
				}

				if(_input.OpponentSecrets.Any())
				{
					DebugLog("Detected the following opponent S.");
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
			catch(AggregateException aggregateEx)
			{
				if(aggregateEx.InnerExceptions.FirstOrDefault(x => x is UnsupportedInteractionException) is not UnsupportedInteractionException ex)
					throw;
				DebugLog($"Unsupported interaction: {ex.Entity?.ToString()}: {ex.Message}");
				Log.Error(ex);
				var cardName = Database.GetCardFromId(ex.Entity?.cardID)?.LocalizedName;
				var message = (cardName != null ? $"{cardName}: " : "") + ex.Message;
				BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.UnsupportedInteraction, message);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(ex, _input, _turn, _recentHDTLog);
				Output = null;
				return null;
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog);
				Output = null;
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
			if(LastAttackingHero.IsControlledBy(_game.Player.Id))
				return CombatResult.Win;
			else
				return CombatResult.Loss;
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

			if (IsIncorrectCombatResult(result))
			{
				terminalCase = true;
				if (ReportErrors)
					AlertWithLastInputOutput(result.ToString());
			}

			if(IsIncorrectLethalResult(lethalResult) && !OpposingKelThuzadDied(lethalResult))
			{
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

			if (metricSampling > 0 && _rnd.NextDouble() < metricSampling)
				Influx.OnBobsBuddySimulationCompleted(result, Output, _turn, _input?.Anomaly, terminalCase);
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

		/**
		 * A comparer that keeps unknown secrets (null) and de-duplicates dbf ids otherwise.
		 * For example { 1, null, 3, 3, null} will be deduplicated to {1, null, 3, null}.
		 */
		private class SecretDbfIdComparer : IEqualityComparer<int?>
		{
			public bool Equals(int? x, int? y)
			{
				if (x == null || y == null)
				{
					return false;
				}

				return x == y;
			}

			public int GetHashCode(int? obj)
			{
				if (obj == null)
				{
					return 0;
				}
				return obj.GetHashCode();
			}
		}
	}
}
