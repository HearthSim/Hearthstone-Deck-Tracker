using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover a Secret."
public class SecretPlan : ClassOrNeutralSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.SecretPlan;
}
