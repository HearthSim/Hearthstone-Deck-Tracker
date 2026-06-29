using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class SketchyInformation : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.SketchyInformation;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasDeathrattle() && card.Cost <= 4);
}
