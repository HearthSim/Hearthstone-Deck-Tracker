using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Deal $3 damage to a minion. If it dies, Discover a Frost Rune card."
public class FrostStrikeCore : FrostRuneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.FrostStrikeCore;
}

