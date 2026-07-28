using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Magnetic. Deathrattle: Summon a random 8-Cost minion."
public class ContainmentUnit : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ContainmentUnit;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
