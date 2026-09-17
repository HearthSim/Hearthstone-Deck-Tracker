using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Draw 2 spells. Refresh 6 Mana Crystals. You can only play spells for the rest of the turn."
public class ArrivalOfTheTitans : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.ArrivalOfTheTitans;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);
}
