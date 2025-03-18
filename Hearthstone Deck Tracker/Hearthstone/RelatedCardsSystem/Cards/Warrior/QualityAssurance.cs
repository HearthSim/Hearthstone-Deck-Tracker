using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class QualityAssurance : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.QualityAssurance;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.GetTag(GameTag.TAUNT) > 0);
}
