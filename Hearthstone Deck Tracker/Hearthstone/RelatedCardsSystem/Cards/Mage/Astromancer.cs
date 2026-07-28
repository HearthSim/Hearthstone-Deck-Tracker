using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Summon a random minion with Cost equal to your hand size."
// The battlecry resolves after the card leaves the hand, so with an in-hand copy the
// hand counts one less.
public class Astromancer : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Astromancer;

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		var handSize = player.Hand.Count();
		if(hoveredEntity != null)
			handSize--;
		return Math.Max(handSize, 0);
	}
}

public class AstromancerCore : Astromancer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.AstromancerCore;
}
