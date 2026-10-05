using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Summon a random minion that costs (8) or more. Get a copy of it."
// Y'Shaarj, Titan Bane's ability.
public class SpawnOfYshaarj : CostAtLeast8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_SpawnOfYshaarjToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
