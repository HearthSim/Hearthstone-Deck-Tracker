using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class NightmareLordXavius : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NightmareLordXavius;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
