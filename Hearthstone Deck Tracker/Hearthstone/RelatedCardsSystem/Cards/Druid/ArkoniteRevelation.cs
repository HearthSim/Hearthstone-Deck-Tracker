namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class ArkoniteRevelation : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.ArkoniteRevelation;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
