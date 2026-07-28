using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell from any class. It costs (1) less."
// Standard Discover sampling.
public class PrismaticElemental : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.PrismaticElemental;
	public override int Picks() => 3;
	public override int EventCount() => 1;
	public override bool IsWithReplacement() => false;
}
