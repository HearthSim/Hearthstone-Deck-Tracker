using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Freeze an enemy. Summon a random 5-Cost minion and Freeze it."
public class ForebodingFreeze : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ForebodingFreeze;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
