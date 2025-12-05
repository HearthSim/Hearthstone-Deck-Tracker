using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class FlightOfTheFirehawk : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.FlightOfTheFirehawk;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION && !card.IsEmptyRace());
}
