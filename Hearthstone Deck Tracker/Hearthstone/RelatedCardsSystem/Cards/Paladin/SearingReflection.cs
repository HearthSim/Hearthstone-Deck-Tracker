using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class SearingReflection : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SearingReflection;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.TypeEnum == CardType.MINION);
}
