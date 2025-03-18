using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class ChronoBoost : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.ChronoBoost;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
				HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.PROTOSS) > 0);
}
