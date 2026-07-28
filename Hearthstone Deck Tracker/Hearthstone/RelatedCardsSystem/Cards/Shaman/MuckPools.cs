namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform a friendly minion into one that costs (1) more."
public class MuckPools : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MuckPools;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}

public class MuckPoolsCorePlaceholder : MuckPools
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MuckPoolsCorePlaceholder;
}
