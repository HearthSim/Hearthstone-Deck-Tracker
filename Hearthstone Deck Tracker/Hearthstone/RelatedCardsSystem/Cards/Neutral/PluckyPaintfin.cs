using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class PluckyPaintfin : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PluckyPaintfin;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTag(GameTag.RUSH));
}
