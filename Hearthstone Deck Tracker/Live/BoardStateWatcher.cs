using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Live.Data;

namespace Hearthstone_Deck_Tracker.Live
{
	internal class BoardStateWatcher
	{
		private const int UpdateDelay = 1000;
		private const int RepeatDelay = 10000;
		private bool _update;
		private bool _running;
		private BoardState? _currentBoardState;
		private DateTime _currentBoardStateTime = DateTime.MinValue;
		private bool _invokedGameStart;
		public event Action<BoardState>? OnNewBoardState;
		public event Action<GameStart>? OnGameStart;

		public void Stop()
		{
			_update = false;
			_currentBoardState = null;
			_invokedGameStart = false;
		}

		public async void Start()
		{
			if(_running)
				return;
			_running = true;
			_update = true;
			while(_update)
			{
				var boardState = GetBoardState();
				var delta = (DateTime.Now - _currentBoardStateTime).TotalMilliseconds;
				var forceInvoke = delta > RepeatDelay && boardState != null && _currentBoardState != null;
				if(forceInvoke || (!boardState?.Equals(_currentBoardState) ?? false))
				{
					if(!_invokedGameStart)
					{
						_invokedGameStart = true;
						OnGameStart?.Invoke(GetGameStart(boardState!));
					}
					OnNewBoardState?.Invoke(boardState!);
					_currentBoardState = boardState;
					_currentBoardStateTime = DateTime.Now;
				}
				await Task.Delay(UpdateDelay);
			}
			_running = false;
		}

		private GameStart GetGameStart(BoardState? boardState)
		{
			var format = Core.Game.CurrentFormat ?? Format.Wild;
			var gameType = HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, format);
			var player = Core.Game.MatchInfo?.LocalPlayer;
			var (rank, legendRank) = format switch
			{
				Format.Standard => (player?.StandardRank, player?.Standard?.LegendRank),
				Format.Classic => (player?.ClassicRank, player?.Classic?.LegendRank),
				Format.Twist => (player?.TwistRank, player?.Twist?.LegendRank),
				_ => (player?.WildRank, player?.Wild?.LegendRank),
			};
			return new GameStart
			{
				Deck = boardState?.Player?.Deck,
				GameType = gameType,
				Rank = rank ?? 0,
				LegendRank = legendRank ?? 0
			};
		}

		private int DbfId(Entity? e)
		{
			if(e == null)
				return 0;
			var card = e.Info.LatestCardId == e.CardId
				? e.Card
				: Database.GetCardFromId(e.Info.LatestCardId);
			return card?.DbfId ?? 0;
		}

		private int DbfId(BattlegroundsTeammateBoardStateEntity? e)
		{
			if(e == null)
				return 0;
			var card = Database.GetCardFromId(e.CardId);
			return card?.DbfId ?? 0;
		}

		private int ZonePosition(Entity e) => e.GetTag(GameTag.ZONE_POSITION);
		private int ZonePosition(BattlegroundsTeammateBoardStateEntity e) =>
			e.Tags.TryGetValue((int)GameTag.ZONE_POSITION, out var position) ? position : 0;

