using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class WidowbloomSeedsman : ICardWithHighlight, ISpellSchoolTutor
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.WidowbloomSeedsman;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.NATURE);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.NATURE };
}

public class WidowbloomSeedsmanCore : WidowbloomSeedsman
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.WidowbloomSeedsmanCorePlaceholder;
}
