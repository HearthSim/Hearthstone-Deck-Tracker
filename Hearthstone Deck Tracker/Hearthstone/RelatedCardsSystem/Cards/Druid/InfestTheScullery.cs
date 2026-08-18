using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Summon two random 3-Cost minions. (Improved by your hero attacks this game.)"
public class InfestTheScullery : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.InfestTheScullery;

	protected override int BatchSize => 2;

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		GetCounter<InfestTheSculleryCounter>(player)?.SummonCost ?? 3;
}
