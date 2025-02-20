﻿namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class UngoroBrochure : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.UngoroBrochure;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}

public class UngoroBrochureSpell : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.UnGoroBrochure_DalaranBrochureToken;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
