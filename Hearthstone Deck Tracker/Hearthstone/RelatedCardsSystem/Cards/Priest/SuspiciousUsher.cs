using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry: Discover a Legendary minion. If your opponent guesses your choice, they get a copy."
public class SuspiciousUsher : ClassOrNeutralLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SuspiciousUsher;
}

public class SuspiciousUsherCorePlaceholder : SuspiciousUsher
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SuspiciousUsherCorePlaceholder;
}
