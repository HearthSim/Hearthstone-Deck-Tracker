using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class LokenJailerOfYoggSaron : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.LokenJailerOfYoggSaron;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
