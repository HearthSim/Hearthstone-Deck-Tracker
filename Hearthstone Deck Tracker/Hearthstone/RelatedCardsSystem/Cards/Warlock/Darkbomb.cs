using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class Darkbomb : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkbombGVG;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW);
}

public class DarkbombWONDERS : Darkbomb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkbombWONDERS;

}
