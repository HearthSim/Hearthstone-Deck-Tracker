using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Battlecry: Discover a Choose One card. It has both effects combined."
public class RaidNegotiator : ClassOrNeutralChooseOneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.RaidNegotiator;
}
