using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a Mage minion."
public class MessengerRaven : MageMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.MessengerRaven;
}
