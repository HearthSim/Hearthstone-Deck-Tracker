using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class ChorusRiff : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ChorusRiff;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);
}
