namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock.SymphonyOfSins;

public class MovementOfPride : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.SymphonyofSins_MovementOfPrideToken;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
