using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Thunderbringer : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Thunderbringer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsElemental() && card.IsBeast(), card.IsElemental(), card.IsBeast());
}
