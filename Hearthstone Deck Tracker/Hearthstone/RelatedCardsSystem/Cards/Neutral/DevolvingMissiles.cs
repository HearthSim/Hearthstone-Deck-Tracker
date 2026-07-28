namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Shoot three missiles at random enemy minions that transform them into ones that cost (1) less."
// Approximation: each enemy minion is treated as one draw from its cost-1 bucket. Exact
// when there are three enemy minions; slightly off otherwise (missiles can stack on the
// same minion).
public class DevolvingMissiles : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DevolvingMissiles;
	protected override int CostOffset => -1;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.EnemyBoard;
}
