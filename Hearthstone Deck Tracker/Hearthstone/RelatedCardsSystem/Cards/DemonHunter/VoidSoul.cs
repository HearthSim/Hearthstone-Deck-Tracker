using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Summon a random 1-Cost Demon. Improve your future Void Souls."
public class VoidSoul : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.VoidSoul;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.MINION && card.IsDemon();
	protected override string PoolCacheKey => "demons";

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		GetCounter<VoidSoulCounter>(player)?.Value ?? 1;
}
