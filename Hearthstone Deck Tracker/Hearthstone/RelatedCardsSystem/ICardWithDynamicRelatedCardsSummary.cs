using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// A related-cards pool whose summary depends on live game state — e.g. evolve/devolve
/// effects, where the outcome pool is relative to the cost of each affected card.
/// <para/>
/// <see cref="ICardWithRelatedCards.GetRelatedCards"/> still returns the full,
/// state-independent pool for browsing (the user narrows it in the pool panel);
/// the summary is recomputed on every hover from the current targets.
/// </summary>
public interface ICardWithDynamicRelatedCardsSummary : ICardWithRelatedCards
{
	/// <summary>
	/// Computes the pool summary from the current game state.
	/// <para/>
	/// Returns the number of cards the statistics were computed over: the union of the
	/// active cost buckets when targets are known, otherwise the full pool size. When no
	/// targets are known, <paramref name="statistics"/> is a non-null empty-state instance
	/// so the summary window still renders its frame (headers + right-click hint), just
	/// without bars, medians or keywords.
	/// <para/>
	/// <paramref name="hoveredEntity"/> is the hovered hand entity when known (null on deck
	/// hovers) — cards whose pool depends on their own live cost or zone read it.
	/// </summary>
	int ComputeSummary(Player player, out Dictionary<string, string>? summary, out PoolStatistics? statistics, bool usePercentages = true, Entity? hoveredEntity = null, List<Card>? pool = null);

	/// <summary>
	/// The full filtered pool for the card — every possible outcome, before any live-state
	/// narrowing or bucketing. A hover builds the display list and the summary from the same
	/// pool, so callers compute it once here and pass it to both
	/// <see cref="GetRelatedCards"/> and <see cref="ComputeSummary"/>, avoiding a second
	/// filter-and-dedup pass per hover. <paramref name="hoveredEntity"/> only matters for pools
	/// whose contents (not just their narrowing) depend on live entity state, e.g. a class that
	/// swaps each turn; cost-relative pools ignore it.
	/// </summary>
	List<Card> GetPool(Player player, Entity? hoveredEntity = null);

	/// <summary>
	/// Entity-aware variant of <see cref="ICardWithRelatedCards.GetRelatedCards"/>: hand
	/// hovers pass the hovered entity so per-copy state (upgrade tags, discounted costs)
	/// selects the right pool when multiple copies are in hand. Callers without an entity
	/// (deck-list tooltips) use the plain overload, which falls back to the first in-hand
	/// copy of the card. Pass <paramref name="pool"/> (from <see cref="GetPool"/>) to reuse an
	/// already-built pool; when null it is computed on demand.
	/// </summary>
	List<Card?> GetRelatedCards(Player player, Entity? hoveredEntity, List<Card>? pool = null);
}
