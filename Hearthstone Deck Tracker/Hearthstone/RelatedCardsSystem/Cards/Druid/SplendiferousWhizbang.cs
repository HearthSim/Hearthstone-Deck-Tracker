using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Druid spell, a Druid minion, or a Neutral minion you can afford to play."
public class MomentOfDiscoveryToken : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.SplendiferousWhizbang_MomentOfDiscoveryToken;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c =>
				(c.IsClass(playerClass) && (c.Type == CardType.SPELL || c.Type == CardType.MINION)) ||
				(c.IsClass("Neutral") && c.Type == CardType.MINION)
			)
			.Select(c => new Card(c));
	}
}
