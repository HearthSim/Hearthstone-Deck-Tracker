using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Casts When Drawn Summon a random 1-Cost Dragon."
// The printed "1-Cost" is a placeholder: the token shuffled in by Blessing of the Dragon
// summons Dragons costing as much as the number of times the player has Imbued, so the
// bucket is read from the Imbue counter (1 when no counter is running yet).
public class EmeraldPortal : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.EmeraldPortal;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.MINION && card.IsDragon();
	protected override string PoolCacheKey => "dragons";

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		GetCounter<ImbueCounter>(player)?.Value ?? 1;
}
