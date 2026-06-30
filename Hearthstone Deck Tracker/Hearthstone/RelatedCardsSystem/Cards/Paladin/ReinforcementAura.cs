using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class ReinforcementAura : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ReinforcementAura;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { TypeEnum: CardType.MINION, Cost: <= 2 });
}
