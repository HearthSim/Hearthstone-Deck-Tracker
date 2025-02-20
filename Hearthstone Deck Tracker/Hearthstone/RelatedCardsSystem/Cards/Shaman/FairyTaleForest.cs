using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class FairyTaleForest : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.FairyTaleForest;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.GetTag(GameTag.BATTLECRY) > 0);
}
