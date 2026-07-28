using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Add a random minion to your hand. It costs (3) less."
public class UnstablePortal : MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.UnstablePortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
