using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class GelbinOfTomorrow : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.GelbinOfTomorrow;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.PALADIN_AURA) > 0);
}
