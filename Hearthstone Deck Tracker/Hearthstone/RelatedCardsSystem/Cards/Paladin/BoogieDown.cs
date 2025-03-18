using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class BoogieDown : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BoogieDown;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Minion", Cost: 1 });
}
