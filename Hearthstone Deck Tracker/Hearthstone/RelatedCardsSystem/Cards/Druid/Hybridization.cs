using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class Hybridization : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.Hybridization;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is {Type: "Minion", Cost: 1 or 4},
			card is {Type: "Minion", Cost: 2},
			card is {Type: "Minion", Cost: 3}
			);
}
