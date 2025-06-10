using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class CaptainsParrot : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.CaptainsParrotLegacy;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsPirate());
}

public class CaptainsParrotVanilla : CaptainsParrot
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.CaptainsParrotVanilla;
}
