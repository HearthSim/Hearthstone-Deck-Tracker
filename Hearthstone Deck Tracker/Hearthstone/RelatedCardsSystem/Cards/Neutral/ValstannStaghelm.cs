using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class ValstannStaghelm : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ValstannStaghelm;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTaunt());
}
