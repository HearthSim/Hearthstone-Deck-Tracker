using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class PeacefulPiper : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.PeacefulPiper;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
