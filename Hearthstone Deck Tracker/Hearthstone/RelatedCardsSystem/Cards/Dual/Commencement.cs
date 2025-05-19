using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class Commencement : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Commencement;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "MINION");
}
