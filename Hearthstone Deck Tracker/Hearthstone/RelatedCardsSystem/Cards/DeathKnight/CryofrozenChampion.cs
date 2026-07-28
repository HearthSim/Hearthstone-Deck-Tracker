using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Deathrattle: Get a random Legendary minion. Reduce its Cost by (1)."
public class CryofrozenChampion : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.CryofrozenChampion;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
