using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Deathrattle: Get two random Temporary 2-Cost minions." (Temporary is a post-pick modifier)
public class TunnelTerror : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.TunnelTerror;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
