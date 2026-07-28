using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover an Undead. Spend 2 Corpses to give it a Dark Gift."
public class RiteOfAtrocity : ClassOrNeutralUndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.RiteOfAtrocity;
}
