namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Transform minions in your hand into ones that cost (3) more. (They keep their original Cost.)"
public class TheStarsAlign : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.TheStarsAlign;
	protected override int CostOffset => 3;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandMinions;
}
