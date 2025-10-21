using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class LivingFlame : ICardWithHighlight, ISpellSchoolTutor
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LivingFlame;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.FIRE };

}
