using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class Grillmaster : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Grillmaster;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		if(!deck.Any())
		{
			return HighlightColor.None;
		}

		var lowestCost = deck.Min(c => c.Cost);
		var highestCost = deck.Max(c => c.Cost);
		return HighlightColorHelper.GetHighlightColor(
			card.Cost == highestCost,
			card.Cost == lowestCost
		);
	}
}
