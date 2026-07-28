using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Destroy a minion. Summon a random minion of the same Cost to replace it."
public class LifeCycle : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.LifeCycle;
	protected override int CostOffset => 0;
	protected override bool AffectsAllTargets => false;

	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity) =>
		GetTargetCosts(player, RelativeCostTargetSource.FriendlyBoard)
			.Concat(GetTargetCosts(player, RelativeCostTargetSource.EnemyBoard))
			.Select(cost => (cost, 0));
}
