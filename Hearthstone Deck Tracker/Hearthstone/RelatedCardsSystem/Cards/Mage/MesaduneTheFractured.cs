using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class MesaduneTheFractured : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.MesaduneTheFractured;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsElemental());
}
