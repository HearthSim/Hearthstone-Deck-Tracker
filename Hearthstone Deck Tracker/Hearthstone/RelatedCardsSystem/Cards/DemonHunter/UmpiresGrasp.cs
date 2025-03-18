using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class UmpiresGrasp : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.UmpiresGrasp;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}
