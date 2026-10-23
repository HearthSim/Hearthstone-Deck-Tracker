using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover an Elemental to summon. Add the others to your hand."
public class WorldPillarFragment : ClassOrNeutralElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.WorldPillarFragmentToken;
}
