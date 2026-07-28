using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Choose One - Draw a Beast; or Discover one."
public class PeacefulPiper : ClassOrNeutralBeastMinionPool, ICardWithHighlight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.PeacefulPiper;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
