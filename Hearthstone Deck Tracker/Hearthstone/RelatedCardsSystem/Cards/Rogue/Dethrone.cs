using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Destroy a minion. Combo: Summon a random 8-Cost minion."
public class Dethrone : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Dethrone;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
