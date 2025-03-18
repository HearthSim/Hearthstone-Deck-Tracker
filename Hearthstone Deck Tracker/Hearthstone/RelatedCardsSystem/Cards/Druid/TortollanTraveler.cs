using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class TortollanTraveler : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.TortollanTraveler;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTaunt() && card.Id != GetCardId());
}
