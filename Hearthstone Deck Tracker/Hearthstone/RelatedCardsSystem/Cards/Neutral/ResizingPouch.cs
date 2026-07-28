using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a card with Cost equal to your remaining Mana Crystals."
// Same live-mana bucket as ScrappyScavenger (remaining crystals after paying for this card).
public class ResizingPouch : ScrappyScavenger
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ResizingPouch;
}
