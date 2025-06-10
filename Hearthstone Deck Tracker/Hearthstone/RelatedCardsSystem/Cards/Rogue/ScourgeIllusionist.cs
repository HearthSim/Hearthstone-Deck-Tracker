using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class ScourgeIllusionist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ScourgeIllusionist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasDeathrattle() && card.Type == "Minion" && card.Id != GetCardId());
}
