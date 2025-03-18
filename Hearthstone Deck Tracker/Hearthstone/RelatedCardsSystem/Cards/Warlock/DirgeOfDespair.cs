using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class DirgeOfDespair : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DirgeOfDespair;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
