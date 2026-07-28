using System;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Spend up to 8 Corpses to summon a random minion of that Cost."
// "Up to" is the player's choice; the representative outcome spends the maximum.
public class CorpseFarm : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.CorpseFarm;

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		player.CorpsesLeft is int corpses ? Math.Min(corpses, 8) : null;
}

public class CorpseFarmCore : CorpseFarm
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.CorpseFarmCore;
}
