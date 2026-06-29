using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class IceFishing : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Shaman.IceFishing;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsMurloc());
}

public class IceFishingCorePlaceholder : IceFishing
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.IceFishingCorePlaceholder;
}
