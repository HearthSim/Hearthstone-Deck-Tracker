using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random 3-Cost minion."
public class StubbornSuspect : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.StubbornSuspect;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
