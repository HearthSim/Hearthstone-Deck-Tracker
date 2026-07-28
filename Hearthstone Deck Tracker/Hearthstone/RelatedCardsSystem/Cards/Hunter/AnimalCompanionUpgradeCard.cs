using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

/// <summary>
/// "Replace your future Animal Companions with random Beasts that cost (N) more."
/// The cost bucket comes from <see cref="AnimalCompanionCounter"/>: hovering your own copy
/// shows the pool you'd get after playing it (counter + <see cref="RelativeCostPoolCard.CostOffset"/>),
/// while the opponent's counter already includes their played upgrades, so their bucket is
/// the counter as-is.
/// </summary>
public abstract class AnimalCompanionUpgradeCard : StateValuePoolCard
{
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.MINION && card.IsBeast();
	protected override string PoolCacheKey => "beasts";

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		var counter = GetCounter<AnimalCompanionCounter>(player);
		if(counter == null)
			return null;
		return player.IsLocalPlayer ? counter.Value + CostOffset : counter.Value;
	}
}
