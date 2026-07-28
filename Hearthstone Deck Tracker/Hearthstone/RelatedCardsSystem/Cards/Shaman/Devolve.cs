namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform all enemy minions into random ones that cost (1) less."
public class Devolve : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Devolve;
	protected override int CostOffset => -1;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.EnemyBoard;
}
