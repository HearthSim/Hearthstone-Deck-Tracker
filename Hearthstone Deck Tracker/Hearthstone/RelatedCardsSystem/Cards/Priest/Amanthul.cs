using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Titan After this uses an ability, Discover any Legendary minion from any class."
public class Amanthul : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.Amanthul;
}
