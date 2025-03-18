using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Arcanologist : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Mage.Arcanologist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SECRET) > 0);
}

public class ArcanologistCore : Arcanologist
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ArcanologistCore;
}
