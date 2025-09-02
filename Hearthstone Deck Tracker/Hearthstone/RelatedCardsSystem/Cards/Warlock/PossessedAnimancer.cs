using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class PossessedAnimancer : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.PossessedAnimancer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
