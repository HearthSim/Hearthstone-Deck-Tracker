using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: Discover a Blood, Frost or Unholy card. Gain Lifesteal, Reborn or Rush based on your choice."
// The Discover offers one option per rune. Modelled as three single draws from the union of all rune
// cards — an approximation, since each option actually comes from its own rune's sub-pool.
public class ScionOfInsight : RuneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.ScionOfInsight;
	public override int Picks() => 1;
	public override int EventCount() => 3;
}
