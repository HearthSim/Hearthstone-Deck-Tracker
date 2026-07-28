using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover any Demon that costs (5) or more." — "any" explicitly makes the pool cross-class
public class TheEternalHold : DiscoverPoolCard, ICardWithHighlight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.TheEternalHold;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: >= 5 } && c.IsDemon())
			.Select(c => new Card(c));
	}
}
