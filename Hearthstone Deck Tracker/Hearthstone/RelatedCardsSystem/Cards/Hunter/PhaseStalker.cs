using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class PhaseStalker : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PhaseStalker;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SECRET) > 0);
}
