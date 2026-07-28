using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: Discover an Undead. Quickdraw: It costs (2) less."
public class FarmHand : ClassOrNeutralUndeadMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.FarmHand;
}
