using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class Dreamwarden : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Dreamwarden;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsCreated);
}
