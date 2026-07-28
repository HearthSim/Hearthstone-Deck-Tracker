namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Choose a minion. Discover one that costs (1) more to transform it into."
// Evolve via Discover: 3 unique picks from the target's cost+1 bucket.
public class BlazingTransmutation : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BlazingTransmutation;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
	protected override int BatchSize => 3;
	protected override bool IsWithReplacement => false;
}
