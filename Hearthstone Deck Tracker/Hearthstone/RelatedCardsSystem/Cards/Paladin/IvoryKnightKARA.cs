using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Battlecry: Discover a spell. Restore Health to your hero equal to its Cost."
public class IvoryKnightKARA : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.IvoryKnightKARA;
}

public class IvoryKnightCore : IvoryKnightKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.IvoryKnightCore;
}

public class IvoryKnightWONDERS : IvoryKnightKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.IvoryKnightWONDERS;
}
