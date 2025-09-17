using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public static class CardLegalityChecker
{
	private static readonly ConcurrentDictionary<(GameType, FormatType), HashSet<string>> LegalCardsByFormat = new();

	public static async Task LoadCardsByFormat(GameType gameType, FormatType format)
	{
		try
		{
			var legalCards = await MakeRequest(gameType, format);
			if(legalCards.Length == 0)
				return;

			LegalCardsByFormat[(gameType, format)] = new HashSet<string>(legalCards);
		}
		catch (Exception e)
		{
			Log.Error($"Error fetching legal cards: {e.Message}");
		}
	}

	private const string Url = "https://hsreplay.net/api/v1/live/legal_cards/";
	private static async Task<string[]> MakeRequest(GameType gameType, FormatType format)
	{
		using HttpRequestMessage req = new(HttpMethod.Get, $"{Url}?game_type={(int)gameType}&format_type={(int)format}");
		req.Headers.UserAgent.ParseAdd(Helper.GetUserAgent());
		var resp = await Core.HttpClient.SendAsync(req);
		if(resp is { StatusCode: HttpStatusCode.OK })
			return JsonConvert.DeserializeObject<string[]>(await resp.Content.ReadAsStringAsync());

		return Array.Empty<string>();
	}

	private static bool IsCardLegal(string cardId, GameType gameType, FormatType format)
	{
		if(LegalCardsByFormat.TryGetValue((gameType, format), out var legalCards))
		{
			return legalCards.Contains(cardId);
		}

		return IsCardFromFormatFallback(new Card(cardId), format);
	}

	public static bool IsCardLegal(this Card card, GameType gameType, FormatType format)
	{
		return IsCardLegal(card.Id, gameType, format);
	}

	public static bool IsCardLegal(this ICardWithRelatedCards card, GameType gameType, FormatType format)
	{
		return IsCardLegal(card.GetCardId(), gameType, format);
	}

	private static bool IsCardFromFormatFallback(Card card, FormatType? format)
	{
		return format switch
		{
			FormatType.FT_CLASSIC => Helper.ClassicOnlySets.Contains(card.Set),
			FormatType.FT_WILD => !Helper.ClassicOnlySets.Contains(card.Set),
			FormatType.FT_STANDARD => !Helper.WildOnlySets.Contains(card.Set) && !Helper.ClassicOnlySets.Contains(card.Set),
			FormatType.FT_TWIST => Helper.TwistSets.Contains(card.Set),
			_ => true
		};
	}

}
