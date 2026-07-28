namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Secret: When a friendly minion dies, summon a random minion with the same Cost."
// One unknown dying friendly minion -> averaged mixture over the board's cost buckets.
public class Effigy : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Effigy;
	protected override int CostOffset => 0;
	protected override bool AffectsAllTargets => false;
}
