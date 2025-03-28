using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock.SymphonyOfSins;

public class MovementOfPride : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.SymphonyofSins_MovementOfPrideToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var minions = deck.Where(c => c.Type == "Minion").ToArray();
		if(minions.Length == 0)
		{
			return HighlightColor.None;
		}
		var highestCost = minions.Max(c => c.Cost);
		return HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.Cost == highestCost);
	}
}
