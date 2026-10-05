using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Lifesteal. Deal 8 damage randomly split among all enemies. Summon an 8-Cost minion."
public class HallsOfOrigination : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.HallsOfOrigination;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
