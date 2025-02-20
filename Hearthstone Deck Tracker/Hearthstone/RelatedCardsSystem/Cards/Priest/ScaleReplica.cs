namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class ScaleReplica : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ScaleReplica;

	// TODO: use deck state to get highest and lowest cost
	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsDragon());
}
