using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class Blink : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Blink;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.GetTag(GameTag.PROTOSS) > 0);
}
