using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Combo: Discover a Mage spell."
public class WandThief : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WandThief;
}
