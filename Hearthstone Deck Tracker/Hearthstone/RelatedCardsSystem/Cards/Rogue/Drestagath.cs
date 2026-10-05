using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Deathrattle: Draw an Old God. If you can't, get a random Old God."
public class Drestagath : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Drestagath;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTag(GameTag.OLD_GOD));
}
