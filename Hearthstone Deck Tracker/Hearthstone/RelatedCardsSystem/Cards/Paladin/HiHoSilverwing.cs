using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class HiHoSilverwing : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.HiHoSilverwing;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL)  == (int)SpellSchool.HOLY);
}
