using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class CactusCutter : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.CactusCutter;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
