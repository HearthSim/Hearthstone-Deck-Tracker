using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class ChalkArtist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ChalkArtist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
