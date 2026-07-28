using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Rush. Battlecry: Fill your opponent's board with random 1-Cost minions."
public class UndefeatedChampion : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.UndefeatedChampion;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => BoardFill.OpponentSlots;
}
