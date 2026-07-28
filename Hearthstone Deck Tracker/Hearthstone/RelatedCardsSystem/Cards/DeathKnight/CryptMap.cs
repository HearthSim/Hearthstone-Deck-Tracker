using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a Frost Rune card. If you play it this turn, also pick one of the others."
public class CryptMap : FrostRuneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.CryptMap;
}
