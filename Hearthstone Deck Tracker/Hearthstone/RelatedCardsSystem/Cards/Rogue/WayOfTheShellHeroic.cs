using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class WayOfTheShellHeroic : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Rogue.WayOfTheShellHeroic;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsCreated);
}
