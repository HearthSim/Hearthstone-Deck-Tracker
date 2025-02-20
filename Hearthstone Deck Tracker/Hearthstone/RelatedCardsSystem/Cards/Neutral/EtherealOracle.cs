namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class EtherealOracle : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.EtherealOracle;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
