using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class CattleRustler : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.CattleRustler;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
