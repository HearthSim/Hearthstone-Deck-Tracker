using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Stealth minion. Give it this effect for a turn."
public class FollowTheFootsteps : ClassOrNeutralStealthMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.FollowTheFootsteps;
}
