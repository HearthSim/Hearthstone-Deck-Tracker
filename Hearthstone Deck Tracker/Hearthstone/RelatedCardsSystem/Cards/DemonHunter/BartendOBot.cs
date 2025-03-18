using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class BartendOBot : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.BartendOBot;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.OUTCAST) > 0);
}
