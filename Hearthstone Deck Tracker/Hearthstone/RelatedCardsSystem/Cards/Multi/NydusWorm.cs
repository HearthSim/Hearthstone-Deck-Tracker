using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class NydusWorm : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.NydusWorm;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.ZERG) > 0);
}
