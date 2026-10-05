using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Draw a different spell. If it's a Nature spell, it casts twice."
public class Efflorescence : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.Efflorescence;

	// Nature spells first: they are the ones that cast twice.
	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		if(card.Id == GetCardId())
			return HighlightColor.None;
		return HighlightColorHelper.GetHighlightColor(
			card.TypeEnum == CardType.SPELL && card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.NATURE,
			card.TypeEnum == CardType.SPELL
		);
	}
}
