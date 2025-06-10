using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class AllianceBannerman : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.AllianceBannerman;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Minion" });
}
