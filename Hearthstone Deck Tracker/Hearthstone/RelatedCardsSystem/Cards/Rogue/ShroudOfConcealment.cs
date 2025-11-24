using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class ShroudOfConcealment : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ShroudOfConcealment;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
