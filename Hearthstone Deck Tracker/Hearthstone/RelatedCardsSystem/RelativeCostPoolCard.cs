using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// Where a relative-cost (evolve/devolve) effect finds the cards it transforms.
/// Board sources use the entity's current COST tag (the game evolves from the current
/// cost, so hand-time discounts carry over); hand and deck sources use the printed cost.
/// </summary>
public enum RelativeCostTargetSource
{
	FriendlyBoard,
	EnemyBoard,
	HandCards,
	HandMinions,
	HandSpells,
	DeckSpells,
	FriendlyWeapon,
}

/// <summary>
/// Base class for evolve/devolve-style cards: effects that transform (or summon off of)
/// existing cards into random ones from the bucket of cards costing
/// <c>target cost + <see cref="CostOffset"/></c>.
/// <para/>
/// The related-cards list is the full pool (every possible outcome regardless of cost),
/// so the user can browse and filter it in the pool panel. The summary, however, is
/// conditioned on live game state: each current target contributes one draw from its own
/// cost bucket, and the statistics are the resulting mixture. With no targets (e.g. empty
/// board) the summary window still renders — headers and right-click hint only.
/// </summary>
public abstract class RelativeCostPoolCard : ICardWithDynamicRelatedCardsSummary
{
	// Full outcome pool per pool kind ("minions", "spells", "weapons") and (GameType, FormatType).
	// Evolve pools are class-agnostic (a 4-drop can evolve into any class's 5-drop), so unlike
	// DiscoverPoolCard's cache there is no player-class dimension and all subclasses sharing a
	// PoolCacheKey share one entry. Deck-dependent filtering (FilterGenerationPool) and the
	// name-dedup are cheap single passes recomputed per call.
	private static readonly Dictionary<string, Dictionary<(GameType, FormatType), List<Card>>> _sharedPoolCache = new();

	public abstract string GetCardId();

	/// <summary>Cost delta applied to each target: +1 for classic Evolve, -1 for Devolve, etc.</summary>
	protected abstract int CostOffset { get; }

	protected virtual RelativeCostTargetSource TargetSource => RelativeCostTargetSource.FriendlyBoard;

	/// <summary>
	/// True when the effect hits every candidate target at once (Evolve, Devolve).
	/// False when only one of the candidates is affected and we can't know which —
	/// targeted effects (Mutate), triggers (Bamboozle, Carefree Cookie). The summary then
	/// averages over the candidates instead of combining them.
	/// </summary>
	protected virtual bool AffectsAllTargets => true;

	/// <summary>Draws per event: 1 for a random outcome, 3 for a discover-style pick (Blazing Transmutation).</summary>
	protected virtual int BatchSize => 1;

	protected virtual bool IsWithReplacement => true;

	/// <summary>Which cards can be an outcome of the effect. Cost is handled by the bucketing, not here.</summary>
	protected virtual bool IsInPool(Card card) => card.TypeEnum == CardType.MINION;

	/// <summary>
	/// Cache discriminator for the shared pool. Subclasses with the same IsInPool result must
	/// return the same key ("minions" default); override alongside IsInPool ("spells", "weapons").
	/// </summary>
	protected virtual string PoolCacheKey => "minions";

	public virtual bool ShouldShowForOpponent(Player opponent) => false;

	/// <summary>
	/// The full filtered pool. Cost-relative pools don't vary by the hovered entity (only the
	/// narrowing/bucketing does), so <paramref name="hoveredEntity"/> is ignored here.
	/// </summary>
	public List<Card> GetPool(Player player, Entity? hoveredEntity = null) =>
		GetFilteredPool(player, PoolContext.GetGameType(), PoolContext.GetFormatType());

	public virtual List<Card?> GetRelatedCards(Player player) => GetRelatedCards(player, null);

	public virtual List<Card?> GetRelatedCards(Player player, Entity? hoveredEntity, List<Card>? pool = null) =>
		(pool ?? GetPool(player, hoveredEntity)).Cast<Card?>().ToList();

