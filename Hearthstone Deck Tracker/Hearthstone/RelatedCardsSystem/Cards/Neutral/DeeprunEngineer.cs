using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Mech. It costs (1) less."
public class DeeprunEngineer : ClassOrNeutralMechMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DeeprunEngineer;
}
