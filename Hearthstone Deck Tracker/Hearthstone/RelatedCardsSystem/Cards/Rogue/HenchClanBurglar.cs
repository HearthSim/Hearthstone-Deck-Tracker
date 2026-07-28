using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Discover a spell from another class."
public class HenchClanBurglar : OffClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.HenchClanBurglar;
}

public class HenchClanBurglarCorePlaceholder : HenchClanBurglar
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.HenchClanBurglarCorePlaceholder;
}
