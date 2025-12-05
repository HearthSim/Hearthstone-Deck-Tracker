using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class LokenJailerOfYoggSaron : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.LokenJailerOfYoggSaron;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);
}
