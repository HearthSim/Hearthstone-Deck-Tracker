using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class TenGallonHat : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TenGallonHat;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
