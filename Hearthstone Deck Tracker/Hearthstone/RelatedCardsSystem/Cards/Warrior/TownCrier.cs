using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class TownCrier : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Warrior.TownCrier;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.RUSH) > 0);
}

public class TownCrierCore : TownCrier
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.TownCrierCorePlaceholder;
}
