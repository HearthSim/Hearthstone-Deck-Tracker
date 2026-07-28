using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Rewind. Summon a random 3-Cost Beast. It attacks a random enemy."
public class Wormhole : Cost3BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Wormhole;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
