using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class Blink : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Blink;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION && card.GetTag(GameTag.PROTOSS) > 0);
}
