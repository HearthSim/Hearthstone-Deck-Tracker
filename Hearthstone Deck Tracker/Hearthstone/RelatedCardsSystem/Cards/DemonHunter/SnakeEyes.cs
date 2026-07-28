using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a 1-Cost card."
public class SnakeEyesRolledAOneToken : ClassOrNeutralCost1CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledAOneToken;
}

// "Discover a 2-Cost card."
public class SnakeEyesRolledATwoToken : ClassOrNeutralCost2CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledATwoToken;
}

// "Discover a 3-Cost card."
public class SnakeEyesRolledAThreeToken : ClassOrNeutralCost3CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledAThreeToken;
}

// "Discover a 4-Cost card."
public class SnakeEyesRolledAFourToken : ClassOrNeutralCost4CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledAFourToken;
}

// "Discover a 5-Cost card."
public class SnakeEyesRolledAFiveToken : ClassOrNeutralCost5CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledAFiveToken;
}

// "Discover a 6-Cost card."
public class SnakeEyesRolledASixToken : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.SnakeEyes_RolledASixToken;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.Cost == 6 && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
