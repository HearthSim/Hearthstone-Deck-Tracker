namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class CreationProtocol : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.CreationProtocol;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
