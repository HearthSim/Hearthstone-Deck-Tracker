using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class Insight : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Priest.Insight;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}

public class InsightCorrupted : Insight
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Priest.Insight_InsightToken;
}
