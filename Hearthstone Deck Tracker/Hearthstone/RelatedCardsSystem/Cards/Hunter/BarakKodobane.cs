using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class BarakKodobane : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BarakKodobane;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Spell", Cost: 1},
			card is { Type: "Spell", Cost: 2},
			card is { Type: "Spell", Cost: 3}
		);
}

public class BarakKodobaneCore : BarakKodobane
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BarakKodobaneCore;
}
