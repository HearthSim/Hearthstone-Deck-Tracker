namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform a friendly minion into a random one that costs (1) more."
public class Mutate : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Mutate;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
