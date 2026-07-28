using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "After you cast a spell, add a random Mage minion to your hand."
public class DuneSculptor : MageMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DuneSculptor;
	public override int Picks() => 1;
}
