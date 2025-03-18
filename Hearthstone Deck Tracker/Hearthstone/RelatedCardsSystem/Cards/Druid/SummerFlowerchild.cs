using System.Collections.Generic;

using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class SummerFlowerchild : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.SummerFlowerchild;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Cost >= 6);
}
