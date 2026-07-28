using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Poisonous. Battlecry: Discover a Dragon."
public class PrimordialExplorer : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PrimordialExplorer;
}
