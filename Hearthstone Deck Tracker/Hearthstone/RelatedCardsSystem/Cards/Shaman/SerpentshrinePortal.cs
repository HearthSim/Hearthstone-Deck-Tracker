using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Deal $3 damage. Summon a random 3-Cost minion. Overload: (1)"
public class SerpentshrinePortal : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.SerpentshrinePortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
