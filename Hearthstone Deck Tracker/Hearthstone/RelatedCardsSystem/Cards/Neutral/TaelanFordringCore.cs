using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class TaelanFordringCore : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TaelanFordringCore;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var minions = deck.Where(c => c.Type == "Minion").ToArray();
		var highestCost = minions.Max(c => c.Cost);
		return HighlightColorHelper.GetHighlightColor(
			card.Type == "Minion" && card.Cost == highestCost
		);
	}
}
