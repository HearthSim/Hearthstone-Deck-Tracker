
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Each turn this is in your hand, transform it into a random Mage spell."
public class ShiftingScroll : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ShiftingScroll;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
