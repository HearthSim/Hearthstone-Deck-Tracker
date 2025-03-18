using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class ArkoniteRevelation : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.ArkoniteRevelation;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
