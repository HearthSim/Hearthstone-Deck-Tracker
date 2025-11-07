using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class GaronaHalforcenTheKingslayers : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Rogue.GaronaHalforcen_TheKingslayersToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Rarity == Rarity.LEGENDARY ||
		                                       (card.Rarity == Rarity.INVALID && card.GetTag(GameTag.ELITE) > 0));
}
