using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a card with Cost equal to your remaining Mana Crystals."
// Discover of "a card" -> class + Neutral scoping, any playable card type.
public class ScrappyScavenger : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ScrappyScavenger;
	protected override int BatchSize => 3;
	protected override bool IsWithReplacement => false;
	protected override bool IsInPool(Card card) =>
		card.TypeEnum is CardType.MINION or CardType.SPELL or CardType.WEAPON or CardType.LOCATION;
	protected override string PoolCacheKey => "cards";

	protected override IEnumerable<Card> FilterPoolForPlayer(IEnumerable<Card> pool, Player player)
	{
		var playerClass = player.CurrentClass;
		return pool.Where(c => c.IsClass(playerClass) || !c.IsClassCard);
	}

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return Math.Max(RemainingMana(player) - HoveredCost(hoveredEntity, GetCardId()), 0);
	}
}
