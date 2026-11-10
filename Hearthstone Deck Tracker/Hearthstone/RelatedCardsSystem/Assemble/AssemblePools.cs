using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Assemble;

/// <summary>
/// The pools of the Assemble cards, from HSReplay.net: for each Assemble card, the middle cards it
/// can offer, and for each middle card the options offered with it.
///
/// The pools come from game data rather than card text, so they are fetched rather than written as
/// card classes. The server also owns which cards Assemble, so a new Assemble card needs no release.
/// Pools are free; their keyword breakdown is the premium part and comes with the Outfinder's
/// keyword data.
/// </summary>
public static class AssemblePools
{
	private const string Url = "https://hsreplay.net/api/v1/live/assemble_pools/";

	// The data is of the last 7 days, refreshed by the server at most hourly.
	private static readonly TimeSpan MaxAge = TimeSpan.FromHours(1);

	// After a failed request, wait this long before asking again: pools are requested on hover.
	private static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(5);

	// Rajh and Shadowcrafter Assemble from every class, whichever class plays them. By dbf id, as
	// on the Assemble Pools page.
	private static readonly HashSet<int> AnyClassDbfIds = new() { 132353, 129944 };

	private record Pools(Dictionary<int, Dictionary<int, int[]>> ByAssembleDbfId, DateTime ExpiresAt);

	private static readonly ConcurrentDictionary<string, Pools> _poolsByGameType = new();
	private static readonly ConcurrentDictionary<string, bool> _requests = new();

	/// <summary>
	/// The game type of the data for a game: the data is of ranked and Underground Arena games, so
	/// other modes use the closest one. Null for modes with no comparable data.
	/// </summary>
	public static string? GetDataGameType(GameType gameType, FormatType format) => gameType switch
	{
		GameType.GT_ARENA or GameType.GT_UNDERGROUND_ARENA => "UNDERGROUND_ARENA",
		GameType.GT_RANKED or GameType.GT_CASUAL or GameType.GT_VS_FRIEND or GameType.GT_VS_AI => format switch
		{
			FormatType.FT_STANDARD => "RANKED_STANDARD",
			FormatType.FT_WILD => "RANKED_WILD",
			_ => null,
		},
		_ => null,
	};

	private static string? CurrentDataGameType => GetDataGameType(PoolContext.GetGameType(), PoolContext.GetFormatType());

	/// <summary>
	/// Fetches the pools for the current game, unless they are fresh. Fire and forget: pools show up
	/// once the request lands.
	/// </summary>
	public static void Load() => Load(CurrentDataGameType);

	private static void Load(string? dataGameType)
	{
		if(dataGameType == null)
			return;
		if(_poolsByGameType.TryGetValue(dataGameType, out var pools) && DateTime.UtcNow < pools.ExpiresAt)
			return;
		if(!_requests.TryAdd(dataGameType, true))
			return;
		Fetch(dataGameType).Forget();
	}

	private static async Task Fetch(string dataGameType)
	{
		try
		{
			var byAssembleDbfId = await MakeRequest(dataGameType);
			if(byAssembleDbfId != null)
				_poolsByGameType[dataGameType] = new Pools(byAssembleDbfId, DateTime.UtcNow + MaxAge);
			else
				Backoff(dataGameType);
		}
		catch(Exception e)
		{
			Log.Error($"Error fetching Assemble pools: {e.Message}");
			Backoff(dataGameType);
		}
		finally
		{
			_requests.TryRemove(dataGameType, out _);
		}
	}

	// Keeps pools fetched earlier, which are better than none, but stops asking for a while.
	private static void Backoff(string dataGameType) => _poolsByGameType.AddOrUpdate(
		dataGameType,
		_ => new Pools(new Dictionary<int, Dictionary<int, int[]>>(), DateTime.UtcNow + RetryAfter),
		(_, pools) => pools with { ExpiresAt = DateTime.UtcNow + RetryAfter }
	);

