#region

using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class HearthStatsAPI
	{
		private const string BaseUrl = "http://192.237.249.9";
		private static string _authToken;

		public static bool IsLoggedIn
		{
			get { return !string.IsNullOrEmpty(_authToken); }
			
		}

		public static async Task<LoginResult> Login(string email, string pwd)
		{
			try
			{
				const string url = BaseUrl + "/api/v2/users/sign_in";
				var data = JsonConvert.SerializeObject(new {user_login = new {email, password = pwd}});
				var json = await Post(url, data);
				dynamic response = JsonConvert.DeserializeObject(json);
				if((bool)response.success)
				{
					_authToken = response.auth_token;
					if(Config.Instance.RememberHearthStatsLogin)
					{
						using(var writer = new StreamWriter(Config.Instance.HearthStatsFilePath, false))
							writer.Write(JsonConvert.SerializeObject(new {auth_token = _authToken}));
					}
					return new LoginResult(true);
				}
				return new LoginResult(false, response.ToString());
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return new LoginResult(false, e.Message);
			}
		}
		
		public static bool Logout()
		{
			_authToken = "";
			try
			{
				if(File.Exists(Config.Instance.HearthStatsFilePath))
					File.Delete(Config.Instance.HearthStatsFilePath);
				return true;
			}
			catch (Exception ex)
			{
				Logger.WriteLine("Error deleting hearthstats credentials file\n" + ex);
				return false;
			}
		}

		public static bool LoadCredentials()
		{
			if(File.Exists(Config.Instance.HearthStatsFilePath))
			{
				try
				{
					using (var reader = new StreamReader(Config.Instance.HearthStatsFilePath))
					{
						dynamic content = JsonConvert.DeserializeObject(reader.ReadToEnd());
						_authToken = content.auth_token;
					}
					return true;
				}
				catch(Exception e)
				{
					Logger.WriteLine("Error loading credentials\n" + e);
					return false;
				}
			}
			return false;
		}

		public static async Task<string> GetDecks(long unixTime)
		{
			Logger.WriteLine("getting decks since " + unixTime, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_after?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime});
			Console.WriteLine(data);
			try
			{
				var response = await Post(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				//success?
				return (string)json.data;

			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}

		public static async Task<bool> PostDeck(Deck deck)
		{
			Logger.WriteLine("uploading deck: " + deck, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_create?auth_token=" + _authToken;
			var cards = deck.Cards.Select(x => new {id = x.Id, count = x.Count});
			var data = JsonConvert.SerializeObject(new {name = deck.Name, note = deck.Note, tags = deck.Tags, @class = deck.Class, cards});
			try
			{
				var response = await Post(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				deck.HearthStatsId = json.data.id;
				Logger.WriteLine("assigned id to deck: " + deck.HearthStatsId, "HearthStatsAPI");
				return true;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return false;
			}
		}

		private static readonly List<GameMode> ValidGameModes = new List<GameMode> {GameMode.Arena, GameMode.Casual, GameMode.Ranked, GameMode.Friendly};

		public static bool IsValidGame(GameStats game)
		{
			var baseMsg = "Game " + game + " is not valid ({0})";
			if(game == null)
			{
				Logger.WriteLine(string.Format(baseMsg, "null"), "HearthStatsAPI");
				return false;
			}
			if(game.IsClone)
			{
				Logger.WriteLine(string.Format(baseMsg, "IsClone"), "HearthStatsAPI");
				return false;
			}
			if(ValidGameModes.All(mode => game.GameMode != mode))
			{
				Logger.WriteLine(string.Format(baseMsg, "invalid game mode: " + game.GameMode), "HearthStatsAPI");
				return false;
			}
			if(game.Result == GameResult.None)
			{
				Logger.WriteLine(string.Format(baseMsg, "invalid result: none"), "HearthStatsAPI");
				return false;
			}
			if(game.HasHearthStatsId)
			{
				Logger.WriteLine(string.Format(baseMsg, "already submitted"), "HearthStatsAPI");
				return false;
			}
			return true;
		}
        public static async Task<bool> PostGameResult(GameStats game, Deck deck)
        {
	        if(!IsValidGame(game))
		        return false;
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("can not upload game, deck has no hearthstats id", "HearthStatsAPI");
				return false;
			}
			Logger.WriteLine("uploading match: " + game, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/matches/hdt_new?auth_token=" + _authToken;

	        dynamic gameObj = new ExpandoObject();
			gameObj.mode = game.GameMode.ToString();
			gameObj.@class = string.IsNullOrEmpty(game.PlayerHero) ? deck.Class : game.PlayerHero;
			gameObj.result = game.Result.ToString();
			gameObj.coin = game.Coin.ToString().ToLower();
			gameObj.numturns = game.Turns;
			gameObj.duration = (int)(game.EndTime - game.StartTime).TotalSeconds;
	        gameObj.deck_id = deck.HearthStatsId;
			if(!string.IsNullOrEmpty(game.OpponentHero))
				gameObj.oppclass = game.OpponentHero;
			if(!string.IsNullOrEmpty(game.OpponentName))
				gameObj.oppname = game.OpponentName;
			if(!string.IsNullOrEmpty(game.Note))
				gameObj.notes = game.Note;
	        if(game.GameMode == GameMode.Ranked && game.HasRank)
		        gameObj.ranklvl = game.Rank.ToString();

			var data = JsonConvert.SerializeObject(gameObj);

			try
			{
				var response = await Post(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				game.HearthStatsId = json.data.id;
				return true;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return false;
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

		public static async Task<string> GetGames(long unixTime)
		{
			return "";
		}

		public static async Task<bool> DeleteDeck(Deck deck)
		{
			if(deck == null)
			{
				Logger.WriteLine("error: deck is null", "HearthStatsAPI");
				return false;
			}
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("error: deck has no HearthStatsId", "HearthStatsAPI");
				return false;
			}
			Logger.WriteLine("deleting deck: " + deck, "HearthStatsAPI");

			var url = BaseUrl + "@@@@@@@@@@@@@@@@" + _authToken;
			var data = JsonConvert.SerializeObject(new {deck_id = deck.HearthStatsId});
			try
			{
				var response = await Post(url, data);
				Console.WriteLine(response);
				var json = JsonConvert.DeserializeObject(response);
				var success = true; //bool.Parse(json.success);
				return success;
			}
			catch (Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return false;
			}
		}
	}
}