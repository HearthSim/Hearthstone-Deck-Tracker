using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class WatercolorArtist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.WatercolorArtist;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST);
}
