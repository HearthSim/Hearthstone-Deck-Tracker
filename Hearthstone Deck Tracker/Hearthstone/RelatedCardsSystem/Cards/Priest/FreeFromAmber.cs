using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover a minion that costs (8) or more. Summon it."
public class FreeFromAmber : ClassOrNeutralCostAtLeast8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.FreeFromAmber;
}
