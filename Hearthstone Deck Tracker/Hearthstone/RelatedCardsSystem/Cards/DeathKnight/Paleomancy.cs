using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover an Undead. Spend 5 Corpses to keep all 3 instead."
// The keep-all is conditional, so sampling stays default.
public class Paleomancy : ClassOrNeutralUndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.Paleomancy;
}
