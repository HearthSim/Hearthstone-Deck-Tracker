using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class ElvenMinstrel : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ElvenMinstrel;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");
}

public class ElvenMinstrelCore : ElvenMinstrel
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ElvenMinstrelCorePlaceholder;
}
