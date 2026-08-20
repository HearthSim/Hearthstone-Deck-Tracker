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
using BoardState = Hearthstone_Deck_Tracker.Live.Data.BoardState;

namespace Hearthstone_Deck_Tracker.Live
{
	internal partial class BoardStateWatcher
	{
		private Hearthstone.Card? ResolveCard(BattlegroundsTeammateBoardStateEntity? e) =>
			e == null ? null : Database.GetCardFromId(e.CardId);

		private int DbfId(BattlegroundsTeammateBoardStateEntity? e) => ResolveCard(e)?.DbfId ?? 0;

		private int? DbfIdOrNull(BattlegroundsTeammateBoardStateEntity? e)
		{
			var dbfId = DbfId(e);
			return dbfId != 0 ? dbfId : (int?)null;
		}

		private int ZonePosition(BattlegroundsTeammateBoardStateEntity e) =>
			e.Tags.TryGetValue((int)GameTag.ZONE_POSITION, out var position) ? position : 0;

		private int[] SortedDbfIds(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(DbfId).ToArray();

		private CardWithEnchantments[] ToSortedBoard(IEnumerable<BattlegroundsTeammateBoardStateEntity> entities) =>
			entities.OrderBy(ZonePosition).Select(e => new CardWithEnchantments(ToCardRef(ResolveCard(e)))).ToArray();

		// a hero power position can be occupied by a hero power, a hero power quest reward or a hero
		// power trinket, all keyed by ADDITIONAL_HERO_POWER_INDEX (0 = bottom/only, 1 = top)
		private int? BgsHeroPowerSlot(Player player, int index)
		{
			var questReward = player.QuestRewards.FirstOrDefault(x =>
				x.HasTag(GameTag.BACON_IS_HEROPOWER_QUESTREWARD) && x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == index);
			if(questReward != null)
				return questReward.Card.DbfId;
			// the game treats any index >= 1 as the secondary trinket slot (ZoneBattlegroundTrinket)
			var trinket = player.Trinkets.FirstOrDefault(x =>
				x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_6) == TrinketHeroPowerSlot &&
				(index == 0 ? x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == 0 : x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) >= 1));
			if(trinket != null)
				return trinket.Card.DbfId;
			var heroPower = player.PlayerEntities.FirstOrDefault(x =>
				x.IsHeroPower && x.IsInPlay && x.GetTag(GameTag.ADDITIONAL_HERO_POWER_INDEX) == index);
			return heroPower != null ? DbfId(heroPower) : (int?)null;
		}

		private const int TrinketFirstSlot = 1;
		private const int TrinketSecondSlot = 2;
		private const int TrinketHeroPowerSlot = 3;

		private int? BgsTrinket(Player player, int trinketSlot)
		{
			var trinketEntity = player.Trinkets.FirstOrDefault(x =>
				x.HasTag(GameTag.TAG_SCRIPT_DATA_NUM_6) &&
				x.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_6) == trinketSlot
			);

			return trinketEntity?.Card.DbfId;
		}

		private int? BgsAnomaly(Entity? game)
		{
			// the "Discover a Dark Gift" button takes the same slot as anomalies
			if(game?.GetTag(GameTag.BACON_DARK_GIFTS_ACTIVE) == 1)
				return Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.DarkGifts1)?.DbfId;

			return BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(game);
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

			// Check if the special shop (timewarped tavern) is currently active
			var specialShopState = Watchers.SpecialShopChoicesStateWatcher.CurrentState;
			var specialShopActive = specialShopState?.IsActive == true && specialShopState.BoardCards.Count > 0;
			var opponentBoard = specialShopActive
				? ToSortedBoard(specialShopState!.BoardCards)
				: ToSortedBoard(opponent.Board.Where(x => x.TakesBoardSlot));

			// the primary hero power sits at the bottom for the player and at the top for the opponent
			var playerHeroPowerPrimary = BgsHeroPowerSlot(player, 0);
			var playerHeroPowerSecondary = BgsHeroPowerSlot(player, 1);
			var opponentHeroPowerPrimary = BgsHeroPowerSlot(opponent, 0);
			var opponentHeroPowerSecondary = BgsHeroPowerSlot(opponent, 1);

			return new Tuple<BoardStatePlayer, BoardStatePlayer>(
				new BoardStatePlayer
				{
					Board = ToSortedBoard(player.Board.Where(x => x.TakesBoardSlot)),
					Hero = HeroDbfId(playerEntity != null ? Find(player, HeroId(playerEntity)) : null),
					HeroPower = playerHeroPowerSecondary == null ? playerHeroPowerPrimary : null,
					HeroPowerTop = playerHeroPowerSecondary,
					HeroPowerBottom = playerHeroPowerSecondary != null ? playerHeroPowerPrimary : null,
					Weapon = playerWeapon != 0 ? playerWeapon :
						BgsQuestReward(player, false) ??
						BuddyDbfId(player) ?? 0,
					FirstTrinket = BgsTrinket(player, TrinketFirstSlot),
					SecondTrinket = BgsTrinket(player, TrinketSecondSlot),
					Hand = new BoardStateHand
					{
						Cards = SortedDbfIds(player.Hand),
						Size = player.HandCount
					},
					Secrets = SortedDbfIds(player.PlayerEntities.Where(x => x.IsInSecret)),
					Fatigue = playerEntity?.GetTag(GameTag.FATIGUE) ?? 0
				}, new BoardStatePlayer
				{
					Board = opponentBoard,
					Hero = HeroDbfId(opponentEntity != null ? Find(opponent, HeroId(opponentEntity)) : null),
					HeroPower = opponentHeroPowerSecondary == null ? opponentHeroPowerPrimary : null,
					HeroPowerTop = opponentHeroPowerSecondary != null ? opponentHeroPowerPrimary : null,
					HeroPowerBottom = opponentHeroPowerSecondary,
					Weapon = opponentWeapon != 0 ? opponentWeapon :
						BgsQuestReward(opponent, false) ??
						BuddyDbfId(opponent) ?? 0,
					FirstTrinket = BgsTrinket(opponent, TrinketFirstSlot),
					SecondTrinket = BgsTrinket(opponent, TrinketSecondSlot),
					Hand = new BoardStateHand
					{
						Size = opponent.HandCount
					},
					Secrets = SortedDbfIds(opponent.PlayerEntities.Where(x => x.IsInSecret)),
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

			var hero = inPlay.FirstOrDefault(entity => GetTag(entity, GameTag.CARDTYPE) == (int)CardType.HERO);

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
				Board = ToSortedBoard(board),
				Hero = DbfId(hero),
				HeroPower = heroPowerSecondary == null ? DbfIdOrNull(heroPowerPrimary) : null,
				HeroPowerTop = DbfIdOrNull(heroPowerSecondary),
				HeroPowerBottom = heroPowerSecondary != null ? DbfIdOrNull(heroPowerPrimary) : null,
				Weapon = weapon != null ? DbfId(weapon) : buddyDbfId,
				FirstTrinket = DbfId(lesserTrinket),
				SecondTrinket = DbfId(greaterTrinket),
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
				HearthstoneBuild = Core.Game.MetaData.HearthstoneBuild,
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
