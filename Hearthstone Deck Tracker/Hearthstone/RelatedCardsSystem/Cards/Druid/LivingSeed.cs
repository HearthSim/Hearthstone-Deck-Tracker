using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class LivingSeed : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.LivingSeedRank1;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}

public class LivingSeedRank2 : LivingSeed
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.LivingSeedRank1_LivingSeedRank2Token;

}

public class LivingSeedRank3 : LivingSeed
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.LivingSeedRank1_LivingSeedRank3Token;

}