		private int[] SortedDbfIds(IEnumerable<Entity> entities) => entities.OrderBy(ZonePosition).Select(DbfId).ToArray();
		private int[] SortedDbfIds(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(DbfId).ToArray();

		private int HeroId(Entity playerEntity) => playerEntity.GetTag(GameTag.HERO_ENTITY);

		private int WeaponId(Entity playerEntity) => playerEntity.GetTag(GameTag.WEAPON);

		private Entity? Find(Player p, int entityId) => p.PlayerEntities.FirstOrDefault(x => x.Id == entityId);

		private Entity? FindHeroPower(Player p) => p.PlayerEntities.FirstOrDefault(x => x.IsHeroPower && x.IsInPlay);

		private BoardStateQuest? Quest(Entity questEntity)
		{
			if(questEntity == null)
				return null;
			return new BoardStateQuest
			{
				DbfId = questEntity.Card.DbfId,
				Progress = questEntity.GetTag(GameTag.QUEST_PROGRESS),
				Total = questEntity.GetTag(GameTag.QUEST_PROGRESS_TOTAL)
			};
		}

		private int? BuddyDbfId(Player player)
		{
			if(Core.Game.BattlegroundsBuddiesEnabled)
				return null;
			var buddyDbfId = player.Hero?.GetTag(GameTag.BACON_COMPANION_ID);
			if(buddyDbfId == 0)
				return null;
			return buddyDbfId;
		}

		private int? BgsQuestReward(Player player, bool heroPower)
		{
			return player.QuestRewards.FirstOrDefault(x => x.HasTag(GameTag.BACON_IS_HEROPOWER_QUESTREWARD) == heroPower)?.Card.DbfId;
		}

		private int? BgsAnomaly(Entity? game) => BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(game);

		// Return the dbf id for an entity, but blacklisted against common hero cards we don't want want to show in the overlay.
		private int HeroDbfId(Entity? entity)
		{
			if(entity == null)
				return 0;

			if(entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.BaconphheroTavernBrawl)
				return 0;

			return DbfId(entity);
		}

		private BoardState? GetBoardState()
		{
			if(Core.Game.IsBattlegroundsMatch)
				return GetBattlegroundsBoardState();
			return GetTraditionalBoardState();
		}

		private BoardState? GetTraditionalBoardState()
		{
			if(Core.Game.PlayerEntity == null || Core.Game.OpponentEntity == null)
				return null;

			var player = Core.Game.Player;
			var opponent = Core.Game.Opponent;

			var deck = DeckList.Instance.ActiveDeck;
			var games = deck?.GetRelevantGames();
			var fullDeckList = new Dictionary<int, int>();
			var initialSideboards = new Dictionary<int, Dictionary<int, int>>();
			if(DeckList.Instance.ActiveDeckVersion != null)
			{
				foreach(var card in DeckList.Instance.ActiveDeckVersion.Cards)
					fullDeckList[card.DbfId] = card.Count;
				foreach(var sideboard in DeckList.Instance.ActiveDeckVersion.Sideboards)
				{
					var owner = Database.GetCardFromId(sideboard.OwnerCardId);
					if(owner != null) {
						initialSideboards[owner.DbfId] = sideboard.Cards.ToDictionary(card => card.DbfId, card => card.Count);
					}
				}
			}
			int FullCount(int dbfId) => fullDeckList == null ? 0 : fullDeckList.TryGetValue(dbfId, out var count) ? count : 0;

			var playerCardsList = new List<int[]>();
			var playerSideboardsList = new List<int[]>();
			if(deck != null)
			{
				foreach(var card in player.GetPlayerCardList(false, false, false))
				{
					if(card.ZilliaxCustomizableCosmeticModule)
					{
						var zilliax = Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.ZilliaxDeluxe3000);
						if(zilliax == null)
							continue;
						var inDeck = FullCount(zilliax.DbfId);
						playerCardsList.Add(new[] { zilliax.DbfId, card.Count, inDeck });
					}
					else
					{
						var inDeck = card.IsCreated ? 0 : FullCount(card.DbfId);
						playerCardsList.Add(new[] { card.DbfId, card.Count, inDeck });
					}
				}
				var currentSideboards = player.GetPlayerSideboards(false);
				foreach(var sideboard in currentSideboards)
				{
					var owner = Database.GetCardFromId(sideboard.OwnerCardId);
					if(owner != null)
					{
						Dictionary<int, int>? initialSideboard = null;
						initialSideboards.TryGetValue(owner.DbfId, out initialSideboard);
						foreach(var card in sideboard.Cards)
						{
							var initialCount = initialSideboard.TryGetValue(card.DbfId, out var count) ? count : 0;
							playerSideboardsList.Add(new[] { owner.DbfId, card.DbfId, card.Count, initialCount });
						}
					}
				}

			}

			var format = Core.Game.CurrentFormat ?? Format.Wild;
			var gameType = HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, format);
			var playerWeapon = DbfId(Find(player, WeaponId(Core.Game.PlayerEntity)));
			var opponentWeapon = DbfId(Find(opponent, WeaponId(Core.Game.OpponentEntity)));

