
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Spellburst: Summon a random 3-Cost minion. (Holy spells don't remove this Spellburst.)"
public class KureTheLightBeyond : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.KureTheLightBeyond;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
