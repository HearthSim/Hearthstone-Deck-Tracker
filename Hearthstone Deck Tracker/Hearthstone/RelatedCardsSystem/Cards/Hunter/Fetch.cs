﻿using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class Fetch : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Fetch;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.IsBeast(),
			card.Type == "Minion",
			card.Type == "Spell"
		);
}
