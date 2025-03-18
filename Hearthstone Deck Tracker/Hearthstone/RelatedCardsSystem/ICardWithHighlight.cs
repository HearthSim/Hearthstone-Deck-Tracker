using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public interface ICardWithHighlight : ICard
{
	HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck);
}
