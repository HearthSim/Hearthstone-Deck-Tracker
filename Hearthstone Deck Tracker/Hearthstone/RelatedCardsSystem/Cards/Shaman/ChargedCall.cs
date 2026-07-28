using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a 1-Cost minion and summon it. (Upgraded for each Overload card you played this game!)"
public class ChargedCall : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ChargedCall;
	protected override int BatchSize => 3;
	protected override bool IsWithReplacement => false;

	protected override IEnumerable<Card> FilterPoolForPlayer(IEnumerable<Card> pool, Player player)
	{
		var playerClass = player.CurrentClass;
		return pool.Where(c => c.IsClass(playerClass) || c.IsClass("Neutral"));
	}

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		if(!player.IsLocalPlayer)
			return null;
		return 1 + (GetCounter<OverloadThisGameCounter>(player)?.Value ?? 0);
	}
}
