using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Summon a random minion with Cost equal to your Armor (up to 10)."
public class HeavyMetal : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.HeavyMetal;

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return Math.Min(player.Hero?.GetTag(GameTag.ARMOR) ?? 0, 10);
	}
}
