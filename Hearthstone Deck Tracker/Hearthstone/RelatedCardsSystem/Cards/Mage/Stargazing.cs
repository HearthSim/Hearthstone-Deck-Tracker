using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Stargazing : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.Stargazing;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.ARCANE);
}
