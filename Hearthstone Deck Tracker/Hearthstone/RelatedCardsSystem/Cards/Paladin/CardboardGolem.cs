using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class CardboardGolem : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.CardboardGolem;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.PALADIN_AURA) > 0);
}
