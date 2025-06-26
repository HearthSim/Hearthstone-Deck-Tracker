using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class VioletTreasuregill : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.VioletTreasuregill;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Spell", Cost: <= 2 });
}
