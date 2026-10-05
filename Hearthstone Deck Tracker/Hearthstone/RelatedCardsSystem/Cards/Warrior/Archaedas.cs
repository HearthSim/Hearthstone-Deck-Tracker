using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Discover a spell. Transform it into a 3/3 weapon of the same cost that casts it each attack."
public class Archaedas : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Archaedas;
}
