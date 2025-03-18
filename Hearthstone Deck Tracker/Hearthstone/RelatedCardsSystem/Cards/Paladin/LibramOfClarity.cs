using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class LibramOfClarity : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.LibramOfClarity;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.Type == "Minion");
}
