using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class StoryOfBarnabus : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.StoryOfBarnabus;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Minion", Attack: >= 5},
			card is { Type: "Minion"});
}
