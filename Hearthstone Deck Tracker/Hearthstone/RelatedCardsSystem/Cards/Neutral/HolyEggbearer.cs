using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class HolyEggbearer: ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.HolyEggbearer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { TypeEnum: CardType.MINION, Attack: 0 });
}
