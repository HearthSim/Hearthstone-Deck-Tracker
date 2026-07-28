using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you control another Mech, Discover a Mech."
public class GorillabotA3 : ClassOrNeutralMechMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GorillabotA3;
}

public class GorillabotA3Core : GorillabotA3
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GorillabotA3Core;
}
