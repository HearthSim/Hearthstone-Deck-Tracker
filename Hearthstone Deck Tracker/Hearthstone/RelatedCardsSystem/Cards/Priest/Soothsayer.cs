using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Prepare, Taunt. Deathrattle: Restore 6 Health your hero. Summon a random 6-Cost minion."
public class Soothsayer : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.Soothsayer;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
