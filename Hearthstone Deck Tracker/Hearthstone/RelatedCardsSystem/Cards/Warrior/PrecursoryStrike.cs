using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class PrecursoryStrike : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.PrecursoryStrike;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
