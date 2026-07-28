using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Give a friendly minion +4/+4 and 'Deathrattle: Summon a random 4-Cost minion.'"
public class ThreshridersBlessing : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ThreshridersBlessing;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
