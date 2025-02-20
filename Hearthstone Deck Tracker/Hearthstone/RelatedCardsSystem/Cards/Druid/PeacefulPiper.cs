namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class PeacefulPiper : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.PeacefulPiper;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
