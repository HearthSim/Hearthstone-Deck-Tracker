using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class MuradinHighKing : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MuradinHighKing;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Id == HearthDb.CardIds.NonCollectible.Shaman.MuradinHighKing_HighKingsHammerToken);
}
