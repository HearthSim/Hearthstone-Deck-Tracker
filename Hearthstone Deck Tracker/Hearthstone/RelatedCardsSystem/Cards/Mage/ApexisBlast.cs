using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $5 damage. If your deck has no minions, summon a random 5-Cost minion."
public class ApexisBlast : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ApexisBlast;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
