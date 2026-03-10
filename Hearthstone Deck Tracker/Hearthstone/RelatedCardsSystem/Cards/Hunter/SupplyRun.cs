using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class SupplyRun : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.SupplyRun;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);
}

public class SupplyRunShattered : SupplyRun
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.SupplyRun_SupplyRunToken1;
}
