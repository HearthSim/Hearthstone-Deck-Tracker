namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class ColiferoTheArtist : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ColiferoTheArtist;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
