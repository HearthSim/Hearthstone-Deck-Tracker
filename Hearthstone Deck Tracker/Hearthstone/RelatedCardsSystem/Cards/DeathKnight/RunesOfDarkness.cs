using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a weapon. Spend 3 Corpses to give it +1/+1."
public class RunesOfDarkness : ClassOrNeutralWeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.RunesOfDarkness;
}
