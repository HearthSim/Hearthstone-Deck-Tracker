using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: If you cast a spell last turn, Discover a spell."
public class Marshspawn : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Marshspawn;
}

public class MarshspawnCorePlaceholder : Marshspawn
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MarshspawnCorePlaceholder;
}
