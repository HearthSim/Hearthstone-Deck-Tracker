using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.Twitch
{
	public class TwitchApi
	{
		private const string ClientId = "yhwtsycu5d6zwi2kkfkwe95fjc69x5";

		private static readonly Dictionary<string, CacheObj> Cache = new Dictionary<string, CacheObj>();

		public static async Task<string> GetVodUrl(int userId, DateTime gameStart)
		{
			var data = await GetDynamic(Urls.Videos(userId));
			var latestVod = data.videos[0];
			var diff = TimeZoneInfo.ConvertTimeToUtc(gameStart) - TimeZoneInfo.ConvertTimeToUtc((DateTime)latestVod.created_at);
			return $"{latestVod.url}?t={diff.Hours}h{diff.Minutes}m{diff.Seconds}s";
		}

		public static async Task<int> GetUserId(string userName)
		{
			var data = await GetDynamic(Urls.User(userName));
			return data.users[0]._id;
		}

		public static async Task<bool> IsStreaming(int userId)
		{
			var data = await GetDynamic(Urls.Stream(userId));
			return data?.stream != null;
		}

		private static async Task<dynamic> GetDynamic(string url)
		{
			try
			{
				var json = await GetData(url);
				return JsonConvert.DeserializeObject<dynamic>(json);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		private static async Task<string> GetData(string url)
		{
			if(Cache.TryGetValue(url, out var cache) && cache.Valid)
				return cache.Data;
			var request = CreateRequest(url);
			using(var response = (HttpWebResponse)await request.GetResponseAsync())
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var data = reader.ReadToEnd();
				Cache[url] = new CacheObj(data);
				return data;
			}
		}

		private static HttpWebRequest CreateRequest(string url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Accept = "application/vnd.twitchtv.v5+json";
			request.Method = "GET";
			request.Headers.Add("Client-ID", ClientId);
			return request;
		}

		private class CacheObj
		{
			private readonly DateTime _created;

			public CacheObj(string data)
			{
				_created = DateTime.Now;
				Data = data;
			}

			public bool Valid => (DateTime.Now - _created).TotalSeconds < 5;
			public string Data { get; }
		}

		private static class Urls
		{
			private const string Base = "https://api.twitch.tv/kraken";
			public static string User(string userName) => $"{Base}/users?login={userName}";
			public static string Videos(int userId, int limit = 1) => $"{Base}/channels/{userId}/videos?limit={limit}";
			public static string Stream(int userId) => $"{Base}/streams/{userId}";
		}
	}
}
