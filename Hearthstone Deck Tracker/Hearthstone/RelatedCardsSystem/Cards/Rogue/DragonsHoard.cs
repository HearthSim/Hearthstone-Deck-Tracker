using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Legendary minion from another class."
public class DragonsHoard : OffClassLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.DragonsHoard;
}
