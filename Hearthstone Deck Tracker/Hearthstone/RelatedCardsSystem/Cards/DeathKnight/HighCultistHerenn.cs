using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class HighCultistHerenn : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.HighCultistHerenn;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.HasDeathrattle());
}
