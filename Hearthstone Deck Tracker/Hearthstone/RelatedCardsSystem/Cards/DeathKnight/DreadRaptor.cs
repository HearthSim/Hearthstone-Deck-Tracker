using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class DreadRaptor : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.DreadRaptor;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasDeathrattle() && card is { Cost: < 3, Type: "Minion" });
}
