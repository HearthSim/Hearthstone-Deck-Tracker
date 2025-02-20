using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class Turbulus : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Turbulus;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.BATTLECRY) > 0);
}
