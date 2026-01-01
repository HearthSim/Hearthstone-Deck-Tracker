using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class Finality : ICardWithHighlight
{
	public string GetCardId() => "HearthDb.CardIds.Collectible.Deathknight.Finality";

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsUndead());
}
