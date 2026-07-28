using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Cast a random 1-Cost spell. Improve your future Chaotic Tendrils."
// ChaoticTendrilCounter holds how many have been played; the next one casts a
// (count + 1)-Cost spell, capped at 10.
public class ChaoticTendril : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ChaoticTendril;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		Math.Min((GetCounter<ChaoticTendrilCounter>(player)?.Value ?? 0) + 1, 10);
}
