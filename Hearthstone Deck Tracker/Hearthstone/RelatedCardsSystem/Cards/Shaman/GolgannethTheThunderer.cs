using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class GolgannethTheThunderer : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.GolgannethTheThunderer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.OVERLOAD) > 0);
}
