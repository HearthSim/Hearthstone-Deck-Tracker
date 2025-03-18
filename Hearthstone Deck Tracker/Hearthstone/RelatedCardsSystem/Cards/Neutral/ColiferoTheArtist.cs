using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class ColiferoTheArtist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ColiferoTheArtist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
