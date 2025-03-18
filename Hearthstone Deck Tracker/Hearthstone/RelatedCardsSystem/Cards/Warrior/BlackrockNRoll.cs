using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class BlackrockNRoll : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.BlackrockNRoll;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
