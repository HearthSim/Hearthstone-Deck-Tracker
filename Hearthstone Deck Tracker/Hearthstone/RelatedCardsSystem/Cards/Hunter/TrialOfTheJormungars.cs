using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class TrialOfTheJormungars : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TrialOfTheJormungars;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast() && card.Cost <= 3);
}
