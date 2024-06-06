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
using BobsBuddyPlayer = BobsBuddy.Simulation.Player;

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

		private BobsBuddyPlayer DuosInputPlayer = new BobsBuddyPlayer(null);
		private BobsBuddyPlayer DuosInputOpponent = new BobsBuddyPlayer(null);
		private BobsBuddyPlayer? DuosInputPlayerTeammate;
		private BobsBuddyPlayer? DuosInputOpponentTeammate;

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

		private bool DoNotReport { get; set; } = true;

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
			if(!Config.Instance.RunBobsBuddy)
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
				if(_game.IsBattlegroundsDuosMatch)
				{
					SnapshotBoardState(_game.GetTurnNumber());

					BobsBuddyDisplay.SetState(BobsBuddyState.WaitingForTeammates);
					BobsBuddyDisplay.ResetText();

					if(_input != null && (DuosInputPlayerTeammate == null || DuosInputOpponentTeammate == null))
					{
						DebugLog("Waiting Teammates. Exiting.");
						return;
					}
				}
				else if(State >= BobsBuddyState.Combat)
				{
					DebugLog($"{_instanceKey} already in {State} state. Exiting");
					return;
				} else
					SnapshotBoardState(_game.GetTurnNumber());
				
				
				State = BobsBuddyState.Combat;
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

				if(_input != null && ((_input.Player.HeroPower.CardId == RebornRite && _input.Player.HeroPower.IsActivated) || (_input.Opponent.HeroPower.CardId == RebornRite && _input.Opponent.HeroPower.IsActivated)))
					await Task.Delay(LichKingDelay);

				await RunAndDisplaySimulationAsync();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog, _game.IsBattlegroundsDuosMatch);
				return;
			}
		}

		public async void MaybeRunDuosPartialCombat()
		{
			if(_input != null && !(DuosInputPlayerTeammate == null || DuosInputOpponentTeammate == null))
			{
				DebugLog("No need to run partial combat, all teammates found. Exiting.");
				return;
			}
			try
			{
				if(!ShouldRun())
					return;
				DebugLog(_instanceKey);

				if(HasErrorState())
					return;

				State = BobsBuddyState.CombatPartial;
				DebugLog("Setting UI state to combat...");
				BobsBuddyDisplay.SetState(BobsBuddyState.CombatPartial);
				BobsBuddyDisplay.ResetText();

				await RunAndDisplaySimulationAsync();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog, isDuos: true);
				return;
			}
		}

		private async Task RunAndDisplaySimulationAsync()
		{
			DebugLog("Running simulation...");
			BobsBuddyDisplay.HidePercentagesShowSpinners();
			var result = await RunSimulation();
			if(result == null || _input == null)
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
			else if(State == BobsBuddyState.CombatPartial)
			{
				DebugLog("Displaying partial simulation results");
				BobsBuddyDisplay.ShowPartialDuosSimulation(
					result.winRate,
					result.tieRate,
					result.lossRate,
					result.theirDeathRate,
					result.myDeathRate,
					result.damageResults.ToList(),
					friendlyWon: DuosInputPlayerTeammate == null,
					playerCanDie: _input.Player.Health <= _input.DamageCap,
					opponentCanDie: _input.Opponent.Health <= _input.DamageCap
				);
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
				if(State == BobsBuddyState.Shopping || State == BobsBuddyState.ShoppingAfterPartial)
				{
					DebugLog($"{_instanceKey} already in shopping state. Exiting");
					return;
				}
				var wasPreviousStateParcial = State == BobsBuddyState.CombatPartial;

				State = wasPreviousStateParcial ? BobsBuddyState.ShoppingAfterPartial : BobsBuddyState.Shopping;

				if(HasErrorState())
					return;

				BobsBuddyDisplay.SetLastOutcome(GetLastCombatDamageDealt());
				if(isGameOver)
				{
					BobsBuddyDisplay.SetState(wasPreviousStateParcial ? BobsBuddyState.GameOverAfterPartial : BobsBuddyState.GameOver);
					DebugLog("Setting UI state to GameOver");
				}
				else
				{
					BobsBuddyDisplay.SetState(wasPreviousStateParcial ? BobsBuddyState.ShoppingAfterPartial : BobsBuddyState.Shopping);
					DebugLog("Setting UI state to shopping");
				}

				ValidateSimulationResultAsync().Forget();
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog, _game.IsBattlegroundsDuosMatch);
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

		private bool SetupInputPlayer(
			Simulator simulator,
			Hearthstone.Player gamePlayer,
			global::BobsBuddy.Simulation.Player inputPlayer,
			Entity? playerEntity,
			bool friendly
			)
		{
			var playerGameHero = gamePlayer.Hero;

			if(playerEntity == null)
			{
				throw new ArgumentException(friendly ? "Player" : "Opponent" + " Entity could not be found. Exiting.");
			}

			if(gamePlayer.Board.Any(IsUnknownCard))
			{
				ErrorState = BobsBuddyErrorState.UnknownCards;
				throw new ArgumentException("Board has unknown cards. Exiting.");
			}

			if(gamePlayer.Board.Any(IsUnsupportedCard))
			{
				ErrorState = BobsBuddyErrorState.UnsupportedCards;
				throw new ArgumentException("Board has unsupported cards. Exiting.");
			}

			if(playerGameHero == null)
			{
				throw new ArgumentException("Hero(es) could not be found. Exiting.");
			}

			var murky = gamePlayer.Board.FirstOrDefault(e => e.CardId == NonCollectible.Neutral.Murky);
			var murkyBuff = murky?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			inputPlayer.BattlecriesPlayed = murky != null && murkyBuff > 0
				? murkyBuff / (murky.HasTag(GameTag.PREMIUM) ? 2 : 1) - 1
				: 0;

			if(!friendly && inputPlayer.Health <= 0)
			{
				inputPlayer.Health = 1000;
			}

			inputPlayer.Health = playerGameHero.Health + playerGameHero.GetTag(GameTag.ARMOR);
			inputPlayer.DamageTaken = playerGameHero.GetTag(GameTag.DAMAGE);
			inputPlayer.Tier = playerGameHero.GetTag(GameTag.PLAYER_TECH_LEVEL);

			var playerHeroPower = gamePlayer.Board.FirstOrDefault(x => x.IsHeroPower);
			var pHpData = playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
			var pHpData2 = playerHeroPower?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_2) ?? 0;
			if(playerHeroPower?.CardId == NonCollectible.Neutral.TeronGorefiend_RapidReanimation)
			{
				var minionsInPlay = gamePlayer.Board.Where(e => e.IsMinion && e.IsControlledBy(gamePlayer.Id)).Select(x => x.Id);
				var attachedToEntityId = gamePlayer.PlayerEntities
					.Where(x => x.CardId == NonCollectible.Neutral.TeronGorefiend_ImpendingDeath && (!friendly || (x.IsInPlay && friendly)))
					.Select(x => x.GetTag(GameTag.ATTACHED))
					.FirstOrDefault(x => minionsInPlay.Any(y => y == x));
				if(attachedToEntityId > 0)
					pHpData = attachedToEntityId;
			}

			if(playerHeroPower?.CardId == NonCollectible.Neutral.FlobbidinousFloop_GloriousGloop)
			{
				var minionsInPlay = gamePlayer.Board.Where(e => e.IsMinion && e.IsControlledBy(gamePlayer.Id)).Select(x => x.Id);
				var attachedToEntityId = gamePlayer.PlayerEntities
					.Where(x => x.CardId == NonCollectible.Neutral.FlobbidinousFloop_InTheGloop && (!friendly || (x.IsInPlay && friendly)))
					.Select(x => x.GetTag(GameTag.ATTACHED))
					.FirstOrDefault(x => minionsInPlay.Any(y => y == x));
				if(attachedToEntityId > 0)
					pHpData = attachedToEntityId;
			}

			inputPlayer.SetHeroPower(playerHeroPower?.CardId ?? "", friendly, WasHeroPowerActivated(playerHeroPower), pHpData, pHpData2);

			foreach(var quest in gamePlayer.Quests)
			{
				var rewardDbfId = quest.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
				var reward = Database.GetCardFromDbfId(rewardDbfId, false);
				inputPlayer.Quests.Add(new QuestData()
				{
					QuestProgress = quest.GetTag(GameTag.QUEST_PROGRESS),
					QuestProgressTotal = quest.GetTag(GameTag.QUEST_PROGRESS_TOTAL),
					QuestCardId = quest.CardId ?? "",
					RewardCardId = reward?.Id ?? ""
				});
			}

			foreach(var reward in gamePlayer.QuestRewards)
			{
				inputPlayer.Quests.Add(new QuestData()
				{
					RewardCardId = reward.Info.LatestCardId ?? ""
				});
			}

			foreach(var objective in gamePlayer.Objectives)
			{
				//TODO: [Duos] Check if friendly translates to player correctly
				inputPlayer.Objectives.Add(GetObjectiveFromEntity(simulator.ObjectiveFactory, friendly, objective));
			}

			var playerSide = GetOrderedMinions(gamePlayer.Board)
				.Where(e => e.IsControlledBy(gamePlayer.Id))
				.Select(e => GetMinionFromEntity(simulator.MinionFactory, friendly, e, GetAttachedEntities(e.Id)));
			foreach(var m in playerSide)
				inputPlayer.Side.Add(m);

			if(friendly)
			{
				inputPlayer.SetSecrets(gamePlayer.Secrets.Select(x => (int?)x.Card.DbfId).ToList());

				foreach(var e in gamePlayer.Hand)
				{
					if(e.IsMinion)
					{
						var minionEntity = new MinionCardEntity(GetMinionFromEntity(simulator.MinionFactory, true, e, GetAttachedEntities(e.Id)), null, simulator)
						{
							CanSummon = !e.HasTag(GameTag.LITERALLY_UNPLAYABLE),
						};
						inputPlayer.Hand.Add(minionEntity);
					}
					else if(e.CardId == NonCollectible.Neutral.BloodGem1)
						inputPlayer.Hand.Add(new BloodGem(null, simulator));
					else if(e.IsSpell)
						inputPlayer.Hand.Add(new SpellCardEntity(null, simulator));
					else
						inputPlayer.Hand.Add(new CardEntity(e.CardId ?? "", null, simulator)); // Not Unknown
				}
			}
			else
			{
				// TODO: [Duos] refactor
				_opponentHand = gamePlayer.Hand.ToList();
				inputPlayer.Hand.Clear();
				inputPlayer.Hand.AddRange(GetOpponentHandEntities(simulator));
			}


			var playerAttached = GetAttachedEntities(playerEntity.Id);
			var pEternalLegion = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Invalid.EternalKnight_EternalKnightPlayerEnchant);
			if(pEternalLegion != null)
				inputPlayer.EternalKnightCounter = pEternalLegion.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			var pUndeadBonus = playerAttached.FirstOrDefault(x => x.CardId == NonCollectible.Neutral.NerubianDeathswarmer_UndeadBonusAttackPlayerEnchantDnt);
			if(pUndeadBonus != null)
				inputPlayer.UndeadAttackBonus = pUndeadBonus.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
			inputPlayer.ElementalPlayCounter = playerEntity.GetTag((GameTag)2878);

			Log.Info($"pEternal={inputPlayer.EternalKnightCounter}, pUndead={inputPlayer.UndeadAttackBonus}, pElemental={inputPlayer.ElementalPlayCounter}, friendly={friendly}");

			inputPlayer.BloodGemAtkBuff = playerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFATKVALUE);
			inputPlayer.BloodGemHealthBuff =playerEntity.GetTag(GameTag.BACON_BLOODGEMBUFFHEALTHVALUE);

			Log.Info($"pBloodGem=+{inputPlayer.BloodGemAtkBuff}/+{inputPlayer.BloodGemHealthBuff}, friendly={friendly}");

			return true;
		}


		private void SnapshotBoardState(int turn)
		{
			DebugLog("Snapshotting board state...");
			LastAttackingHero = null;
			var simulator = new Simulator();
			var input = new Input();

			if(_game.GameEntity == null)
			{
				DebugLog("GameEntity could not be found. Exiting.");
				return;
			}

			input.availableRaces = BattlegroundsUtils.GetAvailableRaces(_currentGameId).ToList();
			if(_game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP_ENABLED) > 0)
				input.DamageCap = _game.GameEntity.GetTag(GameTag.BACON_COMBAT_DAMAGE_CAP);

			try
			{
				if(_input == null)
				{
					SetupInputPlayer(simulator, _game.Player, input.Player, _game.PlayerEntity, true);
					SetupInputPlayer(simulator, _game.Opponent, input.Opponent, _game.OpponentEntity, false);
					DuosInputPlayer = input.Player;
					DuosInputOpponent = input.Opponent;

					DuosInputPlayerTeammate = null;
					DuosInputOpponentTeammate = null;
				}
				else
				{
					if(_game.DuosWasPlayerHeroModified && DuosInputPlayerTeammate == null)
					{
						SetupInputPlayer(simulator, _game.Player, input.PlayerTeammate, _game.PlayerEntity, true);
						DuosInputPlayerTeammate = input.PlayerTeammate;
					}
					if(_game.DuosWasOpponentHeroModified && DuosInputOpponentTeammate == null)
					{
						SetupInputPlayer(simulator, _game.Opponent, input.OpponentTeammate, _game.OpponentEntity, false);
						DuosInputOpponentTeammate = input.OpponentTeammate;
					}
				}

				if(_game.IsBattlegroundsDuosMatch)
				{
					input.isDuos = true;
					input.Player = DuosInputPlayer;
					input.Opponent = DuosInputOpponent;
					input.PlayerTeammate = DuosInputPlayerTeammate ?? input.PlayerTeammate;
					input.OpponentTeammate = DuosInputOpponentTeammate ?? input.OpponentTeammate;
				}
			} catch(Exception e)
			{
				DebugLog(e.ToString());
				return;
			}

			var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
			var anomalyCardId = anomalyDbfId.HasValue ? Database.GetCardFromDbfId(anomalyDbfId.Value, false)?.Id : null;
			if(anomalyCardId != null)
				input.Anomaly = simulator.AnomalyFactory.Create(anomalyCardId);

			input.SetTurn(turn);

			_input = input;
			_turn = turn;

			DebugLog("Successfully snapshotted board state");
		}

		private int _reRunCount;

		private Task TryRerun()
		{
			if(_reRunCount++ <= 10)
			{
				DebugLog($"Input changed, re-running simulation! (#{_reRunCount})");
				if(ShouldRun())
				{
					var expandAfterError = ErrorState == BobsBuddyErrorState.None && Config.Instance.ShowBobsBuddyDuringCombat;
					ErrorState = BobsBuddyErrorState.None;
					BobsBuddyDisplay.SetErrorState(BobsBuddyErrorState.None, null, BobsBuddyDisplay.ResultsPanelExpanded || expandAfterError);
					Output = null;
					return RunAndDisplaySimulationAsync();
				}
			}
			else
			{
				DebugLog("Input changed, but the simulation already re-ran ten times");
			}

			return Task.CompletedTask;
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
			if(entities.Count(x => x is MinionCardEntity) <= _input.Opponent.Hand.Count(x => x is MinionCardEntity))
				return;

			_input.Opponent.Hand.Clear();
			_input.Opponent.Hand.AddRange(entities);

			await TryRerun();
		}

		internal async void UpdateOpponentSecret(Entity entity)
		{
			DoNotReport = true;
			if(!_game.IsBattlegroundsDuosMatch)
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
				_input.Opponent.SetSecrets(
					_game.Opponent.Secrets
						.Select(x => !string.IsNullOrEmpty(x.CardId) ? (int?)x.Card.DbfId : null)
						.Distinct(new SecretDbfIdComparer())
						.ToList()
				);
				DebugLog($"Set opponent S. with {_input.Opponent.Secrets.Count} S.");

				DebugLog("----- Simulation Input -----");
				DebugLog($"Player: heroPower={_input.Player.HeroPower.CardId}, used={_input.Player.HeroPower.IsActivated}, data={_input.Player.HeroPower.Data}");
				DebugLog($"Hand: {string.Join(", ",_input.Player.Hand.Select(x => x.ToString()))}");

				foreach(var minion in _input.Player.Side)
					DebugLog(minion.ToString());

				foreach(var quest in _input.Player.Quests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");

				DebugLog("---");
				DebugLog($"Opponent: heroPower={_input.Opponent.HeroPower.CardId}, used={_input.Opponent.HeroPower.IsActivated}, data={_input.Opponent.HeroPower.Data}");
				DebugLog($"Hand: {string.Join(", ",_input.Opponent.Hand.Select(x => x.ToString()))}");
				foreach(var minion in _input.Opponent.Side)
					DebugLog(minion.ToString());

				foreach(var quest in _input.Opponent.Quests)
					DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");


				if(_input.isDuos)
				{
					DebugLog("---");
					DebugLog("PlayerTeammate: heroPower=" + _input.PlayerTeammate.HeroPower.CardId + ", used=" + _input.PlayerTeammate.HeroPower.IsActivated + ", data=" + _input.PlayerTeammate.HeroPower.Data);
					DebugLog("Hand: " + string.Join(", ", _input.PlayerTeammate.Hand.Select(x => x.ToString())));
					foreach(var minion in _input.PlayerTeammate.Side)
						DebugLog(minion.ToString());

					foreach(var quest in _input.PlayerTeammate.Quests)
						DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");

					DebugLog("---");
					DebugLog("OpponentTeammate: heroPower=" + _input.OpponentTeammate.HeroPower.CardId + ", used=" + _input.OpponentTeammate.HeroPower.IsActivated + ", data=" + _input.OpponentTeammate.HeroPower.Data);
					DebugLog("Hand: " + string.Join(", ", _input.OpponentTeammate.Hand.Select(x => x.ToString())));
					foreach(var minion in _input.OpponentTeammate.Side)
						DebugLog(minion.ToString());

					foreach(var quest in _input.OpponentTeammate.Quests)
						DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");
				}

				DebugLog("---");

				if(_input.isDuos)
				{
					DebugLog("---");
					DebugLog("PlayerTeammate: heroPower=" + _input.PlayerTeammate.HeroPower.CardId + ", used=" + _input.PlayerTeammate.HeroPower.IsActivated + ", data=" + _input.PlayerTeammate.HeroPower.Data);
					DebugLog("Hand: " + string.Join(", ", _input.PlayerTeammate.Hand.Select(x => x.ToString())));
					foreach(var minion in _input.PlayerTeammate.Side)
						DebugLog(minion.ToString());

					foreach(var quest in _input.PlayerTeammate.Quests)
						DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");

					DebugLog("---");
					DebugLog("OpponentTeammate: heroPower=" + _input.OpponentTeammate.HeroPower.CardId + ", used=" + _input.OpponentTeammate.HeroPower.IsActivated + ", data=" + _input.OpponentTeammate.HeroPower.Data);
					DebugLog("Hand: " + string.Join(", ", _input.OpponentTeammate.Hand.Select(x => x.ToString())));
					foreach(var minion in _input.OpponentTeammate.Side)
						DebugLog(minion.ToString());

					foreach(var quest in _input.OpponentTeammate.Quests)
						DebugLog($"[{quest.QuestCardId} ({quest.QuestProgress}/{quest.QuestProgressTotal}): {quest.RewardCardId}]");
				}

				DebugLog("---");

				if(_input.Player.Secrets.Any())
				{
					DebugLog("Detected the following player S.");
					foreach(var s in _input.Player.Secrets)
						DebugLog(s.ToString());
				}

				if(_input.Opponent.Secrets.Any())
				{
					DebugLog("Detected the following opponent S.");
					foreach(var s in _input.Opponent.Secrets)
						DebugLog(s.ToString());
				}

				if(_input.isDuos) { 
					if(_input.OpponentTeammate.Secrets.Any())
					{
						DebugLog("Detected the following opponent teammate S.");
						foreach(var s in _input.OpponentTeammate.Secrets)
							DebugLog(s.ToString());
					}

					if(_input.PlayerTeammate.Secrets.Any())
					{
						DebugLog("Detected the following player teammate S.");
						foreach(var s in _input.PlayerTeammate.Secrets)
							DebugLog(s.ToString());
					}
				}
				 
				DebugLog("----- End of Input -----");

				DebugLog($"Running simulations with MaxIterations={Iterations} and ThreadCount={ThreadCount}...");

				var start = DateTime.Now;

				int timeAlloted = _input.Player.Side.Count >= 6 || _input.Opponent.Side.Count >= 6 ? MaxTimeForComplexBoards : MaxTime;
				Output = await new SimulationRunner().SimulateMultiThreaded(_input, Iterations, ThreadCount, timeAlloted);
				DoNotReport = false;

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
					Sentry.CaptureBobsBuddyException(ex, _input, _turn, _recentHDTLog, _game.IsBattlegroundsDuosMatch);
				Output = null;
				return null;
			}
			catch(Exception e)
			{
				DebugLog(e.ToString());
				Log.Error(e);
				if(ReportErrors)
					Sentry.CaptureBobsBuddyException(e, _input, _turn, _recentHDTLog, _game.IsBattlegroundsDuosMatch);
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
				DebugLog("Output is null. Exiting");
				return;
			}

			if(DoNotReport)
			{
				DebugLog("Output was invalidated. Exiting");
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
				Influx.OnBobsBuddySimulationCompleted(
					result, Output, _turn, _input?.Anomaly, terminalCase,
					isDuos:_game.IsBattlegroundsDuosMatch, isOpposingAkazamzarak: IsOpposingAkazamzarak()
				);

			if(terminalCase)
				Core.Game.Metrics.IncrementBobsBuddyTerminalCase();
		}

		private bool IsIncorrectCombatResult(CombatResult result)
			=> result == CombatResult.Tie && Output?.tieRate == 0
			|| result == CombatResult.Win && Output?.winRate == 0
			|| result == CombatResult.Loss && Output?.lossRate == 0;

		private bool IsIncorrectLethalResult(LethalResult result)
			=> result == LethalResult.FriendlyDied && Output?.myDeathRate == 0
			|| result == LethalResult.OpponentDied && Output?.theirDeathRate == 0;

		private bool OpposingKelThuzadDied(LethalResult result)
			=> result == LethalResult.OpponentDied && _input != null && _input.Opponent.HeroPower.CardId == HeroPowerIds.KelThuzad;

		private bool IsOpposingAkazamzarak()
			=> _input?.Opponent.HeroPower.CardId == HeroPowerIds.Azamarak || _input?.OpponentTeammate?.HeroPower.CardId == HeroPowerIds.Azamarak;

		private void AlertWithLastInputOutput(string result)
		{
			DebugLog($"Queueing alert... (valid input: {_input != null})");
			if(_input != null && Output != null)
				Sentry.QueueBobsBuddyTerminalCase(
					_input, Output, result, _turn, _recentHDTLog, _game.CurrentRegion,
					isDuos: _game.IsBattlegroundsDuosMatch, isOpposingAkazamzarak: IsOpposingAkazamzarak()
				);
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
