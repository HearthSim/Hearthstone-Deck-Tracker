using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a minion with a type you haven't played. If you play it this turn, also pick
// one of the others." The pool shrinks as MinionTypesPlayedThisGameCounter fills up: a
// type stays in the pool while a minion of it could still raise the counter (the
// maximizing rule keeps ambiguous dual-types available), and ALL-type minions always
// qualify. The pool varies with live counter state, so this implements
// ICardWithDynamicRelatedCardsSummary directly instead of extending DiscoverPoolCard.
public class MountainMap : ICardWithDynamicRelatedCardsSummary
{
	// Typed-minion pools per (class, GameType, FormatType); the per-state filtering by
	// unplayed types and deck-dependent passes are cheap and recomputed per call.
	private static readonly Dictionary<(string?, GameType, FormatType), List<Card>> PoolCache = new();

	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MountainMap;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// Pool depends on live counter state and class, not the hovered entity, so it is ignored.
	public List<Card> GetPool(Player player, Entity? hoveredEntity = null) => GetFilteredPool(player);

	public List<Card?> GetRelatedCards(Player player) =>
		GetRelatedCards(player, null);

	public List<Card?> GetRelatedCards(Player player, Entity? hoveredEntity, List<Card>? pool = null) =>
		(pool ?? GetPool(player, hoveredEntity)).Cast<Card?>().ToList();

	public int ComputeSummary(Player player, out Dictionary<string, string>? summary, out PoolStatistics? statistics, bool usePercentages = true, Entity? hoveredEntity = null, List<Card>? pool = null)
	{
		var poolList = (pool ?? GetPool(player, hoveredEntity)).Cast<Card?>().ToList();
		return RelatedCardsManager.TryGetRelatedCardsSummary(poolList, new PickConfig(3, 1, false),
			out summary, out statistics, usePercentages);
	}

	private List<Card> GetFilteredPool(Player player)
	{
		var gt = PoolContext.GetGameType();
		var format = PoolContext.GetFormatType();
		var playerClass = player.CurrentClass;
		if(!PoolCache.TryGetValue((playerClass, gt, format), out var pool))
		{
			pool = HearthDb.Cards.Collectible.Values
				.Where(c => c.Type == CardType.MINION
					&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
					&& (c.Race != Race.INVALID || c.SecondaryRace != Race.INVALID))
				.Select(c => new Card(c))
				.Where(c => c.IsCardLegal(gt, format))
				.OrderBy(c => c.Cost)
				.ToList();
			PoolCache[(playerClass, gt, format)] = pool;
		}

		var unplayed = GetUnplayedTypes(player);
		var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;
		return pool
			.Where(c => HasUnplayedType(c, unplayed))
			.FilterGenerationPool(deck)
			.GroupBy(c => c.Name)
			.Select(g => g.First())
			.ToList();
	}

	private static HashSet<Race> GetUnplayedTypes(Player player)
	{
		var counters = player.IsLocalPlayer
			? Core.Game.CounterManager.PlayerCounters
			: Core.Game.CounterManager.OpponentCounters;
		var counter = counters.OfType<MinionTypesPlayedThisGameCounter>().FirstOrDefault();
		return counter?.GetUnplayedTypes()
			?? new HashSet<Race>(MinionTypesPlayedThisGameCounter.MinionTypes);
	}

	private static bool HasUnplayedType(Card card, HashSet<Race> unplayed) =>
		card.IsAllRace()
		|| (card.RaceEnum is Race race && unplayed.Contains(race))
		|| (card.SecondaryRaceEnum is Race secondary && unplayed.Contains(secondary));
}
