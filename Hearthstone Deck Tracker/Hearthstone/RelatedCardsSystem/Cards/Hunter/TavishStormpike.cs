using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class TavishStormpike : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TavishStormpike;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
