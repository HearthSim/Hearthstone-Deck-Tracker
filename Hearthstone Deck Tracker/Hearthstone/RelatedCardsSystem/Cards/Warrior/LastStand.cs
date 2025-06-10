using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class LastStand : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.LastStand;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTaunt());
}
