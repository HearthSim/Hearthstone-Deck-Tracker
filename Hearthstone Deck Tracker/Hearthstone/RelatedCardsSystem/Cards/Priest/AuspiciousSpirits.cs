
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Summon a random 4-Cost minion. Corrupt: Summon a 7-Cost minion instead."
public class AuspiciousSpirits : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.AuspiciousSpirits;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
