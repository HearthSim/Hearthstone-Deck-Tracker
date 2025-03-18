using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class InstrumentTech : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.InstrumentTech;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Weapon");
}
