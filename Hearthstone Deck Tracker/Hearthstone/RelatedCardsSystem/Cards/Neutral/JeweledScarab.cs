using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a 3-Cost card."
public class JeweledScarab : ClassOrNeutralCost3CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.JeweledScarab;
}
