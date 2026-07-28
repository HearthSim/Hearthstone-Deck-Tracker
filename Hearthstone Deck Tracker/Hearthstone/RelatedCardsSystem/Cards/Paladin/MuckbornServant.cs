using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Taunt Battlecry: Discover a Paladin card."
public class MuckbornServant : PaladinCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.MuckbornServant;
}

public class MuckbornServantCorePlaceholder : MuckbornServant
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.MuckbornServantCorePlaceholder;
}