	public int ComputeSummary(Player player, out Dictionary<string, string>? summary, out PoolStatistics? statistics, bool usePercentages = true, Entity? hoveredEntity = null, List<Card>? pool = null)
	{
		pool ??= GetPool(player, hoveredEntity);

		var targets = GetTargets(player, hoveredEntity).ToList();
		if(targets.Count > 0)
		{
			var byCost = pool.GroupBy(c => c.Cost).ToDictionary(g => g.Key, g => g.ToList());

			// One draw per target from its clamped cost bucket. Multiple targets resolving to
			// the same bucket merge into a draw count so the math treats them as repeats.
			var drawsPerCost = new Dictionary<int, int>();
			foreach(var (cost, offset) in targets)
			{
				var bucket = ResolveBucket(byCost, cost + offset, offset);
				if(bucket is not int b)
					continue;
				drawsPerCost[b] = drawsPerCost.TryGetValue(b, out var draws) ? draws + 1 : 1;
			}

			if(drawsPerCost.Count > 0)
			{
				var buckets = drawsPerCost
					.Select(kvp => (byCost[kvp.Key], kvp.Value))
					.ToList();
				var count = RelatedCardsManager.TryGetBucketedRelatedCardsSummary(
					buckets, BatchSize, IsWithReplacement, AffectsAllTargets,
					out summary, out statistics, usePercentages);
				if(count > 0)
					return count;
			}
		}

		summary = null;
		statistics = BuildEmptyStatistics(pool);
		return pool.Count;
	}

	/// <summary>
	/// The candidate targets as (current cost, cost offset) pairs. The default pairs every
	/// card from <see cref="TargetSource"/> with <see cref="CostOffset"/>; override for
	/// effects with mixed directions (Primordial Wave) or state-value pools that resolve to
	/// a single computed bucket (counters, remaining mana, hand size).
	/// <paramref name="hoveredEntity"/> is the hovered hand entity, when known.
	/// </summary>
	protected virtual IEnumerable<(int Cost, int Offset)> GetTargets(Player player, Entity? hoveredEntity) =>
		GetTargetCosts(player, TargetSource).Select(cost => (cost, CostOffset));

	/// <summary>
	/// The side-appropriate counter instance for <paramref name="player"/>, or null when
	/// the counter isn't running (e.g. non-traditional matches before initialization).
	/// </summary>
	protected static T? GetCounter<T>(Player player) where T : BaseCounter
	{
		var counters = player.IsLocalPlayer
			? Core.Game.CounterManager.PlayerCounters
			: Core.Game.CounterManager.OpponentCounters;
		return counters.OfType<T>().FirstOrDefault();
	}

	/// <summary>
	/// The local player's currently available mana, including temporary mana (coins).
	/// Only meaningful for the local player — the summary is never computed for the opponent.
	/// </summary>
	protected static int RemainingMana(Player player)
	{
		var playerEntity = Core.Game.PlayerEntity;
		if(!player.IsLocalPlayer || playerEntity == null)
			return 0;
		return playerEntity.GetTag(GameTag.RESOURCES)
			+ playerEntity.GetTag(GameTag.TEMP_RESOURCES)
			- playerEntity.GetTag(GameTag.RESOURCES_USED);
	}

	/// <summary>
	/// The hovered entity's live cost when available (hand hovers), otherwise the printed
	/// cost of <paramref name="cardId"/> (deck hovers).
	/// </summary>
	protected static int HoveredCost(Entity? hoveredEntity, string cardId) =>
		hoveredEntity?.GetTag(GameTag.COST) ?? Database.GetCardFromId(cardId)?.Cost ?? 0;

