namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class TramOperator : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TramOperator;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsMech());
}
