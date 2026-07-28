using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Get two random 8-Cost minions. They cost (1) less for each card you played for 2 Mana this game."
public class JadeGuardians : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.JadeGuardians;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
