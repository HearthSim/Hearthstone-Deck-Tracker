using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a Mech. If you have 10 Mana Crystals, keep all 3 instead."
public class OmegaAssembly : ClassOrNeutralMechMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.OmegaAssembly;
}
