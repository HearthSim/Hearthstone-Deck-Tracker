using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a Nature spell. If you play it this turn, Discover another."
// The second Discover is conditional.
public class LightningReflexes : ClassOrNeutralNatureSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.LightningReflexes;
}
