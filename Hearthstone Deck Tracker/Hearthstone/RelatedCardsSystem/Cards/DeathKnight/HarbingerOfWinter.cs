using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class HarbingerOfWinter : ICardWithHighlight, ISpellSchoolTutor
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.HarbingerOfWinterCore;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.FROST };
}

public class HarbingerOfWinterCore : HarbingerOfWinter
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.HarbingerOfWinterCore;

}
