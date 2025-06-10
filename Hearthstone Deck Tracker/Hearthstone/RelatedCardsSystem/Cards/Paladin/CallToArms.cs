using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class CallToArms : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.CallToArms;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Minion", Cost: <= 2 });
}
