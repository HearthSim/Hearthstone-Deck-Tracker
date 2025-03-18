using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class Turbulus : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Turbulus;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.BATTLECRY) > 0);
}
