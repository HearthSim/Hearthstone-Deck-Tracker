using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class IcebloodTower : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.IcebloodTower;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL && card.Id != GetCardId());
}
