using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform all minions into random ones with the same Cost."
// Every minion on either board transforms into its own same-cost bucket.
public class Revolve : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Revolve;
	protected override int CostOffset => 0;

	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity) =>
		GetTargetCosts(player, RelativeCostTargetSource.FriendlyBoard)
			.Concat(GetTargetCosts(player, RelativeCostTargetSource.EnemyBoard))
			.Select(cost => (cost, 0));
}
