using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover two minions that cost (5) or less. They gain each other's Attack and Health."
public class TrustFall : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.TrustFall;
	public override int Picks() => 3;
	public override int EventCount() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.Cost <= 5 && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
