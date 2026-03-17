using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class ArchwitchWillow : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ArchwitchWillow;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsDemon());
}

public class ArchwitchWillowCorePlaceholder : ArchwitchWillow
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ArchwitchWillowCore;
}
