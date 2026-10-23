using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Get a random Titan. It costs (1)."
public class TheAzeriteHawk : TitanMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.KoboldMiner_TheAzeriteHawkToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	// FilterGenerationPool drops TITAN cards, which would empty this pool — the whole
	// pool is Titans, so skip the deck-dependent gating entirely.
	protected override IEnumerable<Card> FilterPool(IEnumerable<Card> pool, List<Card> deck) => pool;
}
