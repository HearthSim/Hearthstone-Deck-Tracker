using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Choose One - Destroy a minion with 3 or less Attack; or Summon a random 2-Cost minion."
public class TwilightInfluence : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.TwilightInfluence;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
