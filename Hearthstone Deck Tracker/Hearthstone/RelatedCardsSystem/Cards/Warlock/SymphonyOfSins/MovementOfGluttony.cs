using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock.SymphonyOfSins;

public class MovementOfGluttony : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.SymphonyofSins_MovementOfGluttonyToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
