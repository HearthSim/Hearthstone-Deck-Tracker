using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class NydusWorm : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.NydusWorm;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.ZERG) > 0);
}
