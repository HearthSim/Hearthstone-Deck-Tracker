using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class MysteryEgg : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MysteryEgg;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}

public class MysteryEggMini : MysteryEgg
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.MysteryEgg_MysteryEggToken;
}
