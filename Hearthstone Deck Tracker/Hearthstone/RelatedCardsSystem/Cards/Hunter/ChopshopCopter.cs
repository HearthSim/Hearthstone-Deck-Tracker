using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "After a friendly Mech dies, add a random Mech to your hand."
public class ChopshopCopter : MechMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ChopshopCopter;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
