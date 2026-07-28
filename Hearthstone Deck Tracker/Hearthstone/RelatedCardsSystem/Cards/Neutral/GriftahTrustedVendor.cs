using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Get 3 random Legendary cards."
public class AmuletOfTrackingToken2 : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.GriftahTrustedVendor_AmuletOfTrackingToken2;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Rarity: Rarity.LEGENDARY })
			.Select(c => new Card(c));
	}
}

// "Summon a random 4-Cost minion and give it Taunt."
public class AmuletOfCrittersToken2 : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.GriftahTrustedVendor_AmuletOfCrittersToken2;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
