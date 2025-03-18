using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class BlindeyeSharpshooter : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.BlindeyeSharpshooter;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.IsNaga(),
			card.Type == "Spell");
}
