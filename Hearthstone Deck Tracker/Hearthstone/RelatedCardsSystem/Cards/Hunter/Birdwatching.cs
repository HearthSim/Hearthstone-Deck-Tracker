namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class Birdwatching : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Birdwatching;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
