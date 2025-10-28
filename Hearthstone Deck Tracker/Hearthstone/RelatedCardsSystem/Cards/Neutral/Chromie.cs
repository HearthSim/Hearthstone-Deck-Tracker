using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Chromie : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Chromie;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var cardsPlayedThisGame = Core.Game.Player.CardsPlayedThisMatch
			.Select(e => e.CardId)
			.WhereNotNull()
			.ToHashSet();

		return HighlightColorHelper.GetHighlightColor(cardsPlayedThisGame.Contains(card.Id));
	}
}
