using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you played an Elemental last turn, Discover an Elemental."
public class ServantOfKalimos : ClassOrNeutralElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ServantOfKalimos;
}
