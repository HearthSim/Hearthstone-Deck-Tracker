using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class BargainBin : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BargainBin;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.Type == "Spell",
			card.Type == "Minion",
			card.Type == "Weapon"
			);
}