			return new BoardState
			{
				Player = new BoardStatePlayer
				{
					Board = SortedDbfIds(player.Board.Where(x => x.TakesBoardSlot)),
					Deck = new BoardStateDeck
					{
						Cards = playerCardsList,
						Sideboards = playerSideboardsList,
						Name = deck?.Name,
						Format = deck?.GuessFormatType() ?? FormatType.FT_UNKNOWN,
						Hero = Database.GetHeroCardFromClass(deck?.Class)?.DbfId ?? 0,
						Wins = games?.Count(g => g.Result == GameResult.Win) ?? 0,
						Losses = games?.Count(g => g.Result == GameResult.Loss) ?? 0,
						Size = player.DeckCount
					},
					Secrets = SortedDbfIds(player.PlayerEntities.Where(x => x.IsInSecret)),
					Hero = HeroDbfId(Find(player, HeroId(Core.Game.PlayerEntity))),
					Hand = new BoardStateHand
					{
						Cards = SortedDbfIds(player.Hand),
						Size = player.HandCount
					},
					HeroPower = BgsQuestReward(player, true) ?? DbfId(FindHeroPower(player)),
					Weapon = playerWeapon != 0 ? playerWeapon : (BgsQuestReward(player, false) ?? BuddyDbfId(player) ?? 0),
					Fatigue = Core.Game.PlayerEntity.GetTag(GameTag.FATIGUE)
				},
				Opponent = new BoardStatePlayer
				{
					Board = SortedDbfIds(opponent.Board.Where(x => x.TakesBoardSlot)),
					Deck = new BoardStateDeck
					{
						Size = opponent.DeckCount
					},
					Hand = new BoardStateHand
					{
						Size = opponent.HandCount
					},
					Secrets = SortedDbfIds(opponent.PlayerEntities.Where(x => x.IsInSecret)),
					Hero = HeroDbfId(Find(opponent, HeroId(Core.Game.OpponentEntity))),
					HeroPower = BgsQuestReward(opponent, true) ?? DbfId(FindHeroPower(opponent)),
					Weapon = opponentWeapon != 0 ? opponentWeapon : (BgsQuestReward(opponent, false) ?? BuddyDbfId(opponent) ?? 0),
					Fatigue = Core.Game.OpponentEntity.GetTag(GameTag.FATIGUE)
				},
				GameType = gameType,
			};
		}

		private Tuple<BoardStatePlayer, BoardStatePlayer> GetBattlegroundsSoloPlayerBoardStates()
		{
			var player = Core.Game.Player;
			var opponent = Core.Game.Opponent;

			var playerEntity = Core.Game.PlayerEntity;
			int? playerWeaponEntityId = playerEntity != null ? WeaponId(playerEntity) : null;
			int playerWeapon = playerWeaponEntityId.HasValue ? DbfId(Find(player, playerWeaponEntityId.Value)) : 0;

			var opponentEntity = Core.Game.OpponentEntity;
			int? opponentWeaponEntityId = opponentEntity != null ? WeaponId(opponentEntity) : null;
			int opponentWeapon = opponentWeaponEntityId.HasValue ? DbfId(Find(opponent, opponentWeaponEntityId.Value)) : 0;

			return new Tuple<BoardStatePlayer, BoardStatePlayer>(
				new BoardStatePlayer
				{
					Board = SortedDbfIds(player.Board.Where(x => x.TakesBoardSlot)),
					Hero = HeroDbfId(playerEntity != null ? Find(player, HeroId(playerEntity)) : null),
					HeroPower = BgsQuestReward(player, true) ?? DbfId(FindHeroPower(player)),
					Weapon = playerWeapon != 0 ? playerWeapon : (BgsQuestReward(player, false) ?? BuddyDbfId(player) ?? 0),
					Hand = new BoardStateHand
					{
						Cards = SortedDbfIds(player.Hand),
						Size = player.HandCount
					},
					Secrets = SortedDbfIds(player.PlayerEntities.Where(x => x.IsInSecret)),
					Fatigue = playerEntity?.GetTag(GameTag.FATIGUE) ?? 0
				}, new BoardStatePlayer
				{
					Board = SortedDbfIds(opponent.Board.Where(x => x.TakesBoardSlot)),
					Hero = HeroDbfId(opponentEntity != null ? Find(opponent, HeroId(opponentEntity)) : null),
					HeroPower = BgsQuestReward(opponent, true) ?? DbfId(FindHeroPower(opponent)),
					Weapon = opponentWeapon != 0 ? opponentWeapon
						: (BgsQuestReward(opponent, false) ?? BuddyDbfId(opponent) ?? 0),
					Hand = new BoardStateHand
					{
						Size = opponent.HandCount
					},
					Secrets = SortedDbfIds(opponent.PlayerEntities.Where(x => x.IsInSecret)),
					Fatigue = opponentEntity?.GetTag(GameTag.FATIGUE) ?? 0
				}
			);
		}

		private static int GetTag(BattlegroundsTeammateBoardStateEntity entity, GameTag tag)
		{
			return entity.Tags.TryGetValue((int)tag, out var value) ? value : 0;
		}

		private BoardStatePlayer GetBattlegroundsDuosPlayerBoardState(
			BattlegroundsDuosBoardState duosState,
			int controller
		)
		{
			var friendlyEntities = duosState.Entities.Where(
				entity => GetTag(entity, GameTag.CONTROLLER) == controller
			).ToList();

			var inPlay = friendlyEntities.Where(
				entity => GetTag(entity, GameTag.ZONE) == (int)Zone.PLAY
			).ToList();

			var hero =inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.HERO);
			var heroPower = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.HERO_POWER);
			var weapon = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.WEAPON)
				?? inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_QUEST_REWARD);

			var board = inPlay.Where(x =>
				(CardType)GetTag(x, GameTag.CARDTYPE) is CardType.MINION or CardType.LOCATION or CardType.BATTLEGROUND_SPELL
			);

			var hand = friendlyEntities.Where(
				entity => GetTag(entity, GameTag.ZONE) == (int)Zone.HAND
			).ToList();

			var secrets = friendlyEntities.Where(
				entity => GetTag(entity, GameTag.ZONE) == (int)Zone.SECRET
			);

			return new BoardStatePlayer
			{
				Board = SortedDbfIds(board),
				Hero = DbfId(hero),
				HeroPower = DbfId(heroPower),
				Weapon = DbfId(weapon),
				Hand = new BoardStateHand
				{
					Cards = SortedDbfIds(hand),
					Size = hand.Count,
				},
				Secrets = SortedDbfIds(secrets),
				Fatigue = 0,
			};
		}

		private Tuple<BoardStatePlayer, BoardStatePlayer> GetBattlegroundsDuosPlayerBoardStates(
			BattlegroundsDuosBoardState duosState
		)
		{
			return new Tuple<BoardStatePlayer, BoardStatePlayer>(
				GetBattlegroundsDuosPlayerBoardState(duosState, Core.Game.Player.Id),
				GetBattlegroundsDuosPlayerBoardState(duosState, Core.Game.Opponent.Id)
			);
		}

		private BoardState? GetBattlegroundsBoardState()
		{
			if(Core.Game.PlayerEntity == null || Core.Game.OpponentEntity == null)
				return null;

			var maybeDuosState = Core.Game.BattlegroundsDuosBoardState;
			var duosState = maybeDuosState?.IsViewingTeammate == true ? maybeDuosState : null;
			var (playerBoardState, opponentBoardState) = duosState != null
				? GetBattlegroundsDuosPlayerBoardStates(duosState)
				: GetBattlegroundsSoloPlayerBoardStates();

			var format = Core.Game.CurrentFormat ?? Format.Wild;
			var gameType = HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, format);

			return new BoardState
			{
				Player = playerBoardState,
				Opponent = opponentBoardState,
				GameType = gameType,
				BattlegroundsAnomaly = BgsAnomaly(Core.Game.GameEntity),
				BobsBuddyOutput = GetBobsBuddyState()
			};
		}

		private Data.BobsBuddyState? GetBobsBuddyState()
		{
			if(Core.Game.CurrentGameStats == null || Core.Game.GameEntity == null)
				return null;

			var turn = Core.Game.GameEntity.GetTag(GameTag.TURN) %2 == 0? Core.Game.GetTurnNumber() : Core.Game.GetTurnNumber() - 1;

			var invokerInstance = BobsBuddyInvoker.GetInstance(Core.Game.CurrentGameStats.GameId, Math.Max(turn, 1) , false);

			var output = invokerInstance?.Output;

			TwitchSimulationState simulationState = TwitchSimulationState.WaitingForCombat;
			var errorstate = invokerInstance?.ErrorState ?? BobsBuddyErrorState.None;
			if(errorstate != BobsBuddyErrorState.None)
			{
				switch(invokerInstance?.ErrorState)
				{
					case BobsBuddyErrorState.NotEnoughData:
						simulationState = TwitchSimulationState.TooFewSimulations;
						break;
					case BobsBuddyErrorState.UnknownCards:
					// Re-using unknown here to not add new state on twitch
					case BobsBuddyErrorState.UnsupportedCards:
					case BobsBuddyErrorState.UnsupportedInteraction:
						simulationState = TwitchSimulationState.UnknownCards;
						break;
					case BobsBuddyErrorState.UpdateRequired:
						simulationState = TwitchSimulationState.UpdateRequired;
						break;
				}
				return new Data.BobsBuddyState { SimulationState = simulationState };
			}

			if(output == null)
			{
				return new Data.BobsBuddyState
				{
					SimulationState = TwitchSimulationState.WaitingForCombat
				};
			}

			var outputState = invokerInstance?.State;
			switch(outputState)
			{
				case BobsBuddy.BobsBuddyState.Combat or BobsBuddy.BobsBuddyState.CombatPartial:
					simulationState = TwitchSimulationState.InCombat;
					break;
				case BobsBuddy.BobsBuddyState.Shopping or BobsBuddy.BobsBuddyState.ShoppingAfterPartial:
					simulationState = TwitchSimulationState.InNonFirstShoppingPhase;
					break;
				case BobsBuddy.BobsBuddyState.Initial or BobsBuddy.BobsBuddyState.WaitingForTeammates:
					simulationState = TwitchSimulationState.WaitingForCombat;
					break;
				case BobsBuddy.BobsBuddyState.CombatWithoutSimulation:
					break;
				case null:
					simulationState = TwitchSimulationState.WaitingForCombat;
					break;
			}

			return new Data.BobsBuddyState
			{
				PlayerLethalRate = output.theirDeathRate,
				WinRate = output.winRate,
				TieRate = output.tieRate,
				LossRate = output.lossRate,
				OpponentLethalRate = output.myDeathRate,
				SimulationState = simulationState
			};
		}
	}
}
