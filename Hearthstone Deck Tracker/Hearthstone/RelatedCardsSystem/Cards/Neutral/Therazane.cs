using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Therazane : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Therazane;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsElemental());
}
