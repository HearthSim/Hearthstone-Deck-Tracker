using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class BlazingAccretion : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.BlazingAccretion;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE ||
			card.IsElemental());
}
