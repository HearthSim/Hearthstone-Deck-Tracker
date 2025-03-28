using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class ScaleReplica : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ScaleReplica;

	// TODO: use deck state to get highest and lowest cost
	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var dragons = deck.Where(c => c.IsDragon()).ToArray();
		if (dragons.Length == 0) 
		{
			return HighlightColor.None;
		}
		var lowestCost = dragons.Min(c => c.Cost);
		var highestCost = dragons.Max(c => c.Cost);
		return HighlightColorHelper.GetHighlightColor(
			card.IsDragon() && card.Cost == highestCost,
			card.IsDragon() && card.Cost == lowestCost
			);
	}
}
