namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform a minion into a random one that costs (3) more."
public class ExplosiveEvolution : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ExplosiveEvolution;
	protected override int CostOffset => 3;
	protected override bool AffectsAllTargets => false;
}
