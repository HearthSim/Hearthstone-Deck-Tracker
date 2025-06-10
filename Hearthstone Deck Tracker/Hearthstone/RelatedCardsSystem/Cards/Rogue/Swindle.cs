using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class Swindle : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Swindle;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell", card.Type == "Minion");
}
