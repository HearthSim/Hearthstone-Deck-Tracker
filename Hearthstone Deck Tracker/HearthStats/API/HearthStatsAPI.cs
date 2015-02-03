#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

		private static readonly List<GameMode> ValidGameModes = new List<GameMode> {GameMode.Casual, GameMode.Ranked, GameMode.Friendly};

		public static bool IsLoggedIn
		{
			get { return !string.IsNullOrEmpty(_authToken); }
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
			catch(Exception ex)
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
					using(var reader = new StreamReader(Config.Instance.HearthStatsFilePath))
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

		private static HttpWebRequest CreateRequest(string url, string method)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Accept = "application/json";
			request.Method = method;
			return request;
		}

		#region Async

		public static async Task<LoginResult> LoginAsync(string email, string pwd)
		{
			try
			{
				const string url = BaseUrl + "/api/v2/users/sign_in";
				var data = JsonConvert.SerializeObject(new {user_login = new {email, password = pwd}});
				var json = await PostAsync(url, data);
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

		public static async Task<List<Deck>> GetDecksAsync(long unixTime)
		{
			Logger.WriteLine("getting decks since " + unixTime, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_after?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
			Console.WriteLine(data);
			try
			{
				var response = await PostAsync(url, data);

				var obj = JsonConvert.DeserializeObject<ResponseWrapper<DeckObjectWrapper[]>>(response);
				if(obj.status == "success")
					return obj.data.Select(dw => dw.ToDeck()).ToList();
				return new List<Deck>();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return new List<Deck>();
			}
		}

		public static async Task<PostResult> PostDeckAsync(Deck deck)
		{
			if(deck == null)
			{
				Logger.WriteLine("deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsId)
			{
				Logger.WriteLine("deck already posted", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("uploading deck: " + deck, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_create?auth_token=" + _authToken;
			var cards = deck.Cards.Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new
			{
				name = deck.Name,
				note = deck.Note,
				tags = deck.Tags,
				@class = deck.Class,
				cards,
				// url,
				// version = deck.Version.ToString("{M}.{m}")
			});
			try
			{
				var response = await PostAsync(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					deck.VersionOnHearthStats = true;
					deck.HearthStatsId = json.data.id;
					deck.HearthStatsDeckVersionId = json.data.version_id;
					Logger.WriteLine("assigned id to deck: " + deck.HearthStatsId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		public static async Task<PostResult> PostVersionAsync(Deck deck, string hearthStatsId)
		{
			if(deck == null)
			{
				Logger.WriteLine("version(deck) is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.VersionOnHearthStats)
			{
				Logger.WriteLine("version(deck) already posted", "HearthStatsAPI");
				return PostResult.Failed;
			}
			var version = deck.Version.ToString("{M}.{m}");
			Logger.WriteLine("uploading version " + version + " of " + deck, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/create_version?auth_token=" + _authToken;
			var cards = deck.Cards.Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {deck_id = hearthStatsId, version, cards});
			try
			{
				var response = await PostAsync(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					deck.VersionOnHearthStats = true;
					deck.HearthStatsDeckVersionId = json.data.version_id;
					//deck.HearthStatsId = hearthStatsId;
					Logger.WriteLine("assigned id to version: " + deck.HearthStatsId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		public static async Task<PostResult> PostGameResultAsync(GameStats game, Deck deck)
		{
			if(!IsValidGame(game))
				return PostResult.Failed;
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("can not upload game, deck has no hearthstats id", "HearthStatsAPI");
				return PostResult.Failed;
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
				var response = await PostAsync(url, data);
				var json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					game.HearthStatsId = json.data.id;
					game.HearthStatsDeckVersionId = json.data.version_id;
					Logger.WriteLine("assigned id to version: " + deck.HearthStatsId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		private static async Task<string> PostAsync(string url, string data)
		{
			return await PostAsync(url, Encoding.UTF8.GetBytes(data));
		}

		private static async Task<string> PostAsync(string url, byte[] data)
		{
			var request = CreateRequest(url, "POST");
			using(var stream = await request.GetRequestStreamAsync())
				stream.Write(data, 0, data.Length);
			var response = await request.GetResponseAsync();
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		private static async Task<string> GetAsync(string url)
		{
			var request = CreateRequest(url, "GET");
			var response = await request.GetResponseAsync();
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		public static async Task<List<GameStats>> GetGamesAsync(long unixTime)
		{
			Logger.WriteLine("getting games since " + unixTime, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/matches/hdt_after?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
			Console.WriteLine(data);
			try
			{
				var response = await PostAsync(url, data);
				var obj = JsonConvert.DeserializeObject<ResponseWrapper<GameStatsObjectWrapper[]>>(response);
				if(obj.status == "success")
					return obj.data.Select(x => x.ToGameStats()).ToList();
				return new List<GameStats>();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return new List<GameStats>();
			}
		}

		public static async Task<PostResult> DeleteDeckAsync(Deck deck)
		{
			if(deck == null)
			{
				Logger.WriteLine("error: deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(!deck.HasHearthStatsId)
			{
				Logger.WriteLine("error: deck has no HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("deleting deck: " + deck, "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/decks/delete?auth_token=" + _authToken; // TODO 
			var data = JsonConvert.SerializeObject(new {deck_id = new[] {deck.HearthStatsId}});
			try
			{
				var response = await PostAsync(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("deleted deck", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		public static async Task<PostResult> DeleteMatchAsync(GameStats game)
		{
			if(game == null)
			{
				Logger.WriteLine("error: game is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(!game.HasHearthStatsId)
			{
				Logger.WriteLine("error: game has no HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("deleting game: " + game, "HearthStatsAPI");

			var url = BaseUrl + "@@@@@@@@@@@@@@@@" + _authToken; // TODO
			var data = JsonConvert.SerializeObject(new {deck_id = game.HearthStatsId});
			try
			{
				var response = await PostAsync(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("deleted game", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		//TODO TEST
		public static async Task<PostResult> MoveMatchAsync(GameStats game, Deck newDeck)
		{
			if(game == null)
			{
				Logger.WriteLine("error: game is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(!game.HasHearthStatsId)
			{
				Logger.WriteLine("error: game has no HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(newDeck == null)
			{
				Logger.WriteLine("error: deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(!newDeck.HasHearthStatsId)
			{
				Logger.WriteLine("error: deck has no HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("deleting game: " + game, "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/matches/move?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {match_id = new[] {game.HearthStatsId}, deck_id = newDeck.HearthStatsId});
			try
			{
				var response = await PostAsync(url, data);
				Console.WriteLine(response);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("moved game", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.CanRetry;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.CanRetry;
			}
		}

		#endregion

		#region Sync

		public static LoginResult Login(string email, string pwd)
		{
			return LoginAsync(email, pwd).Result;
		}

		public static List<Deck> GetDecks(long unixTime)
		{
			return GetDecksAsync(unixTime).Result;
		}

		public static PostResult PostDeck(Deck deck)
		{
			return PostDeckAsync(deck).Result;
		}

		public static PostResult PostGameResult(GameStats game, Deck deck)
		{
			return PostGameResultAsync(game, deck).Result;
		}

		private static string Post(string url, string data)
		{
			return PostAsync(url, data).Result;
		}

		private static string Post(string url, byte[] data)
		{
			return PostAsync(url, data).Result;
		}

		private static string Get(string url)
		{
			return GetAsync(url).Result;
		}

		public static List<GameStats> GetGames(long unixTime)
		{
			return GetGamesAsync(unixTime).Result;
		}

		public static PostResult DeleteDeck(Deck deck)
		{
			return DeleteDeckAsync(deck).Result;
		}

		public static PostResult PostVersion(Deck version, string hearthStatsId)
		{
			return PostVersionAsync(version, hearthStatsId).Result;
		}

		public static PostResult MoveMatch(GameStats game, Deck newDeck)
		{
			return MoveMatchAsync(game, newDeck).Result;
		}

		#endregion

		#region

		private static readonly Dictionary<int, GameResult> _gameResultDict = new Dictionary<int, GameResult>
		{
			{1, GameResult.Win},
			{2, GameResult.Loss},
			{3, GameResult.Draw}
		};

		private static readonly Dictionary<int, GameMode> _gameModeDict = new Dictionary<int, GameMode>
		{
			{1, GameMode.Arena},
			{2, GameMode.Casual},
			{3, GameMode.Ranked},
			{4, GameMode.None}, //Tournament
			{5, GameMode.Friendly}
		};

		private static readonly Dictionary<int, string> _heroDict = new Dictionary<int, string>
		{
			{1, "Druid"},
			{2, "Hunter"},
			{3, "Mage"},
			{4, "Paladin"},
			{5, "Priest"},
			{6, "Rogue"},
			{7, "Shaman"},
			{8, "Warlock"},
			{9, "Warrior"}
		};

		public class CardObject
		{
			public string count;
			public string id;

			public CardObject(Card card)
			{
				if(card != null)
				{
					id = card.Id;
					count = card.Count.ToString();
				}
			}

			public Card ToCard()
			{
				try
				{
					var card = Game.GetCardFromId(id);
					if(card != null && !string.IsNullOrEmpty(count))
						card.Count = int.Parse(count);
					return card;
				}
				catch(Exception e)
				{
					Console.WriteLine("error converting CardObject: " + e, "HearthStatsAPI");
					return null;
				}
			}
		}

		public class DeckObject
		{
			public int id;
			public int klass_id;
			public string name;
			public string notes;
			public string[] tags;
			public string url;
			public int version_id;

			public Deck ToDeck(CardObject[] cards)
			{
				try
				{
					return new Deck(name ?? "", _heroDict[klass_id],
					                cards == null ? new List<Card>() : cards.Select(x => x.ToCard()).Where(x => x != null), tags ?? new string[0],
					                notes ?? "", url ?? "", DateTime.Now, new List<Card>(), new SerializableVersion(1, 0), new List<Deck>(), true,
					                id.ToString(), Guid.NewGuid(), true) {HearthStatsDeckVersionId = version_id.ToString()};
				}
				catch(Exception e)
				{
					Logger.WriteLine("error converting DeckObject: " + e, "HearthStatsAPI");
					return null;
				}
			}
		}

		public class DeckObjectWrapper
		{
			public CardObject[] cards;
			public DeckObject deck;

			public Deck ToDeck()
			{
				return deck.ToDeck(cards);
			}
		}

		public class GameStatsObject
		{
			public int klass_id { get; set; }
			public bool coin { get; set; }
			public string created_at { get; set; } //unix
			public int duration { get; set; }
			public int id { get; set; }
			public int mode_id { get; set; }
			public string notes { get; set; }
			public int numturns { get; set; }
			public int oppclass_id { get; set; }
			public string oppname { get; set; }
			public int ranklvl { get; set; }
			public int result_id { get; set; }

			public GameStats ToGameStats(string versionId, string deckId)
			{
				try
				{
					return new GameStats
					{
						Result = _gameResultDict[result_id],
						GameMode = _gameModeDict[mode_id],
						PlayerHero = _heroDict[klass_id],
						OpponentHero = _heroDict[oppclass_id],
						OpponentName = oppname,
						Turns = numturns,
						Coin = coin,
						HearthStatsId = id.ToString(),
						HearthStatsDeckId = deckId,
						HearthStatsDeckVersionId = versionId,
						Note = notes,
						Rank = ranklvl,
						StartTime = DateTime.Parse(created_at),
						EndTime = DateTime.Parse(created_at).AddSeconds(duration),
					};
				}
				catch(Exception e)
				{
					Logger.WriteLine("error converting GameStatsObject " + e, "HearthStatsAPI");
					return null;
				}
			}
		}

		public class GameStatsObjectWrapper
		{
			public GameStatsObject match { get; set; }
			public string deck_id { get; set; }
			public string version_id { get; set; }

			public GameStats ToGameStats()
			{
				return match.ToGameStats(version_id, deck_id);
			}
		}

		public class ResponseWrapper<T>
		{
			public string status { get; set; }
			public T data { get; set; }
		}

		#endregion
	}
}