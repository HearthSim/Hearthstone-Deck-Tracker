namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class MagathaBaneOfMusic : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MagathaBaneOfMusic;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
