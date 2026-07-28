using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Spell Damage +2 Battlecry: Discover a Dragon."
public class AzureExplorer : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.AzureExplorer;
}
