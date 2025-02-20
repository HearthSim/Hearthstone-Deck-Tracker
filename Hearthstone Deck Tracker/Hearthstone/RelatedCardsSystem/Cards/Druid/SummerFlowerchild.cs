namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class SummerFlowerchild : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.SummerFlowerchild;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Cost >= 6);
}
