using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Taunt. Deathrattle: Summon a random 8-Cost minion."
public class TravelSecurity : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.TravelSecurity;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
