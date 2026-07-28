using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Secret from another class."
public class SketchyStranger : OffClassSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SketchyStranger;
}

public class SketchyStrangerCore : SketchyStranger
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SketchyStrangerCorePlaceholder;
}
