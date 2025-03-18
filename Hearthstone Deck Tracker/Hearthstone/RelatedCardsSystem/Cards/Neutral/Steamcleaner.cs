using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Steamcleaner : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Steamcleaner;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsCreated);
}

public class SteamcleanerCore : Steamcleaner
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SteamcleanerCorePlaceholder;
}
