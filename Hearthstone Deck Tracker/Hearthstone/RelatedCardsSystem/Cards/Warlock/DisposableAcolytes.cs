using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "When you play or discard this, summon two random 1-Cost minions."
public class DisposableAcolytes : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DisposableAcolytes;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
