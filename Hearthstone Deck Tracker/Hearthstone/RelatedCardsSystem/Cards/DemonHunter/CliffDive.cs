using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class CliffDive : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.CliffDive;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
