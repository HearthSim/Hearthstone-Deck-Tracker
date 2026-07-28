using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Discover a Demon. Your next one costs (1) less."
public class DemonicStudies : ClassOrNeutralDemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DemonicStudies;
}

public class DemonicStudiesCorePlaceholder : DemonicStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DemonicStudiesCorePlaceholder;
}
