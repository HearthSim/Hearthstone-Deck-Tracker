using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class ForgeOfSouls : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ForgeOfSouls;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.WEAPON);
}
