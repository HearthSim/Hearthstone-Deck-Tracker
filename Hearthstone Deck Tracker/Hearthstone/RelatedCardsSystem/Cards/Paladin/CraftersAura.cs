using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "At the end of your turn, summon a random 6-Cost minion. Lasts 3 turns."
public class CraftersAura : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.CraftersAura;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 3;
}
