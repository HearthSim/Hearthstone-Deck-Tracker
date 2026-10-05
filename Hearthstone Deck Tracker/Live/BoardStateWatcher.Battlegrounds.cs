using System;
using System.Collections.Generic;
using System.Linq;
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
		private Hearthstone.Card? ResolveCard(BattlegroundsTeammateBoardStateEntity? e) =>
			e == null ? null : Database.GetCardFromId(e.CardId);

		private CardRef Ref(BattlegroundsTeammateBoardStateEntity? e) => ToCardRef(ResolveCard(e));

		private CardRef? RefOrNull(BattlegroundsTeammateBoardStateEntity? e)
		{
			var card = ResolveCard(e);
			return card?.DbfId > 0 ? ToCardRef(card) : (CardRef?)null;
		}

		private int ZonePosition(BattlegroundsTeammateBoardStateEntity e) =>
			e.Tags.TryGetValue((int)GameTag.ZONE_POSITION, out var position) ? position : 0;

		private CardRef[] SortedRefs(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(Ref).ToArray();

		private CardWithEnchantments[] ToSortedBoard(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(e => new CardWithEnchantments(ToCardRef(ResolveCard(e)))).ToArray();

		private CardWithEnchantments[] ToSortedBoardWithDarkGifts(IEnumerable<Entity> entities) =>
			entities.OrderBy(ZonePosition).Select(ToCardWithDarkGifts).ToArray();
		private CardWithEnchantments[] ToSortedBoardWithDarkGifts(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(ToCardWithDarkGifts).ToArray();

		private CardWithEnchantments ToCardWithDarkGifts(Entity entity)
		{
			var darkGifts = Core.Game.Entities.Values
				.Where(x => x.IsAttachedTo(entity.Id) && x.HasTag(GameTag.IS_NIGHTMARE_BONUS))
				.OrderBy(x => x.Id)
				.Select(DarkGiftCard)
				.WhereNotNull()
				.Select(ToCardRef);
			return new CardWithEnchantments(ToCardRef(ResolveCard(entity)), darkGifts);
		}

		private CardWithEnchantments ToCardWithDarkGifts(BattlegroundsTeammateBoardStateEntity entity)
		{
			// the teammate board state has no entity ids we could resolve, but the client itself picks the
			// enchantment portrait by CREATOR_DBID
			var darkGifts = entity.Attachments
				.Where(x => GetTag(x, GameTag.IS_NIGHTMARE_BONUS) > 0)
				.OrderBy(x => GetTag(x, GameTag.ENTITY_ID))
				.Select(x => Database.GetCardFromDbfId(GetTag(x, GameTag.CREATOR_DBID), false))
				.Where(IsBattlegroundsDarkGift)
				.Select(ToCardRef);
			return new CardWithEnchantments(ToCardRef(ResolveCard(entity)), darkGifts);
		}

		// the Deity Sigil sits in the secret zone as an objective. A Deity has no entity of its own
		// until it awakens, so report the Deity the sigil is holding instead of the sigil itself.
		private Hearthstone.Card? ResolveSecretCard(Entity entity)
		{
			if(entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.SecretDeityDnt)
				return ResolveCard(entity);
			return Database.GetCardFromDbfId(entity.GetTag(GameTag.BACON_EVOLUTION_CARD_ID), false) ?? ResolveCard(entity);
		}

		private Hearthstone.Card? ResolveSecretCard(BattlegroundsTeammateBoardStateEntity entity)
		{
			if(entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.SecretDeityDnt)
				return ResolveCard(entity);
			return Database.GetCardFromDbfId(GetTag(entity, GameTag.BACON_EVOLUTION_CARD_ID), false) ?? ResolveCard(entity);
		}

		private CardRef[] SortedSecretRefs(IEnumerable<Entity> entities) =>
			entities.OrderBy(ZonePosition).Select(e => ToCardRef(ResolveSecretCard(e))).ToArray();

		private CardRef[] SortedSecretRefs(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(e => ToCardRef(ResolveSecretCard(e))).ToArray();

		// the enchantment has no art of its own, so report the dark gift that created it
		private Hearthstone.Card? DarkGiftCard(Entity enchantment)
		{
			var creatorId = enchantment.GetTag(GameTag.CREATOR);
			if(Core.Game.Entities.TryGetValue(creatorId, out var creator) && IsBattlegroundsDarkGift(creator.Card))
				return creator.Card;
			var dbCreator = Database.GetCardFromDbfId(enchantment.GetTag(GameTag.CREATOR_DBID), false);
			return IsBattlegroundsDarkGift(dbCreator) ? dbCreator : null;
		}

		private static bool IsBattlegroundsDarkGift(Hearthstone.Card? card) =>
			card?.TypeEnum == CardType.SPELL && card.CardSet == CardSet.BATTLEGROUNDS && card.GetTag(GameTag.IS_NIGHTMARE_BONUS) > 0;

		// a hero power position can be occupied by a hero power, a hero power quest reward or a hero
		// power trinket, all keyed by ADDITIONAL_HERO_POWER_INDEX (0 = bottom/only, 1 = top)
		private CardRef? BgsHeroPowerSlot(Player player, int index)
		{
			var questReward = player.QuestRewards.FirstOrDefault(x =>
				x.HasTag(GameTag.BACON_IS_HEROPOWER_QUESTREWARD) && x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == index);
			if(questReward != null)
				return ToCardRef(questReward.Card);
			// the game treats any index >= 1 as the secondary trinket slot (ZoneBattlegroundTrinket)
			var trinket = player.Trinkets.FirstOrDefault(x =>
				x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_6) == TrinketHeroPowerSlot &&
				(index == 0 ? x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == 0 : x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) >= 1));
			if(trinket != null)
				return ToCardRef(trinket.Card);
			var heroPower = player.PlayerEntities.FirstOrDefault(x =>
				x.IsHeroPower && x.IsInPlay && x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == index);
			return heroPower != null ? Ref(heroPower) : (CardRef?)null;
		}

		private const int TrinketFirstSlot = 1;
		private const int TrinketSecondSlot = 2;
		private const int TrinketHeroPowerSlot = 3;

		private CardRef? BgsTrinket(Player player, int trinketSlot)
		{
			var trinketEntity = player.Trinkets.FirstOrDefault(x =>
				x.HasTag(GameTag.TAG_SCRIPT_DATA_NUM_6) &&
				x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_6) == trinketSlot
			);

			return trinketEntity != null ? ToCardRef(trinketEntity.Card) : (CardRef?)null;
		}

		private CardRef? BgsAnomaly(Entity? game)
		{
			return CardRefFromDbfId(BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(game));
		}

		private CardRef? BgsDarkGifts(Entity? game)
		{
			if(game?.GetTag(GameTag.BACON_DARK_GIFTS_ACTIVE) != 1)
				return null;
			var darkGifts = Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.DarkGifts);
			return darkGifts != null ? ToCardRef(darkGifts) : (CardRef?)null;
		}

		private Tuple<BoardStatePlayer, BoardStatePlayer> GetBattlegroundsSoloPlayerBoardStates()
		{
			var player = Core.Game.Player;
			var opponent = Core.Game.Opponent;

			var playerEntity = Core.Game.PlayerEntity;
			int? playerWeaponEntityId = playerEntity != null ? WeaponId(playerEntity) : null;
			var playerWeapon = playerWeaponEntityId.HasValue ? RefOrNull(Find(player, playerWeaponEntityId.Value)) : null;

			var opponentEntity = Core.Game.OpponentEntity;
			int? opponentWeaponEntityId = opponentEntity != null ? WeaponId(opponentEntity) : null;
			var opponentWeapon = opponentWeaponEntityId.HasValue ? RefOrNull(Find(opponent, opponentWeaponEntityId.Value)) : null;

			// Check if the special shop (timewarped tavern) is currently active
			var specialShopState = Watchers.SpecialShopChoicesStateWatcher.CurrentState;
			var specialShopActive = specialShopState?.IsActive == true && specialShopState.BoardCards.Count > 0;
			var opponentBoard = specialShopActive
				? ToSortedBoard(specialShopState!.BoardCards)
				: ToSortedBoardWithDarkGifts(opponent.Board.Where(x => x.TakesBoardSlot));

			// the primary hero power sits at the bottom for the player and at the top for the opponent
			var playerHeroPowerPrimary = BgsHeroPowerSlot(player, 0);
			var playerHeroPowerSecondary = BgsHeroPowerSlot(player, 1);
			var opponentHeroPowerPrimary = BgsHeroPowerSlot(opponent, 0);
			var opponentHeroPowerSecondary = BgsHeroPowerSlot(opponent, 1);

			return new Tuple<BoardStatePlayer, BoardStatePlayer>(
				new BoardStatePlayer
				{
					Board = ToSortedBoardWithDarkGifts(player.Board.Where(x => x.TakesBoardSlot)),
					HeroPower = playerHeroPowerSecondary == null ? playerHeroPowerPrimary : null,
					HeroPowerTop = playerHeroPowerSecondary,
					HeroPowerBottom = playerHeroPowerSecondary != null ? playerHeroPowerPrimary : null,
					Weapon = playerWeapon ??
						BgsQuestReward(player, false) ??
						Buddy(player) ?? 0,
					FirstTrinket = BgsTrinket(player, TrinketFirstSlot),
					SecondTrinket = BgsTrinket(player, TrinketSecondSlot),
					Hand = new BoardStateHand
					{
						Cards = SortedRefs(player.Hand),
						Size = player.HandCount
					},
					Secrets = SortedSecretRefs(player.PlayerEntities.Where(x => x.IsInSecret)),
					Fatigue = playerEntity?.GetTag(GameTag.FATIGUE) ?? 0
				}, new BoardStatePlayer
				{
					Board = opponentBoard,
					HeroPower = opponentHeroPowerSecondary == null ? opponentHeroPowerPrimary : null,
					HeroPowerTop = opponentHeroPowerSecondary != null ? opponentHeroPowerPrimary : null,
					HeroPowerBottom = opponentHeroPowerSecondary,
					Weapon = opponentWeapon ??
						BgsQuestReward(opponent, false) ??
						Buddy(opponent) ?? 0,
					FirstTrinket = BgsTrinket(opponent, TrinketFirstSlot),
					SecondTrinket = BgsTrinket(opponent, TrinketSecondSlot),
					Hand = new BoardStateHand
					{
						Size = opponent.HandCount
					},
					Secrets = SortedSecretRefs(opponent.PlayerEntities.Where(x => x.IsInSecret)),
					Fatigue = opponentEntity?.GetTag(GameTag.FATIGUE) ?? 0
				}
			);
		}

		private static int GetTag(BattlegroundsTeammateBoardStateEntity? entity, GameTag tag)
		{
			if(entity == null)
				return 0;
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

			var lesserTrinket = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_TRINKET && GetTag(entity, GameTag.TAG_SCRIPT_DATA_NUM_6) == TrinketFirstSlot);
			var greaterTrinket = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_TRINKET && GetTag(entity, GameTag.TAG_SCRIPT_DATA_NUM_6) == TrinketSecondSlot);

			BattlegroundsTeammateBoardStateEntity? HeroPowerSlot(int index) =>
				inPlay.FirstOrDefault(entity =>
					GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_QUEST_REWARD
					&& GetTag(entity, GameTag.BACON_IS_HEROPOWER_QUESTREWARD) > 0
					&& GetTag(entity, GameTag.ADDITIONAL_HERO_POWER_INDEX) == index)
				?? inPlay.FirstOrDefault(entity =>
					GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_TRINKET
					&& GetTag(entity, GameTag.TAG_SCRIPT_DATA_NUM_6) == TrinketHeroPowerSlot
					&& (index == 0
						? GetTag(entity, GameTag.ADDITIONAL_HERO_POWER_INDEX) == 0
						: GetTag(entity, GameTag.ADDITIONAL_HERO_POWER_INDEX) >= 1))
				?? inPlay.FirstOrDefault(entity =>
					GetTag(entity, GameTag.CARDTYPE) == (int)CardType.HERO_POWER
					&& GetTag(entity, GameTag.ADDITIONAL_HERO_POWER_INDEX) == index);

			var heroPowerPrimary = HeroPowerSlot(0);
			var heroPowerSecondary = HeroPowerSlot(1);

			var weapon = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.WEAPON)
				?? inPlay.FirstOrDefault(entity =>
					GetTag(entity, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_QUEST_REWARD
					&& GetTag(entity, GameTag.BACON_IS_HEROPOWER_QUESTREWARD) == 0);

			var buddyDbfId = 0;
			if(Core.Game.BattlegroundsBuddiesEnabled)
			{
				var meter = friendlyEntities.FirstOrDefault(x => GetTag(x, GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_HERO_BUDDY);
				if(meter != null && GetTag(meter, GameTag.ZONE) == (int)Zone.PLAY)
					buddyDbfId = GetTag(meter, GameTag.BACON_COMPANION_ID);
			}

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
				Board = ToSortedBoardWithDarkGifts(board),
				HeroPower = heroPowerSecondary == null ? RefOrNull(heroPowerPrimary) : null,
				HeroPowerTop = RefOrNull(heroPowerSecondary),
				HeroPowerBottom = heroPowerSecondary != null ? RefOrNull(heroPowerPrimary) : null,
				Weapon = weapon != null ? Ref(weapon) : CardRefFromDbfId(buddyDbfId) ?? 0,
				FirstTrinket = Ref(lesserTrinket),
				SecondTrinket = Ref(greaterTrinket),
				Hand = new BoardStateHand
				{
					Cards = SortedRefs(hand),
					Size = hand.Count,
				},
				Secrets = SortedSecretRefs(secrets),
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
				HearthstoneBuild = Core.Game.MetaData.HearthstoneBuild,
				BattlegroundsAnomaly = BgsAnomaly(Core.Game.GameEntity),
				BattlegroundsDarkGiftsSlot = BgsDarkGifts(Core.Game.GameEntity),
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
