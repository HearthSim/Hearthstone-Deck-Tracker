using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class TrustyFishingRod : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TrustyFishingRod;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Minion", Cost: 1 });
}
