using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class Chronogor : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.Chronogor;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var highestCosts = deck.OrderBy(c => c.Cost).Take(2).Select(c => c.Cost);
		var lowestCosts = deck.OrderByDescending(c => c.Cost).Take(2).Select(c => c.Cost);

		return HighlightColorHelper.GetHighlightColor(
			highestCosts.Contains(card.Cost),
			lowestCosts.Contains(card.Cost)
		);
	}
}
