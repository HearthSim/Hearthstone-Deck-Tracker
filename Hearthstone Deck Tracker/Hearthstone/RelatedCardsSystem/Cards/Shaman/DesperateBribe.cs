using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Summon two 2-cost minions for each player. Transform your minions into ones that cost (1) more."
public class DesperateBribe : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.DesperateBribe;
	protected override int CostOffset => 1;

	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity) =>
		GetTargetCosts(player, RelativeCostTargetSource.FriendlyBoard)
			.Concat(Enumerable.Repeat(2, 2)) // the two summoned 2-Cost minions
			.Select(cost => (cost, CostOffset));
}
