using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Battlecry: Discover a Taunt minion."
public class StonehillDefender : ClassOrNeutralTauntMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.StonehillDefender;
}

public class StonehillDefenderCore : StonehillDefender
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.StonehillDefenderCore;
}
