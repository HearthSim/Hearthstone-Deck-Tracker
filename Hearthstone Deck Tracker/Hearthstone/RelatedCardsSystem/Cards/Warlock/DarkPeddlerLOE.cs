using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Battlecry: Discover a 1-Cost card."
public class DarkPeddlerLOE : ClassOrNeutralCost1CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkPeddlerLOE;
}

public class DarkPeddlerCore : DarkPeddlerLOE
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkPeddlerCore;
}

public class DarkPeddlerWONDERS : DarkPeddlerLOE
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkPeddlerWONDERS;
}
