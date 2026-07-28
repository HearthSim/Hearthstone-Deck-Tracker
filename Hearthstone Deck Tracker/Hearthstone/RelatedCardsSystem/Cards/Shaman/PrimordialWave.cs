using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform enemy minions into ones that cost (1) less and friendly minions into ones that cost (1) more."
public class PrimordialWave : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.PrimordialWave;
	protected override int CostOffset => 1;

	// Both directions at once: friendly minions evolve (+1), enemy minions devolve (-1).
	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity) =>
		GetTargetCosts(player, RelativeCostTargetSource.FriendlyBoard).Select(cost => (cost, 1))
			.Concat(GetTargetCosts(player, RelativeCostTargetSource.EnemyBoard).Select(cost => (cost, -1)));
}

public class PrimordialWaveCorePlaceholder : PrimordialWave
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.PrimordialWaveCorePlaceholder;
}
