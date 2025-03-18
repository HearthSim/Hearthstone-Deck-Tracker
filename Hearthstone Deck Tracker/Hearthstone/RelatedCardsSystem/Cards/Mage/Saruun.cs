using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Saruun : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Mage.Saruun;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsElemental());
}
