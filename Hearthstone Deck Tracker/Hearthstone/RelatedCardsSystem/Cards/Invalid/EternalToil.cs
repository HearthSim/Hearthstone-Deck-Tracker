using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Invalid;

// "Deal $1 damage to a minion. If it survives, draw a card. If it dies, summon a random 1-Cost minion."
public class EternalToil : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Invalid.EternalToil;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