	private static async Task<Dictionary<int, Dictionary<int, int[]>>?> MakeRequest(string dataGameType)
	{
		using HttpRequestMessage req = new(HttpMethod.Get, $"{Url}?game_type={dataGameType}");
		req.Headers.UserAgent.ParseAdd(Helper.GetUserAgent());
		var resp = await Core.HttpClient.SendAsync(req);
		// 204 when the server has no data for the game type yet.
		if(resp is not { StatusCode: HttpStatusCode.OK })
			return null;

		var data = JsonConvert.DeserializeObject<AssemblePoolsResponse>(await resp.Content.ReadAsStringAsync());
		return data?.Pools?.ToDictionary(
			assemble => int.Parse(assemble.Key),
			assemble => assemble.Value.ToDictionary(middle => int.Parse(middle.Key), middle => middle.Value)
		);
	}

	/// <summary>
	/// The pool of an Assemble card for the current game, or null when the card doesn't Assemble or
	/// its pool hasn't loaded. Asking for a pool that isn't there yet fetches it, which covers decklist
	/// hovers in the menu, before any game has loaded one.
	/// </summary>
	public static AssemblePool? GetPool(Card assembleCard, string? playerClass)
	{
		var dataGameType = CurrentDataGameType;
		if(dataGameType == null)
			return null;
		Load(dataGameType);
		if(!_poolsByGameType.TryGetValue(dataGameType, out var pools))
			return null;
		if(!pools.ByAssembleDbfId.TryGetValue(assembleCard.DbfId, out var middles))
			return null;

		var classFilter = GetClassFilter(assembleCard, playerClass);
		var middleCards = new List<AssembleMiddleCard>();
		foreach(var middle in middles)
		{
			var middleCard = Database.GetCardFromDbfId(middle.Key, false);
			// Cards newer than our card data can't be shown.
			if(middleCard == null || !classFilter(middleCard))
				continue;
			var options = middle.Value
				.Select(dbfId => Database.GetCardFromDbfId(dbfId, false))
				.WhereNotNull()
				.Where(classFilter)
				.ToList();
			middleCards.Add(new AssembleMiddleCard(middleCard, options));
		}
		return middleCards.Count > 0 ? new AssemblePool(assembleCard, middleCards) : null;
	}

	/// <summary>
	/// Whether the current game has a pool for the card. The server owns which cards Assemble, so
	/// this only knows once pools have loaded; asking fetches them when they haven't.
	/// </summary>
	public static bool HasPool(int dbfId)
	{
		var dataGameType = CurrentDataGameType;
		if(dataGameType == null)
			return false;
		Load(dataGameType);
		return _poolsByGameType.TryGetValue(dataGameType, out var pools) && pools.ByAssembleDbfId.ContainsKey(dbfId);
	}

	/// <summary>
	/// An Assemble card offers cards from the class playing it and Neutral. The data mixes in other
	/// classes from copies other classes played, so the class playing it picks the right ones. Neutral
	/// Assemble cards, Rajh and Shadowcrafter offer every class.
	/// </summary>
	private static Func<Card, bool> GetClassFilter(Card assembleCard, string? playerClass)
	{
		var cardClasses = assembleCard.GetClasses().ToList();
		if(cardClasses.Contains("Neutral") || AnyClassDbfIds.Contains(assembleCard.DbfId))
			return _ => true;
		// Without a player (decklist hovers outside a game with no deck selected), use the card's own.
		var classes = string.IsNullOrEmpty(playerClass) ? cardClasses : new List<string> { playerClass! };
		return card => card.GetClasses().Any(c => c == "Neutral" || classes.Contains(c));
	}

	private class AssemblePoolsResponse
	{
		[JsonProperty("pools")]
		public Dictionary<string, Dictionary<string, int[]>>? Pools { get; set; }
	}
}

/// <summary>
/// The pool of an Assemble card: its middle cards, most offered first.
/// </summary>
public record AssemblePool(Card AssembleCard, List<AssembleMiddleCard> MiddleCards)
{
	public List<Card> Cards => MiddleCards.Select(m => m.Card).ToList();
}

/// <summary>
/// A middle card of an Assemble pool, with the options offered with it, most offered first.
/// </summary>
public record AssembleMiddleCard(Card Card, List<Card> Options);
