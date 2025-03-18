using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class PendantOfEarth : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Priest.PendantOfEarth;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
