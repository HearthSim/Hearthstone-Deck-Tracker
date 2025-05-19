using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class SwordOfTheFallen : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SwordOfTheFallen;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SECRET) > 0);
}
