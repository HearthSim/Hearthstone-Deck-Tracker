using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class RotheartDryad : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.RotheartDryad;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Minion", Cost: >= 7 });
}
