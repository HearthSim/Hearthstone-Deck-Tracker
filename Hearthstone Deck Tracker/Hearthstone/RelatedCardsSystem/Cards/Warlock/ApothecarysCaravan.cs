using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class ApothecarysCaravan : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ApothecarysCaravan;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Minion", Cost: 1 });
}
