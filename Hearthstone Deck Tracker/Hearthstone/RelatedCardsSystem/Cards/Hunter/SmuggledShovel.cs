using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class SmuggledShovel : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.SmuggledShovel;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card is { IsCreated: true, TypeEnum: CardType.SPELL });
}
