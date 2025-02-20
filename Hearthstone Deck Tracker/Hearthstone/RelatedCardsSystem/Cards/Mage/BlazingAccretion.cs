using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class BlazingAccretion : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.BlazingAccretion;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE ||
			card.IsElemental());
}
