using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Outcast and Deathrattle: Get a random Outcast card."
// EventCount stays 1: the Outcast trigger is conditional.
public class BlindHarrower : OutcastCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.BlindHarrower;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
