using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Move all spells in your deck to the top. You can pick to draw from the top or bottom each turn."
public class DiscsOfNorgannon : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.DiscsOfNorgannon;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);
}
