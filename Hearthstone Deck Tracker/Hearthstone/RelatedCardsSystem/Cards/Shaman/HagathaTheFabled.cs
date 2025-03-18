using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class HagathaTheFabled : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.HagathaTheFabled;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Spell", Cost: >= 5 });
}
