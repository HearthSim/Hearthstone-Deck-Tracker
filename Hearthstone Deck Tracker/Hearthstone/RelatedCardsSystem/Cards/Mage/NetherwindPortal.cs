using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Secret: After your opponent casts a spell, summon a random 4-Cost minion."
public class NetherwindPortal : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.NetherwindPortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
