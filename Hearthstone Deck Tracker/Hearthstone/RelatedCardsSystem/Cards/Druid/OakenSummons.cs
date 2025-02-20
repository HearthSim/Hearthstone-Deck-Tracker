namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class OakenSummons : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.OakenSummons;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card is { Type: "Minion", Cost: <= 4 });
}

public class OakenSummonsCore : OakenSummons
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.OakenSummonsCore;
}
