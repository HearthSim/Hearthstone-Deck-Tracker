using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a 4-Cost card."
public class GoldenScarab : ClassOrNeutralCost4CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GoldenScarab;
}
