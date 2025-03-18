using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class GameMasterNemsy : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.GameMasterNemsy;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
