using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random Dragon to your hand."
public class BoneDrake : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BoneDrake;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}

public class BoneDrakeCorePlaceholder : BoneDrake
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BoneDrakeCorePlaceholder;
}
