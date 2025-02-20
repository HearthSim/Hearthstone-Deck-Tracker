namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock.SymphonyOfSins;

public class MovementOfGluttony : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.SymphonyofSins_MovementOfGluttonyToken;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
