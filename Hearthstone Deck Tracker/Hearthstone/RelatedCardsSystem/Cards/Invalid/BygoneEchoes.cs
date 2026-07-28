using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Invalid;

// "Summon a random 4-Cost minion. Spend 4 Corpses to summon another. Outcast: And another."
public class BygoneEchoes : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Invalid.BygoneEchoes;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
