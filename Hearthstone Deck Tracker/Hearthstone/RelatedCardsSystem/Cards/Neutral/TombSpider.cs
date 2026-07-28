using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Beast."
public class TombSpider : ClassOrNeutralBeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TombSpider;
}
