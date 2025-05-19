using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class TinyfinsCaravan : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.TinyfinsCaravan;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsMurloc());
}
