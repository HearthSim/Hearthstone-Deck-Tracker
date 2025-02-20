namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class MessengerBuzzard : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MessengerBuzzard;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
