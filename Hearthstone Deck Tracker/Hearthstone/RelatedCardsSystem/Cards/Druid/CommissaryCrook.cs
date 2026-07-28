using System;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Prepare Battlecry: Spend all your Mana. Summon a random minion of that Cost."
// The mana spent is what's left after paying for the card itself (live cost when an
// in-hand copy exists, printed cost otherwise).
public class CommissaryCrook : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.CommissaryCrook;

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return Math.Max(RemainingMana(player) - HoveredCost(hoveredEntity, GetCardId()), 0);
	}
}
