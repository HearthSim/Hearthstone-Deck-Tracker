using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class ElementalAllies : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.ElementalAllies;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
