using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class RedscaleDragontamer : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Paladin.RedscaleDragontamer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDragon());
}

public class RedscaleDragontamerCorePlaceholder : RedscaleDragontamer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.RedscaleDragontamerCore;

}
