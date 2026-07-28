using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover 2 minions. If your deck has no minions, reduce the Cost of any in your hand by (2)."
public class Solitude : DiscoverPoolCard, ICardWithHighlight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Solitude;
	public override int Picks() => 3;
	public override int EventCount() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.MINION);
}
