using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Lifesteal Battlecry: Discover a Dragon."
public class BronzeExplorer : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BronzeExplorer;
}

public class BronzeExplorerCorePlaceholder : BronzeExplorer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BronzeExplorerCorePlaceholder;
}
