using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class PetCollector : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PetCollector;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast() && card.Cost <= 5);
}
