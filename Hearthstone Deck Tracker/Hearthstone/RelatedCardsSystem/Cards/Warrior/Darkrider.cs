using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: If you're holding a Dragon, Discover a Dragon with a Dark Gift."
public class Darkrider : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Darkrider;
}
