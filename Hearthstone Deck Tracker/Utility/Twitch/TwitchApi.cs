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

		public static async Task<string> GetVodUrl(int userId)
		{
			try
			{
				var data = await GetData(Urls.Videos(userId));
				var dynData = JsonConvert.DeserializeObject<dynamic>(data.Data);
				if(dynData == null)
					return null;
				foreach(var video in dynData.videos)
				{
					if(video.status != "recording")
						continue;
					return TwitchApiHelper.GenerateTwitchVodUrl((string)video.url, (DateTime)video.created_at, data.Date);
				}
				return null;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task<bool> IsStreaming(int userId)
		{
			try
			{
				var data = await GetData(Urls.Stream(userId));
				var dynData = JsonConvert.DeserializeObject<dynamic>(data.Data);
				return dynData?.stream != null;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		private static async Task<ResponseData> GetData(string url)
		{
			if(Cache.TryGetValue(url, out var cache) && cache.Valid)
				return cache.Data;
			var request = CreateRequest(url);
			using(var response = (HttpWebResponse)await request.GetResponseAsync())
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var date = DateTime.Parse(response.Headers.Get("date"));
				var data = new ResponseData(reader.ReadToEnd(), date);
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

			public CacheObj(ResponseData data)
			{
				_created = DateTime.Now;
				Data = data;
			}

			public bool Valid => (DateTime.Now - _created).TotalSeconds < 5;
			public ResponseData Data { get; }
		}

		private class ResponseData
		{
			public ResponseData(string data, DateTime date)
			{
				Data = data;
				Date = date;
			}

			public string Data { get; }
			public DateTime Date { get; }
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
