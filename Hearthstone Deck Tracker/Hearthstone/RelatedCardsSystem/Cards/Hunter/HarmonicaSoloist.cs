using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: If you control no other minions, Discover and cast a Secret."
public class HarmonicaSoloist : ClassOrNeutralSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.HarmonicaSoloist;
}
