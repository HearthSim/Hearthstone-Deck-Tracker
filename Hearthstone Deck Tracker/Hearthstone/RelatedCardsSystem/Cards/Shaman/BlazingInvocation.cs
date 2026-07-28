using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a Battlecry minion. It costs (1) less."
public class BlazingInvocation : ClassOrNeutralBattlecryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BlazingInvocation;
}

public class BlazingInvocationCore : BlazingInvocation
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BlazingInvocationCore;
}
