using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Whenever your hero takes damage on your turn, summon a random 3-Cost minion."
public class DiseasedVulture : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DiseasedVulture;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
