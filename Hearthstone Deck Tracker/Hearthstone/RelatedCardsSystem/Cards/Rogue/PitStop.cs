using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class PitStop : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.PitStop;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsMech());
}
