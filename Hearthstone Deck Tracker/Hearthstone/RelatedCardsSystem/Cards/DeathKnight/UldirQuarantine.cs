using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Locked until you play a card from each Rune. Summon a random 5-Cost minion."
public class UldirQuarantine : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.UldirQuarantine;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
