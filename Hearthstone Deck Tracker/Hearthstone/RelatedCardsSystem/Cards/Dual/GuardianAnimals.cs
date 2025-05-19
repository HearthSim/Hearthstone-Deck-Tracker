using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class GuardianAnimals : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.GuardianAnimals;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast() && card.Cost <= 5);
}
