using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class CostumedSinger : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.CostumedSinger;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SECRET) > 0);
}
