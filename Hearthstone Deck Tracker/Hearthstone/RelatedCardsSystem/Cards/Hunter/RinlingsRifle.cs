using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "After your hero attacks, Discover a Secret and cast it."
public class RinlingsRifle : ClassOrNeutralSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.RinlingsRifle;
}
