using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class PluckyPaintfin : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PluckyPaintfin;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.RUSH) > 0);
}
