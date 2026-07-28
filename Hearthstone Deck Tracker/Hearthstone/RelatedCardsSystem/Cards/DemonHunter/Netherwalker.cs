using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Battlecry: Discover a Demon."
public class Netherwalker : ClassOrNeutralDemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Netherwalker;
}

public class NetherwalkerCore : Netherwalker
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.NetherwalkerCore;
}
