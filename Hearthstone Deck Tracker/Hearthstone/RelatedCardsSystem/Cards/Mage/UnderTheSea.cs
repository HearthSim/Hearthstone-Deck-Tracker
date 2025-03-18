using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class UnderTheSea : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.UnderTheSea;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell" && card.Id != GetCardId());
}
