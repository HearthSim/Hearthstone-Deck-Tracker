using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a {0} spell. (Swaps class each turn!)"
// The class it currently discovers from is on the entity's TAG_SCRIPT_DATA_NUM_1 (a
// CardClass value). With no tag — deck-list hovers, tag not yet set — it falls back to
// the player's own class. The pool varies by class rather than cost, so this implements
// ICardWithDynamicRelatedCardsSummary directly instead of extending RelativeCostPoolCard.
public class ShadowedInformant : ICardWithDynamicRelatedCardsSummary
{
	// Class-scoped spell pools per (class, GameType, FormatType); deck-dependent filtering
	// and name-dedup are cheap single passes recomputed per call.
	private static readonly Dictionary<(string?, GameType, FormatType), List<Card>> PoolCache = new();

	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ShadowedInformant;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// The pool itself varies by the discovered class, which is read off the hovered entity's
	// tag (or the in-hand copy's when none is passed), so hoveredEntity is honoured here.
	public List<Card> GetPool(Player player, Entity? hoveredEntity = null) =>
		GetFilteredPool(player, DiscoverClass(player, hoveredEntity));

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

	// The hand grid comes from GetRelatedCards, which never receives the hovered entity,
	// so with no entity the in-hand copy's tag is looked up by card id.
	private string? DiscoverClass(Player player, Entity? hoveredEntity)
	{
		var tag = hoveredEntity?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1)
			?? player.Hand.FirstOrDefault(e => e.CardId == GetCardId())?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1)
			?? 0;
		return tag > 0 ? HearthDbConverter.ConvertClass((CardClass)tag) : player.CurrentClass;
	}

	private List<Card> GetFilteredPool(Player player, string? className)
	{
		var gt = PoolContext.GetGameType();
		var format = PoolContext.GetFormatType();
		if(!PoolCache.TryGetValue((className, gt, format), out var pool))
		{
			pool = HearthDb.Cards.Collectible.Values
				.Where(c => c.Type == CardType.SPELL && c.IsClass(className))
				.Select(c => new Card(c))
				.Where(c => c.IsCardLegal(gt, format))
				.OrderBy(c => c.Cost)
				.ToList();
			PoolCache[(className, gt, format)] = pool;
		}

		var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;
		return pool
			.FilterGenerationPool(deck)
			.GroupBy(c => c.Name)
			.Select(g => g.First())
			.ToList();
	}
}
