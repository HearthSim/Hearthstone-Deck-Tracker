namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class GameMasterNemsy : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.GameMasterNemsy;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
