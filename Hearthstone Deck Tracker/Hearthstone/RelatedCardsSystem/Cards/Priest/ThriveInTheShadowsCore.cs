using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class ThriveInTheShadowsCore : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ThriveInTheShadowsCorePlaceholderPLACEHOLDER_2022042;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);
}
