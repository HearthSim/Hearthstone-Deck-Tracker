using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class TwilightDeceptor : ICardWithHighlight, ISpellSchoolTutor
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.TwilightDeceptor;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.SHADOW };
}
