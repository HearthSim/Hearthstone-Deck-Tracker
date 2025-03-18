using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class GroveShaper : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.GroveShaper;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL)  == (int)SpellSchool.NATURE);
}
