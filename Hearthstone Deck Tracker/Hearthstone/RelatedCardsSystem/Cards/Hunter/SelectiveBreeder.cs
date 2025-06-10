using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class SelectiveBreeder : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.SelectiveBreederCorePlaceholder;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}

public class SelectiveBreederLegacy : SelectiveBreeder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.SelectiveBreederLegacy;
}
