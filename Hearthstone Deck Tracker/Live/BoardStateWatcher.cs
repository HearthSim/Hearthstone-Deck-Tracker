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
using Hearthstone_Deck_Tracker.Utility.Extensions;
using BoardState = Hearthstone_Deck_Tracker.Live.Data.BoardState;

namespace Hearthstone_Deck_Tracker.Live
{
	internal partial class BoardStateWatcher
	{
		// what identifies cards in the twitch payload, hard-coded to dbf ids until the extension handles card ids
		private const bool SendCardIds = false;

		// disable the player's deck list (cards, sideboards and name)
		private const bool SendDeckList = true;

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
				LegendRank = legendRank ?? 0,
				HearthstoneBuild = Core.Game.MetaData.HearthstoneBuild
			};
		}

		private Hearthstone.Card? ResolveCard(Entity? e)
		{
			if(e == null)
				return null;
			return e.Info.LatestCardId == e.CardId
				? e.Card
				: Database.GetCardFromId(e.Info.LatestCardId);
		}

		private CardRef ToCardRef(Hearthstone.Card? card)
		{
			if(card == null)
				return 0;
			return SendCardIds ? (CardRef)card.Id : (CardRef)card.DbfId;
		}

		private CardRef? CardRefFromDbfId(int? dbfId)
		{
			if(dbfId is not > 0)
				return null;
			var card = SendCardIds ? Database.GetCardFromDbfId(dbfId.Value, false) : null;
			return card != null ? ToCardRef(card) : dbfId.Value;
		}

		private CardRef Ref(Entity? e) => ToCardRef(ResolveCard(e));

		private CardRef? RefOrNull(Entity? e)
		{
			var card = ResolveCard(e);
			return card?.DbfId > 0 ? ToCardRef(card) : (CardRef?)null;
		}

		private int ZonePosition(Entity e) => e.GetTag(GameTag.ZONE_POSITION);

		private CardRef[] SortedRefs(IEnumerable<Entity> entities) => entities.OrderBy(ZonePosition).Select(Ref).ToArray();

		private CardWithEnchantments[] ToSortedBoard(IEnumerable<Entity> entities) =>
			entities.OrderBy(ZonePosition).Select(e => new CardWithEnchantments(ToCardRef(ResolveCard(e)))).ToArray();

		private CardWithEnchantments[] ToSortedBoard(IEnumerable<BoardCard> boardCards) =>
			boardCards
				.Where(c => c?.CardId != null)
				.Select(c => Database.GetCardFromId(c.CardId))
				.WhereNotNull()
				.Select(card => new CardWithEnchantments(ToCardRef(card)))
				.ToArray();

		private int HeroId(Entity playerEntity) => playerEntity.GetTag(GameTag.HERO_ENTITY);

		private int WeaponId(Entity playerEntity) => playerEntity.GetTag(GameTag.WEAPON);

		private Entity? Find(Player p, int entityId) => p.PlayerEntities.FirstOrDefault(x => x.Id == entityId);

		private Entity? FindHeroPower(Player p)
			=> p.PlayerEntities.FirstOrDefault(x => x.IsHeroPower && x.IsInPlay && x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == 0);

		private BoardStateQuest? Quest(Entity questEntity)
		{
			if(questEntity == null)
				return null;
			return new BoardStateQuest
			{
				DbfId = ToCardRef(questEntity.Card),
				Progress = questEntity.GetTag(GameTag.QUEST_PROGRESS),
				Total = questEntity.GetTag(GameTag.QUEST_PROGRESS_TOTAL)
			};
		}

		private CardRef? Buddy(Player player)
		{
			if(!Core.Game.BattlegroundsBuddiesEnabled)
				return null;

			var meter = player.Board.FirstOrDefault(x => x.GetTag(GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_HERO_BUDDY);
			if(meter == null || meter?.GetTag(GameTag.ZONE) != (int)Zone.PLAY)
				return null;

			var buddyDbfId = meter?.GetTag(GameTag.BACON_COMPANION_ID);
			if(buddyDbfId == 0)
				buddyDbfId = player.Hero?.GetTag(GameTag.BACON_COMPANION_ID);

			return CardRefFromDbfId(buddyDbfId);
		}

		private CardRef? BgsQuestReward(Player player, bool heroPower)
		{
			var questReward = player.QuestRewards.FirstOrDefault(x => x.HasTag(GameTag.BACON_IS_HEROPOWER_QUESTREWARD) == heroPower);
			return questReward != null ? ToCardRef(questReward.Card) : (CardRef?)null;
		}

		// Return the card for an entity, but blacklisted against common hero cards we don't want want to show in the overlay.
		private CardRef HeroRef(Entity? entity)
		{
			if(entity == null)
				return 0;

			if(entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.BaconphheroTavernBrawl)
				return 0;

			return Ref(entity);
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

			var playerCardsList = new List<object[]>();
			var playerSideboardsList = new List<object[]>();
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
						playerCardsList.Add(new object[] { ToCardRef(zilliax), card.Count, inDeck });
					}
					else
					{
						var inDeck = card.IsCreated ? 0 : FullCount(card.DbfId);
						playerCardsList.Add(new object[] { ToCardRef(card), card.Count, inDeck });
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
							playerSideboardsList.Add(new object[] { ToCardRef(owner), ToCardRef(card), card.Count, initialCount });
						}
					}
				}

			}

			var format = Core.Game.CurrentFormat ?? Format.Wild;
			var gameType = HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, format);
			var playerWeapon = RefOrNull(Find(player, WeaponId(Core.Game.PlayerEntity)));
			var opponentWeapon = RefOrNull(Find(opponent, WeaponId(Core.Game.OpponentEntity)));

			var anomalyId = new[] { GameTag.ANOMALY1, GameTag.ANOMALY2 }.Select(x => Core.Game.GameEntity?.GetTag(x)).FirstOrDefault(x => x is > 0);
			var anomaly = anomalyId.HasValue && Core.Game.Entities.TryGetValue(anomalyId.Value, out var anomalyEntity) && anomalyEntity != null
				? ToCardRef(anomalyEntity.Card)
				: (CardRef?)null;

			// Check if the special shop (timewarped tavern) is currently active
			var specialShopState = Watchers.SpecialShopChoicesStateWatcher.CurrentState;
			var specialShopActive = specialShopState?.IsActive == true && specialShopState.BoardCards.Count > 0;
			var opponentBoard = specialShopActive
				? ToSortedBoard(specialShopState!.BoardCards)
				: ToSortedBoard(opponent.Board.Where(x => x.TakesBoardSlot));

			return new BoardState
			{
				Player = new BoardStatePlayer
				{
					Board = ToSortedBoard(player.Board.Where(x => x.TakesBoardSlot)),
					Deck = new BoardStateDeck
					{
						// the extension only hides the deck list (including its title) for an empty array, not a missing one
						Cards = SendDeckList ? playerCardsList : new List<object[]>(),
						Sideboards = SendDeckList ? playerSideboardsList : null,
						Name = SendDeckList ? deck?.Name : null,
						Format = deck?.GuessFormatType() ?? FormatType.FT_UNKNOWN,
						Hero = ToCardRef(Database.GetHeroCardFromClass(deck?.Class)),
						Wins = games?.Count(g => g.Result == GameResult.Win) ?? 0,
						Losses = games?.Count(g => g.Result == GameResult.Loss) ?? 0,
						Size = player.DeckCount
					},
					Secrets = SortedRefs(player.PlayerEntities.Where(x => x.IsInSecret)),
					Hero = HeroRef(Find(player, HeroId(Core.Game.PlayerEntity))),
					Hand = new BoardStateHand
					{
						Cards = SortedRefs(player.Hand),
						Size = player.HandCount
					},
					HeroPower = BgsQuestReward(player, true) ?? RefOrNull(FindHeroPower(player)),
					Weapon = playerWeapon ?? BgsQuestReward(player, false) ?? Buddy(player) ?? 0,
					Fatigue = Core.Game.PlayerEntity.GetTag(GameTag.FATIGUE)
				},
				Opponent = new BoardStatePlayer
				{
					Board = opponentBoard,
					Deck = new BoardStateDeck
					{
						Size = opponent.DeckCount
					},
					Hand = new BoardStateHand
					{
						Size = opponent.HandCount
					},
					Secrets = SortedRefs(opponent.PlayerEntities.Where(x => x.IsInSecret)),
					Hero = HeroRef(Find(opponent, HeroId(Core.Game.OpponentEntity))),
					HeroPower = BgsQuestReward(opponent, true) ?? RefOrNull(FindHeroPower(opponent)),
					Weapon = opponentWeapon ?? BgsQuestReward(opponent, false) ?? Buddy(opponent) ?? 0,
					Fatigue = Core.Game.OpponentEntity.GetTag(GameTag.FATIGUE)
				},
				GameType = gameType,
				HearthstoneBuild = Core.Game.MetaData.HearthstoneBuild,
				TraditionalAnomaly = anomaly,
			};
		}
	}
}
