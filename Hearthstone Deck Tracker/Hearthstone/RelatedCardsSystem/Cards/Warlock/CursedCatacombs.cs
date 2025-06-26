using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class CursedCatacombs : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.CursedCatacombs;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
