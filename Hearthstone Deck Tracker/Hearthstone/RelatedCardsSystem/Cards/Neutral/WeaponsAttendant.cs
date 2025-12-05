using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class WeaponsAttendant : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WeaponsAttendant;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.WEAPON, card.IsPirate());
}
