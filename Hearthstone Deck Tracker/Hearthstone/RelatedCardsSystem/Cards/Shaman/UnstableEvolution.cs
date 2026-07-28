namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Echo Transform a friendly minion into one that costs (1) more."
public class UnstableEvolution : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.UnstableEvolution;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
