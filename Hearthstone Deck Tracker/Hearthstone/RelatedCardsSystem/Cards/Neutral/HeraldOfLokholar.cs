using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class HeraldOfLokholar : ICardWithHighlight, ISpellSchoolTutor
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.HeraldOfLokholar;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.FROST };
}
