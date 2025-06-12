using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class WallowTheWretched: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.WallowTheWretched;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.OriginalClass) && GetRelatedCards(opponent).Count > 1;
	}

	public List<Card?> GetRelatedCards(Player player)
	{
		if(player.IsLocalPlayer)
		{
			return player.RevealedEntities
				.Where(entity =>
					entity.HasTag(GameTag.IS_NIGHTMARE_BONUS) &&
					!entity.IsInSetAside && !entity.IsInZone(Zone.REMOVEDFROMGAME))
				.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
				.Where(card => card is {Type: "Spell"})
				.ToList();
		}

		var opponentDarkGiftEnchantments = player.RevealedEntities.Where(entity =>
			entity.HasTag(GameTag.IS_NIGHTMARE_BONUS) &&
			entity.GetTag(GameTag.CARDTYPE) == (int)CardType.ENCHANTMENT);
		return player.RevealedEntities
			.Where(entity =>
				entity.HasTag(GameTag.IS_NIGHTMARE_BONUS) &&
				!entity.IsInSetAside && !entity.IsInZone(Zone.REMOVEDFROMGAME) &&
				opponentDarkGiftEnchantments.Any(m => m.GetTag(GameTag.CREATOR) == entity.Id))
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is {Type: "Spell"})
			.ToList();
	}

}
