using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class ChronoBoost : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.ChronoBoost;

	public HighlightColor ShouldHighlight(Card card) =>
				HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.PROTOSS) > 0);
}
