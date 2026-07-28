using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a Mage minion. If your deck has no minions, keep all 3 instead."
public class FontOfPower : MageMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.FontOfPower;
}
