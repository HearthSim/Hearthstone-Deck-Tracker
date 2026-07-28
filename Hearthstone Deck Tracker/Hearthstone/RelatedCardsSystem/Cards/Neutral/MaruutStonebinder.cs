using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If your deck started with no duplicates, Discover an Elemental to summon. Add the others to your hand."
public class MaruutStonebinder : ClassOrNeutralElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MaruutStonebinder;
}
