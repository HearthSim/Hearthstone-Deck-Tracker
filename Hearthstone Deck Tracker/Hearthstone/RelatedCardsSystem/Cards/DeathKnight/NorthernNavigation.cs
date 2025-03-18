using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class NorthernNavigation : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.NorthernNavigation;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST,
			card.Type == "Spell");
}
