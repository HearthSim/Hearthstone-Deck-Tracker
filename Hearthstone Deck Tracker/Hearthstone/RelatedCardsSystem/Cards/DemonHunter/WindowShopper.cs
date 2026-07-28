using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Miniaturize. Battlecry: Discover a Demon."
public class WindowShopper : ClassOrNeutralDemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.WindowShopper;
}
