using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// A <see cref="RelativeCostPoolCard"/> whose outcome is a single cost bucket computed from
/// live game state — a counter, corpses, remaining mana, hand size, an entity tag.
/// <para/>
/// Unlike evolve-style pools, the related-cards list (and thus the right-click pool panel)
/// is pre-filtered to the same bucket the summary uses; the full pool only shows when the
/// bucket can't be resolved. Because <see cref="GetRelatedCards"/> never receives the
/// hovered entity, it looks up the player's in-hand copy by card id so the panel reflects
/// the same live entity state (upgrade tags, discounted costs) as the summary.
/// </summary>
public abstract class StateValuePoolCard : RelativeCostPoolCard
{
	protected override int CostOffset => 0;

	/// <summary>
	/// The bucket cost for the current state, or null when it can't be known (no counter,
	/// no corpses, opponent side) — yielding the empty-state summary and the full pool.
	/// </summary>
	protected abstract int? TargetCost(Player player, Entity? hoveredEntity);

	protected override IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity)
	{
		if(TargetCost(player, hoveredEntity) is int cost)
			yield return (cost, 0);
	}

	public override List<Card?> GetRelatedCards(Player player) =>
		GetRelatedCards(player, player.Hand.FirstOrDefault(e => e.CardId == GetCardId()));

	public override List<Card?> GetRelatedCards(Player player, Entity? hoveredEntity, List<Card>? pool = null)
	{
		pool ??= GetPool(player, hoveredEntity);

		if(TargetCost(player, hoveredEntity) is not int cost)
			return pool.Cast<Card?>().ToList();

		var byCost = pool.GroupBy(c => c.Cost).ToDictionary(g => g.Key, g => g.ToList());
		return ResolveBucket(byCost, cost, 0) is int bucket
			? byCost[bucket].Cast<Card?>().ToList()
			: pool.Cast<Card?>().ToList();
	}
}
