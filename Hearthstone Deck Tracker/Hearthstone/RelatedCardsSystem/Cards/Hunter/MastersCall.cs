using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class MastersCall : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MastersCall;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast(), card.TypeEnum == CardType.MINION);
}

public class MastersCallCore : MastersCall
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MastersCallCore;
}
