using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class SpiritGuide : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Priest.SpiritGuide;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY,
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW
			);
}

public class SpiritGuideCorePlaceholder : SpiritGuide
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SpiritGuideCorePlaceholder;
}
