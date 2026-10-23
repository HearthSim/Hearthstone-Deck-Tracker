using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Discover two 8-Cost minions. Summon them."
public class TheAzeriteOx : ClassOrNeutralCost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Warrior.KoboldMiner_TheAzeriteOxToken;
	public override int EventCount() => 2;
}
