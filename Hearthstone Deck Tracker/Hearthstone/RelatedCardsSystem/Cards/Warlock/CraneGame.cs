using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class CraneGame : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.CraneGame;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
