using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: Discover a Secret."
public class MysteryWinner : ClassOrNeutralSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MysteryWinner;
}
