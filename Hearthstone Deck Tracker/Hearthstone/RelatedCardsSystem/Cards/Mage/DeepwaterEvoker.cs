using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class DeepwaterEvoker : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.DeepwaterEvoker;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
