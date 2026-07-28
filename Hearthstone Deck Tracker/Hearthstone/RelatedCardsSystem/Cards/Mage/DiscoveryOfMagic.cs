using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a spell from a spell school you haven't cast this game (from any class)."
// The pool shrinks as PlayedSpellSchoolsCounter fills up: a spell stays only while its
// school hasn't been cast. Cross-class ("from any class"). Varies with live counter state,
// so this implements ICardWithDynamicRelatedCardsSummary directly (cf. MountainMap).
public class DiscoveryOfMagic : ICardWithDynamicRelatedCardsSummary
{
	// All schooled spells (cross-class) per (GameType, FormatType); the per-state filtering
	// by unplayed school and deck-dependent pass are cheap and recomputed per call.
	private static readonly Dictionary<(GameType, FormatType), List<Card>> PoolCache = new();

	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Mage.DiscoveryOfMagic;

	// Discover: 3 unique choices, single event.
	protected virtual PickConfig Config => new PickConfig(3, 1, false);

	public bool ShouldShowForOpponent(Player opponent) => false;

	// Pool depends on live counter state, not the hovered entity, so hoveredEntity is ignored.
	public List<Card> GetPool(Player player, Entity? hoveredEntity = null) => GetFilteredPool(player);

	public List<Card?> GetRelatedCards(Player player) => GetRelatedCards(player, null);

	public List<Card?> GetRelatedCards(Player player, Entity? hoveredEntity, List<Card>? pool = null) =>
		(pool ?? GetPool(player, hoveredEntity)).Cast<Card?>().ToList();

	public int ComputeSummary(Player player, out Dictionary<string, string>? summary, out PoolStatistics? statistics, bool usePercentages = true, Entity? hoveredEntity = null, List<Card>? pool = null)
	{
		var poolList = (pool ?? GetPool(player, hoveredEntity)).Cast<Card?>().ToList();
		return RelatedCardsManager.TryGetRelatedCardsSummary(poolList, Config, out summary, out statistics, usePercentages);
	}

	private List<Card> GetFilteredPool(Player player)
	{
		var gt = PoolContext.GetGameType();
		var format = PoolContext.GetFormatType();
		if(!PoolCache.TryGetValue((gt, format), out var pool))
		{
			pool = HearthDb.Cards.Collectible.Values
				.Where(c => c.Type == CardType.SPELL && c.GetTag(GameTag.SPELL_SCHOOL) > 0)
				.Select(c => new Card(c))
				.Where(c => c.IsCardLegal(gt, format))
				.OrderBy(c => c.Cost)
				.ToList();
			PoolCache[(gt, format)] = pool;
		}

		var played = GetPlayedSchools(player);
		var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;
		return pool
			.Where(c => !played.Contains((SpellSchool)c.GetTag(GameTag.SPELL_SCHOOL)))
			.FilterGenerationPool(deck)
			.GroupBy(c => c.Name)
			.Select(g => g.First())
			.ToList();
	}

	private static HashSet<SpellSchool> GetPlayedSchools(Player player)
	{
		var counters = player.IsLocalPlayer
			? Core.Game.CounterManager.PlayerCounters
			: Core.Game.CounterManager.OpponentCounters;
		var counter = counters.OfType<PlayedSpellSchoolsCounter>().FirstOrDefault();
		return counter != null
			? new HashSet<SpellSchool>(counter.GetPlayedSpellSchools())
			: new HashSet<SpellSchool>();
	}
}
