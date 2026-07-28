using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a 2-Cost card."
public class ScarabKeychain : ClassOrNeutralCost2CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ScarabKeychain;
}
