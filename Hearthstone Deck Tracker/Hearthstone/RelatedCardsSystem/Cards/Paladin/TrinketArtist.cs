using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class TrinketArtist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.TrinketArtist;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.PALADIN_AURA) > 0,
			card.GetTag(GameTag.DIVINE_SHIELD) > 0);
}
