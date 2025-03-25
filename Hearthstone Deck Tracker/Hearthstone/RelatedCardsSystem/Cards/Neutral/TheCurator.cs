using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class TheCurator : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TheCurator;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast(), card.IsDragon(), card.IsMurloc());
}

public class TheCuratorCorePlaceholder : TheCurator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TheCuratorCore;

}
