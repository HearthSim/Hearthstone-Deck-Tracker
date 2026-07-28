
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Secret: When your turn starts, Discover and cast a Secret from another class."
public class Perjury : OffClassSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Perjury;
}

public class PerjuryCore : Perjury
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.PerjuryCorePlaceholder;
}
