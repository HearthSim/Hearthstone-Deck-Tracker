using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class DigForTreasure : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.DigForTreasure;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsPirate(), card.TypeEnum == CardType.MINION);
}
