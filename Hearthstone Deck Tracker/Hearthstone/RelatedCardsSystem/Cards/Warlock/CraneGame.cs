namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class CraneGame : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.CraneGame;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
