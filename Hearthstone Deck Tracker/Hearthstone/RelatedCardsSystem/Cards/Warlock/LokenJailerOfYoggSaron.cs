namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class LokenJailerOfYoggSaron : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.LokenJailerOfYoggSaron;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}
