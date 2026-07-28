using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Discover a weapon."
public class InisToolkit : ClassOrNeutralWeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.InisToolkit;
}
