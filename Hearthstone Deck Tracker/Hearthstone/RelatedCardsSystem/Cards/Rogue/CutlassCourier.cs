using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class CutlassCourier : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.CutlassCourier;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsPirate(), card.Type == "Minion");
}
