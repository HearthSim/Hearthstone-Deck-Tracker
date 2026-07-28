using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Give your minions 'Deathrattle: Summon a random 4-Cost minion.'"
public class TalanjisLastStand : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.TalanjisLastStand;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
