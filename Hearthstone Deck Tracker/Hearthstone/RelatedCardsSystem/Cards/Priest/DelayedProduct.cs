using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover and summon a minion that costs (8) or more. It goes Dormant for 2 turns."
public class DelayedProduct : ClassOrNeutralCostAtLeast8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.DelayedProduct;
}
