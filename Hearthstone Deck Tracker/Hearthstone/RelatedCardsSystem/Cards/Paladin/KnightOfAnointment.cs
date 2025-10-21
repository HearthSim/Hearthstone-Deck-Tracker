using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class KnightOfAnointment : ICardWithHighlight, ISpellSchoolTutor
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.KnightOfAnointment;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.HOLY };
}
