using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class CostumedSinger : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.CostumedSinger;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SECRET) > 0);
}
