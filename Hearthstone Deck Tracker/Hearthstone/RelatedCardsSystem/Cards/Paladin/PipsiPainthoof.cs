using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class PipsiPainthoof : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.PipsiPainthoof;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.DIVINE_SHIELD) > 0,
			card.GetTag(GameTag.RUSH) > 0,
			card.GetTag(GameTag.TAUNT) > 0
		);
}
