using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class FaeTrickster : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FaeTrickster;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Spell", Cost: >= 5 });
}
