using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Your Rewinds keep BOTH potential outcomes. Battlecry: Discover a Rewind card from any class."
public class Morchie : RewindCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Morchie;
}
