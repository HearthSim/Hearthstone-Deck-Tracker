namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class TakeToTheSkies : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.TakeToTheSkies;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsDragon());
}
