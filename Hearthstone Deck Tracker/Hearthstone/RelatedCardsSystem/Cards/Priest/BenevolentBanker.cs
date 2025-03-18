﻿using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class BenevolentBanker : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.BenevolentBanker;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
