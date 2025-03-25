using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class FinjaTheFlyingStar : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FinjaTheFlyingStar;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsMurloc());
}

public class FinjaTheFlyingStarCorePlaceholder : FinjaTheFlyingStar
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FinjaTheFlyingStarCore;
}
