using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class RushTheStage : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.RushTheStage;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.RUSH) > 0);
}
