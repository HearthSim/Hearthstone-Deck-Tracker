namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class AlarmedSecuritybot : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.AlarmedSecuritybot;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.Type == "Minion" && card.Id != GetCardId());
}
