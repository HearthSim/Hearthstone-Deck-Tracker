using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Draw a minion. Give Beasts in your hand +2/+2."
public class TitanSized : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.TitanSized;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.TypeEnum == CardType.MINION
		);
}
