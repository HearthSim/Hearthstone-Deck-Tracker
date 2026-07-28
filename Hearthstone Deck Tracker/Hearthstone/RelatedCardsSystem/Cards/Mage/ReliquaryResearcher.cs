using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: If you've Excavated twice, cast two random Mage Secrets."
// The two Secrets end up in play together, so they must be distinct: one batch of 2
// unique draws (without replacement), like DiscoAtTheEndOfTime. Pool and Secret
// generator inherited from TricksyImproviser.
public class ReliquaryResearcher : MageSecretPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ReliquaryResearcher;
	public override int Picks() => 2;
	public override bool IsWithReplacement() => false;
}
