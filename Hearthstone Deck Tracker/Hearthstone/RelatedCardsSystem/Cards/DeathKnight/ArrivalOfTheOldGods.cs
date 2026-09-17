using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Draw 2 minions. Refresh 5 Mana Crystals. You can only play minions for the rest of the turn."
public class ArrivalOfTheOldGods : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.ArrivalOfTheOldGods;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);
}
