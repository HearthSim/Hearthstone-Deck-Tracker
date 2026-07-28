using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Druid minion. If your hand has no minions, keep all 3 instead."
public class CrystalsongPortal : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.CrystalsongPortal;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Druid"))
			.Select(c => new Card(c));
	}
}
