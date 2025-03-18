using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class EmbraceOfNature : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.EmbraceOfNature;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.CHOOSE_ONE) > 0);
}
