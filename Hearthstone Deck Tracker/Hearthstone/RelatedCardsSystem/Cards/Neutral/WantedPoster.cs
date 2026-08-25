using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a minion that costs (5) or more. Give it Prepare."
public class WantedPoster : ClassOrNeutralCostAtLeast5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WantedPoster;
}
