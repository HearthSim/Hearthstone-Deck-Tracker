using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Spend all your Mana. Cast a random spell that costs that much."
public class ForbiddenShrine : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ForbiddenShrine;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return Math.Max(RemainingMana(player) - HoveredCost(hoveredEntity, GetCardId()), 0);
	}
}
