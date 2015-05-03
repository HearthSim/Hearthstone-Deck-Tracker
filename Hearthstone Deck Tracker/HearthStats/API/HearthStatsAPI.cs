#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API.Objects;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class HearthStatsAPI
	{
		private const string BaseUrl = "http://hearthstats.net";

		#region authentication

		private static string _authToken;

		public static bool IsLoggedIn
		{
			get { return !string.IsNullOrEmpty(_authToken); }
		}

		public static string LoggedInAs { get; private set; }

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
				Logger.WriteLine("Error deleting hearthstats credentials file\n" + ex, "HearthStatsAPI");
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
						LoggedInAs = content.email;
					}
					return true;
				}
				catch(Exception e)
				{
					Logger.WriteLine("Error loading credentials\n" + e, "HearthStatsAPI");
					return false;
				}
			}
			return false;
		}

		public static async Task<LoginResult> LoginAsync(string email, string pwd)
		{
			try
			{
				const string url = BaseUrl + "/api/v2/users/sign_in";
				var data = JsonConvert.SerializeObject(new {user_login = new {email, password = pwd}});
				var json = await PostAsync(url, Encoding.UTF8.GetBytes(data));
				dynamic response = JsonConvert.DeserializeObject(json);
				if((bool)response.success)
				{
					_authToken = response.auth_token;
					LoggedInAs = response.email;
					if(Config.Instance.RememberHearthStatsLogin)
					{
						using(var writer = new StreamWriter(Config.Instance.HearthStatsFilePath, false))
							writer.Write(JsonConvert.SerializeObject(new {auth_token = _authToken, email}));
					}
					return new LoginResult(true);
				}
				return new LoginResult(false, response.ToString());
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return new LoginResult(false, e.Message);
			}
		}

		#endregion

		#region webrequests

		private static async Task<string> PostAsync(string url, string data)
		{
#if DEBUG
			Logger.WriteLine("> " + data, "HearthStatsAPI");
#endif
			return await PostAsync(url, Encoding.UTF8.GetBytes(data));
		}

		private static async Task<string> PostAsync(string url, byte[] data)
		{
			var request = CreateRequest(url, "POST");
			using(var stream = await request.GetRequestStreamAsync())
				stream.Write(data, 0, data.Length);
			var webResponse = await request.GetResponseAsync();
			using(var responseStream = webResponse.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var response = reader.ReadToEnd();
#if DEBUG
				Logger.WriteLine("< " + response, "HearthStatsAPI");
#endif
				return response;
			}
		}

		private static async Task<string> GetAsync(string url)
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

		#endregion

		#region decks/matches

		public static async Task<List<Deck>> GetDecksAsync(long unixTime)
		{
			Logger.WriteLine("getting decks since " + unixTime, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_after?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
			try
			{
				var response = await PostAsync(url, data);

				var obj = JsonConvert.DeserializeObject<ResponseWrapper<DeckObjectWrapper[]>>(response);
				if(obj.status == "success")
					return obj.data.Where(dw => dw != null && dw.deck != null && dw.cards != null).Select(dw => dw.ToDeck()).ToList();
				return new List<Deck>();
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return new List<Deck>();
			}
		}

		public static async Task<List<GameStats>> GetGamesAsync(long unixTime)
		{
			Logger.WriteLine("getting games since " + unixTime, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/matches/hdt_after?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
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
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return new List<GameStats>();
			}
		}

		public static async Task<PostResult> PostDeckAsync(Deck deck, Deck masterDeck = null)
		{
			if(deck == null)
			{
				Logger.WriteLine("deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.IsArenaDeck)
			{
				Logger.WriteLine("deck is an arena deck", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsId)
			{
				Logger.WriteLine("deck already posted", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("uploading deck: " + deck, "HearthStatsAPI");

			var name = masterDeck == null ? deck.Name : masterDeck.Name;
			var tags = masterDeck == null ? deck.Tags : masterDeck.Tags;
			var notes = masterDeck == null ? AddSpecialTagsToNote(deck) : AddSpecialTagsToNote(masterDeck);

			var url = BaseUrl + "/api/v2/decks/hdt_create?auth_token=" + _authToken;
			var cards = deck.Cards.Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {name, notes, tags, @class = deck.Class, cards});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					deck.HearthStatsId = json.data.deck.id;
					deck.DeckStats.HearthStatsDeckId = json.data.deck.id;
					deck.HearthStatsDeckVersionId = json.data.deck_versions[0].id;
					//deck.DeckStats.HearthStatsDeckVersionId = json.data.deck_versions[0].id;
					deck.SyncWithHearthStats = true;
					Logger.WriteLine("HearthStatsId assigned to deck: " + deck.HearthStatsId, "HearthStatsAPI");
					Logger.WriteLine("HearthStatsDeckVersionId assigned to deck: " + deck.HearthStatsDeckVersionId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostVersionAsync(Deck deck, string hearthStatsId)
		{
			if(deck == null)
			{
				Logger.WriteLine("version(deck) is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsDeckVersionId)
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
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					deck.HearthStatsDeckVersionId = json.data.id;
					deck.HearthStatsIdForUploading = hearthStatsId;
					Logger.WriteLine("assigned id to version: " + deck.HearthStatsDeckVersionId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
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
			long versionId;
			if(!long.TryParse(deck.HearthStatsDeckVersionId, out versionId))
			{
				Logger.WriteLine("error: invalid HearthStatsDeckVersionId", "HearthStatsAPI");
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
			gameObj.deck_id = deck.HearthStatsIdForUploading;
			gameObj.deck_version_id = versionId;
			if(!string.IsNullOrEmpty(game.OpponentHero))
				gameObj.oppclass = game.OpponentHero;
			if(!string.IsNullOrEmpty(game.OpponentName))
				gameObj.oppname = game.OpponentName;
			if(!string.IsNullOrEmpty(game.Note))
				gameObj.notes = game.Note;
			if(game.GameMode == GameMode.Ranked && game.HasRank)
				gameObj.ranklvl = game.Rank.ToString();
			gameObj.created_at = game.StartTime.ToUniversalTime().ToString("s");

			var data = JsonConvert.SerializeObject(gameObj);

			try
			{
				var response = await PostAsync(url, data);
				var json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					game.HearthStatsId = json.data.id;
					Logger.WriteLine("assigned id to match: " + game.HearthStatsId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				if(json.status.ToString() == "fail" && json.message.ToString().Contains("Deck could not be found"))
				{
					//deck does not exist on hearthstats
					deck.ResetHearthstatsIds();
					DeckList.Save();
					deck.DeckStats.Games.ForEach(g => g.ResetHearthstatsIds());
					DeckStatsList.Save();
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> DeleteDeckAsync(Deck deck)
		{
			return await DeleteDeckAsync(new[] {deck});
		}

		public static async Task<PostResult> DeleteDeckAsync(IEnumerable<Deck> decks)
		{
			var filtered = decks.Where(d => d != null).ToList();
			var noId = filtered.Where(d => !d.HasHearthStatsId).ToList();
			foreach(var deck in noId)
			{
				Logger.WriteLine("error: deck " + deck.Name + " has no HearthStatsId", "HearthStatsAPI");
				filtered.Remove(deck);
			}
			var invalidId = filtered.Where(d => !Regex.IsMatch(d.HearthStatsId, @"^\d+$")).ToList();
			foreach(var deck in invalidId)
			{
				Logger.WriteLine("error: deck " + deck.Name + " has no valid HearthStatsId", "HearthStatsAPI");
				filtered.Remove(deck);
			}
			if(!filtered.Any())
				return PostResult.Failed;

			Logger.WriteLine("deleting decks: " + filtered.Select(d => d.Name).Aggregate((c, n) => c + ", " + n), "HearthStatsAPI");

			var ids = filtered.Select(d => long.Parse(d.HearthStatsId)).ToArray();

			var url = BaseUrl + "/api/v2/decks/delete?auth_token=" + _authToken; // TODO 
			var data = JsonConvert.SerializeObject(new {deck_id = ids});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("deleted decks", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> DeleteMatchesAsync(List<GameStats> games)
		{
			var validGames = games.Where(g => g != null).ToList();
			if(!validGames.Any())
			{
				Logger.WriteLine("error: all games null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			var noHearthStatsId = games.Where(g => !g.HasHearthStatsId).ToList();
			if(noHearthStatsId.Any())
			{
				foreach(var game in noHearthStatsId)
				{
					Logger.WriteLine("error: game has no HearthStatsId " + game, "HearthStatsAPI");
					validGames.Remove(game);
				}
				if(!validGames.Any())
					return PostResult.Failed;
			}
			var invalidHearthStatsId = new List<GameStats>();
			foreach(var game in validGames)
			{
				long validId;
				if(!long.TryParse(game.HearthStatsId, out validId))
					invalidHearthStatsId.Add(game);
			}
			foreach(var game in invalidHearthStatsId)
			{
				Logger.WriteLine("error: game has no valid HearthStatsId " + game, "HearthStatsAPI");
				validGames.Remove(game);
			}
			if(!validGames.Any())
				return PostResult.Failed;

			Logger.WriteLine("deleting games: " + validGames.Select(g => g.ToString()).Aggregate((c, n) => c + ", " + n), "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/matches/delete?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {match_id = validGames.Select(g => long.Parse(g.HearthStatsId))});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("deleted game", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

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
			long deckId;
			if(!long.TryParse(newDeck.HearthStatsId, out deckId))
			{
				Logger.WriteLine("error: deck has invalid HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}

			long gameId;
			if(!long.TryParse(game.HearthStatsId, out gameId))
			{
				Logger.WriteLine("error: game has invalid HearthStatsId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("moving game: " + game, "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/matches/move?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {match_id = new[] {gameId}, deck_id = deckId});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					Logger.WriteLine("moved game", "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				Logger.WriteLine("error: " + response, "HearthStatsAPI");
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> UpdateDeckAsync(Deck editedDeck)
		{
			if(editedDeck == null)
			{
				Logger.WriteLine("deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(!editedDeck.HasHearthStatsId)
			{
				Logger.WriteLine("deck does not exist yet, uploading", "HearthStatsAPI");
				return await PostDeckAsync(editedDeck);
			}
			Logger.WriteLine("editing deck: " + editedDeck, "HearthStatsAPI");
			var url = BaseUrl + "/api/v2/decks/hdt_edit?auth_token=" + _authToken;
			var cards = editedDeck.Cards.Select(x => new CardObject(x));
			var data =
				JsonConvert.SerializeObject(
				                            new
				                            {
					                            deck_id = editedDeck.HearthStatsId,
					                            name = editedDeck.Name,
					                            notes = AddSpecialTagsToNote(editedDeck),
					                            tags = editedDeck.Tags,
					                            cards
				                            });
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
					return PostResult.WasSuccess;
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> CreatArenaRunAsync(Deck deck)
		{
			if(deck == null)
			{
				Logger.WriteLine("deck is null", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.IsArenaDeck == false)
			{
				Logger.WriteLine("deck is not a viable arena deck", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsArenaId)
			{
				Logger.WriteLine("arena deck already posted", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasVersions)
			{
				Logger.WriteLine("arena deck cannot have versions", "HearthStatsAPI");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsId)
			{
				if(deck.CheckIfArenaDeck() == false)
				{
					Logger.WriteLine("deck has non-arena games", "HearthStatsAPI");
					return PostResult.Failed;
				}
				if(deck.DeckStats.Games.Count <= 1)
				{
					Logger.WriteLine("deck has hearthstats id but no arena id. deleting and uploading as arena deck.", "HearthStatsAPI");
					await DeleteDeckAsync(deck);
					deck.HearthStatsId = "";
					deck.HearthStatsDeckVersionId = "";
				}
				else
				{
					Logger.WriteLine("deck already has games but no arena id. cannot upload deck.", "HearthStatsAPI");
					return PostResult.Failed;
				}
			}
			Logger.WriteLine("creating new arena run: " + deck, "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/arena_runs/new?auth_token=" + _authToken;
			var cards = deck.Cards.Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {@class = deck.Class, cards});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
				{
					deck.HearthStatsArenaId = json.data.id;
					Logger.WriteLine("assigned arena id to deck: " + deck.HearthStatsId, "HearthStatsAPI");
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostArenaMatch(GameStats game, Deck deck)
		{
			if(!IsValidArenaGame(game))
				return PostResult.Failed;
			if(!deck.HasHearthStatsArenaId)
			{
				Logger.WriteLine("can not upload game, deck has no HearthStatsArenaId", "HearthStatsAPI");
				return PostResult.Failed;
			}
			Logger.WriteLine("uploading arena match: " + game, "HearthStatsAPI");

			var url = BaseUrl + "/api/v2/matches/hdt_new?auth_token=" + _authToken;

			dynamic gameObj = new ExpandoObject();
			gameObj.mode = game.GameMode.ToString();
			gameObj.@class = string.IsNullOrEmpty(game.PlayerHero) ? deck.Class : game.PlayerHero;
			gameObj.result = game.Result.ToString();
			gameObj.coin = game.Coin.ToString().ToLower();
			gameObj.numturns = game.Turns;
			gameObj.duration = (int)(game.EndTime - game.StartTime).TotalSeconds;
			gameObj.arena_run_id = deck.HearthStatsArenaId;
			if(!string.IsNullOrEmpty(game.OpponentHero))
				gameObj.oppclass = game.OpponentHero;
			if(!string.IsNullOrEmpty(game.OpponentName))
				gameObj.oppname = game.OpponentName;

			var data = JsonConvert.SerializeObject(gameObj);

			try
			{
				var response = await PostAsync(url, data);
				var json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "success")
					return PostResult.WasSuccess;
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "HearthStatsAPI");
				return PostResult.Failed;
			}
		}

		#endregion

		#region misc

		private static readonly List<GameMode> ValidGameModes = new List<GameMode> {GameMode.Casual, GameMode.Ranked, GameMode.Friendly};

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

		public static bool IsValidArenaGame(GameStats game)
		{
			var baseMsg = "Game " + game + " is no valid arena game ({0})";
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
			if(game.GameMode != GameMode.Arena)
			{
				Logger.WriteLine(string.Format(baseMsg, "invalid game mode: " + game.GameMode), "HearthStatsAPI");
				return false;
			}
			if(game.HasHearthStatsId)
			{
				Logger.WriteLine(string.Format(baseMsg, "already submitted"), "HearthStatsAPI");
				return false;
			}
			return true;
		}

		private static string AddSpecialTagsToNote(Deck deck)
		{
			var note = deck.Note;
			if(!string.IsNullOrEmpty(deck.Url))
				note += "\r\n[HDT-source=" + deck.Url + "]";

			if(deck.Archived)
				note += "\r\n[HDT-archived]";

			return note;
		}

		#endregion
	}
}