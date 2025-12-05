using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class Tuskpiercer : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Tuskpiercer;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION && card.HasDeathrattle());
}

public class TuskpiercerCorePlaceholder : Tuskpiercer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.TuskpiercerCore;
}
