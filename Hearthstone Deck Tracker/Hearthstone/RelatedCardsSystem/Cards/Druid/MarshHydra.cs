using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Rush. After this attacks, add a random 8-Cost minion to your hand."
public class MarshHydra : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.MarshHydra;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
