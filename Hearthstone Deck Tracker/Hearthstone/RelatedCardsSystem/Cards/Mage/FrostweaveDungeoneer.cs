using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class FrostweaveDungeoneer : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.FrostweaveDungeoneer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST,
			card.TypeEnum == CardType.SPELL);
}
