using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class MalygosTheSpellweaverCore : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MalygosTheSpellweaverCore;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
