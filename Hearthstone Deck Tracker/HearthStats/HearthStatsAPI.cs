using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HearthStats
{
	class HearthStatsAPI
	{
		private const string baseUrl = "http://192.237.249.9";
		private static string _userKey;
		private static string _authToken;
        public async static Task<bool> Login(string email, string pwd)
        {
	        try
	        {
		        if(File.Exists(Config.Instance.HearthStatsFilePath))
		        {
			        using(var reader = new StreamReader(Config.Instance.HearthStatsFilePath))
			        {
				        dynamic content = JsonConvert.DeserializeObject(reader.ReadToEnd());
				        _userKey = content.userkey;
				        _authToken = content.auth_token;
			        }
			        Console.WriteLine("loaded key from file");
			        return true;
		        }
		        const string url = baseUrl + "/api/v2/users/sign_in";
		        var data = JsonConvert.SerializeObject(new {user_login = new {email = email, password = pwd}});

		        var json = await Post(url, data);
		        dynamic response = JsonConvert.DeserializeObject(json);
		        if((bool)response.success)
		        {
			        _userKey = response.userkey;
			        _authToken = response.auth_token;
			        using(var writer = new StreamWriter(Config.Instance.HearthStatsFilePath, false))
				        writer.Write(JsonConvert.SerializeObject(new {userkey = _userKey, auth_token = _authToken}));
			        return true;
		        }
		        return false;
	        }
	        catch(Exception e)
	        {
				Console.WriteLine(e);
		        return false;
	        }
		}

		public static async void GetDecks()
		{
			var url = baseUrl + "/api/v2/decks/show?auth_token=" + _authToken;
			var response = await Get(url);
			Console.WriteLine(response);

		}

		public static async void PostDeck(Deck deck)
		{
			var url = baseUrl + "/api/v2/decks/hdt_create?auth_token=" + _authToken;

			var cards = deck.Cards.Select(x => new {id = x.Id, count = x.Count});

			var data =
				JsonConvert.SerializeObject(
				                            new
				                            {
					                            name = deck.Name,
					                            note = deck.Note,
					                            tags = deck.Tags,
					                            @class = deck.Class,
					                            cards = cards
				                            });
			try
			{
				var response = await Post(url, data);
				Console.WriteLine(response);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine(data);
		}

		public static async void PostGameResult(Deck deck)
		{
			var url = baseUrl + "/api/v2/matches/new?auth_token=" + _authToken;
            var game = deck.DeckStats.Games.FirstOrDefault();
			if(game == null)
				return;
			
			var data = JsonConvert.SerializeObject(new
			{
				mode = game.GameMode.ToString(),
				@class = game.PlayerHero ?? "unknown",
				oppclass = game.OpponentHero ?? "unknown",
				oppname = game.OpponentName ?? "unknown",
				result = game.Result,
				coin = game.Coin.ToString().ToLower(),
				numturns = game.Turns,
				duration = (int)(game.EndTime - game.StartTime).TotalSeconds,
				notes = game.Note ?? "",
				deck_id = deck.HearthStatsId
			});

			try
			{
				var result = await Post(url, data);
				Console.WriteLine(result);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static async Task<string> Post(string url, string data)
		{
			return await Post(url, Encoding.UTF8.GetBytes(data));
		}

		private static async Task<string> Post(string url, byte[] data)
		{
			var request = CreateRequest(url, "POST");
			using(var stream = await request.GetRequestStreamAsync())
				stream.Write(data, 0, data.Length);
			var response = await request.GetResponseAsync();
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		private static async Task<string> Get(string url)
		{
			var request = CreateRequest(url, "GET");
			var response = await request.GetResponseAsync();
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		private static HttpWebRequest CreateRequest(string url, string method)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Accept = "application/json";
			request.Method = method;
			return request;
		}

	}

}
