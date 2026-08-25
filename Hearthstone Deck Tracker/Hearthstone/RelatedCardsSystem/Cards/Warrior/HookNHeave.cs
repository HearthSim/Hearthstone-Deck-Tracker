using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a Pirate. Summon two 1/1 Cannoneers."
public class HookNHeave : ClassOrNeutralPirateMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.HookNHeave;
}