	protected static IEnumerable<int> GetTargetCosts(Player player, RelativeCostTargetSource source)
	{
		switch(source)
		{
			case RelativeCostTargetSource.FriendlyBoard:
				return player.Board.Where(e => e.IsMinion).Select(e => e.GetTag(GameTag.COST));
			case RelativeCostTargetSource.EnemyBoard:
				return Core.Game.Opponent.Board.Where(e => e.IsMinion).Select(e => e.GetTag(GameTag.COST));
			case RelativeCostTargetSource.HandCards:
				return player.Hand.Where(e => e.HasCardId).Select(e => e.Card.Cost);
			case RelativeCostTargetSource.HandMinions:
				return player.Hand.Where(e => e.IsMinion && e.HasCardId).Select(e => e.Card.Cost);
			case RelativeCostTargetSource.HandSpells:
				return player.Hand.Where(e => e.IsSpell && e.HasCardId).Select(e => e.Card.Cost);
			case RelativeCostTargetSource.DeckSpells:
				// Approximation: the registered deck list rather than the exact remaining deck.
				var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;
				return deck.Where(c => c.TypeEnum == CardType.SPELL)
					.SelectMany(c => Enumerable.Repeat(c.Cost, Math.Max(1, c.Count)));
			case RelativeCostTargetSource.FriendlyWeapon:
				return player.Board.Where(e => e.IsWeapon).Select(e => e.GetTag(GameTag.COST));
			default:
				return Enumerable.Empty<int>();
		}
	}

	/// <summary>
	/// Maps a desired cost to the nearest non-empty bucket, mirroring the game's reroll
	/// behavior: clamp into the pool's cost range, then walk back toward the target's
	/// original cost (there are no 11+-cost minions, so evolving a 10-drop yields another
	/// 10-drop; devolving below the cheapest bucket yields the cheapest).
	/// </summary>
	protected static int? ResolveBucket(Dictionary<int, List<Card>> byCost, int desired, int offset)
	{
		var min = int.MaxValue;
		var max = int.MinValue;
		foreach(var cost in byCost.Keys)
		{
			if(cost < min) min = cost;
			if(cost > max) max = cost;
		}
		if(min > max)
			return null;

		var current = Math.Min(Math.Max(desired, min), max);
		var step = offset >= 0 ? -1 : 1;
		while(!byCost.ContainsKey(current))
		{
			current += step;
			if(current < min || current > max)
				return null;
		}
		return current;
	}

	/// <summary>
	/// Per-player filter applied on top of the shared (class-agnostic) pool cache — a cheap
	/// pass recomputed per call. Override for pools with discover-style class scoping.
	/// </summary>
	protected virtual IEnumerable<Card> FilterPoolForPlayer(IEnumerable<Card> pool, Player player) => pool;

	protected List<Card> GetFilteredPool(Player player, GameType gt, FormatType format)
	{
		var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;

		// No self-exclusion, unlike DiscoverPoolCard: an evolve outcome can share a name
		// with its source (and spells-pool cards can produce themselves).
		return FilterPoolForPlayer(GetBasePool(gt, format), player)
			.FilterGenerationPool(deck)
			// removes duplicated cards (for example core + expansion version)
			.GroupBy(c => c.Name)
			.Select(g => g.First())
			.ToList();
	}

	private List<Card> GetBasePool(GameType gt, FormatType format)
	{
		if(!_sharedPoolCache.TryGetValue(PoolCacheKey, out var poolCache))
		{
			poolCache = new Dictionary<(GameType, FormatType), List<Card>>();
			_sharedPoolCache[PoolCacheKey] = poolCache;
		}

		if(!poolCache.TryGetValue((gt, format), out var pool))
		{
			pool = HearthDb.Cards.Collectible.Values
				.Select(c => new Card(c))
				.Where(c => IsInPool(c) && c.IsCardLegal(gt, format))
				.OrderBy(c => c.Cost)
				.ToList();
			poolCache[(gt, format)] = pool;
		}

		return pool;
	}

	// Renders the summary window frame without content: section headers and em-dash
	// medians only. Attack/Health sections only appear for pools that contain minions.
	private static PoolStatistics BuildEmptyStatistics(List<Card> pool)
	{
		var hasMinions = pool.Any(c => c.TypeEnum != CardType.SPELL);
		var emptyBars = Array.Empty<StatBar>();
		return new PoolStatistics(
			"—",
			hasMinions ? "—" : null,
			hasMinions ? "—" : null,
			emptyBars,
			hasMinions ? emptyBars : null,
			hasMinions ? emptyBars : null);
	}
}
