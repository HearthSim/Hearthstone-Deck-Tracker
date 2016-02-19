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
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API.Objects;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API
{
	public class HearthStatsAPI
	{
#if DEBUG
		static HearthStatsAPI()
		{
			var fileInfo = new DirectoryInfo(Config.Instance.DataDir).GetFiles("baseurl*").FirstOrDefault();
			if(fileInfo != null)
			{
				using(var sr = new StreamReader(fileInfo.FullName))
					BaseUrl = sr.ReadToEnd();
			}
		}
#endif

		private static string BaseUrl { get; } = "http://api.hearthstats.net/api/v3";

		#region authentication

		private static string _authToken;

		public static bool IsLoggedIn => !string.IsNullOrEmpty(_authToken);

		public static string LoggedInAs { get; private set; }

		public static bool Logout()
		{
			Log.Info("Logged out.");
			_authToken = "";
			try
			{
				if(File.Exists(Config.Instance.HearthStatsFilePath))
					File.Delete(Config.Instance.HearthStatsFilePath);
				return true;
			}
			catch(Exception ex)
			{
				Log.Error("Error deleting hearthstats credentials file\n" + ex);
				return false;
			}
		}

		public static bool LoadCredentials()
		{
			if(File.Exists(Config.Instance.HearthStatsFilePath))
			{
				try
				{
					Log.Info("Loading stored credentials...");
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
					Log.Error("Error loading credentials\n" + e);
					return false;
				}
			}
			return false;
		}

		public static async Task<LoginResult> LoginAsync(string email, string password)
		{
			try
			{
				Log.Info("Logging in...");
				var url = BaseUrl + "/users/sign_in";
				var data = JsonConvert.SerializeObject(new {user_login = new {email, password}});
				var json = await PostAsync(url, Encoding.UTF8.GetBytes(data));
				dynamic response = JsonConvert.DeserializeObject(json);
				if((bool)response.success)
				{
					Log.Info("Successfully logged in.");
					_authToken = response.auth_token;
					LoggedInAs = response.email;
					if(Config.Instance.RememberHearthStatsLogin)
					{
						using(var writer = new StreamWriter(Config.Instance.HearthStatsFilePath, false))
							writer.Write(JsonConvert.SerializeObject(new {auth_token = _authToken, email}));
					}
					return new LoginResult(true);
				}
				Log.Error("Error logging in...");
				return new LoginResult(false, response.ToString());
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new LoginResult(false, e.Message);
			}
		}


		public static async Task<LoginResult> RegisterAsync(string email, string password)
		{
			try
			{
				var url = BaseUrl + "/users";
				var data = JsonConvert.SerializeObject(new {user = new {email, password}});
				var json = await PostAsync(url, Encoding.UTF8.GetBytes(data));
				dynamic response = JsonConvert.DeserializeObject(json);
				if((string)response.email == email && (int)response.id > 0)
					return new LoginResult(true);
				return new LoginResult(false, response.ToString());
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new LoginResult(false, e.Message);
			}
		}

		#endregion

		#region webrequests

		private static async Task<string> PostAsync(string url, string data)
		{
#if DEBUG
			Log.Debug("> " + data);
#endif
			return await PostAsync(url, Encoding.UTF8.GetBytes(data));
		}

		private static async Task<string> PostAsync(string url, byte[] data)
		{
			try
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
					Log.Debug("< " + response);
#endif
					return response;
				}
			}
			catch(WebException e)
			{
				if(Core.MainWindow != null)
					ErrorManager.AddError(new Error("HearthStats", e.Message));
				throw;
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
			Log.Info("getting decks since " + unixTime);
			var url = BaseUrl + "/decks/after_date?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
			try
			{
				var response = await PostAsync(url, data);

				var obj = JsonConvert.DeserializeObject<ResponseWrapper<DeckObjectWrapper[]>>(response);
				if(obj.status == "200")
				{
					return
						obj.data.Where(dw => dw?.deck != null && dw.cards != null).Select(dw => dw.ToDeck()).Where(d => d != null).ToList();
				}
				return new List<Deck>();
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new List<Deck>();
			}
		}

		public static async Task<List<GameStats>> GetGamesAsync(long unixTime)
		{
			Log.Info("getting games since " + unixTime);
			var url = BaseUrl + "/matches/after_date?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {date = unixTime.ToString()});
			try
			{
				var response = await PostAsync(url, data);
				var obj = JsonConvert.DeserializeObject<ResponseWrapper<GameStatsObjectWrapper[]>>(response);
				if(obj.status == "200")
					return obj.data.Select(x => x.ToGameStats()).ToList();
				return new List<GameStats>();
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new List<GameStats>();
			}
		}

		public static async Task<PostResult> PostDeckAsync(Deck deck, Deck masterDeck = null)
		{
			if(deck == null)
			{
				Log.Warn("deck is null");
				return PostResult.Failed;
			}
			if(deck.IsArenaDeck)
			{
				Log.Warn("deck is an arena deck");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsId)
			{
				Log.Warn("deck already posted");
				return PostResult.Failed;
			}
			Log.Info("uploading deck: " + deck);

			var name = masterDeck == null ? deck.Name : masterDeck.Name;
			var tags = masterDeck == null ? deck.Tags : masterDeck.Tags;
			var notes = masterDeck == null ? AddSpecialTagsToNote(deck) : AddSpecialTagsToNote(masterDeck);

			var url = BaseUrl + "/decks?auth_token=" + _authToken;
			var cards = deck.Cards.Where(Database.IsActualCard).Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {name, notes, tags, @class = deck.Class, cards});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					deck.HearthStatsId = json.data.deck.id;
					deck.DeckStats.HearthStatsDeckId = json.data.deck.id;
					deck.HearthStatsDeckVersionId = json.data.deck_versions[0].id;
					//deck.DeckStats.HearthStatsDeckVersionId = json.data.deck_versions[0].id;
					deck.SyncWithHearthStats = true;
					Log.Info("HearthStatsId assigned to deck: " + deck.HearthStatsId);
					Log.Info("HearthStatsDeckVersionId assigned to deck: " + deck.HearthStatsDeckVersionId);
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostVersionAsync(Deck deck, string hearthStatsId)
		{
			if(deck == null)
			{
				Log.Warn("version(deck) is null");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsDeckVersionId)
			{
				Log.Warn("version(deck) already posted");
				return PostResult.Failed;
			}
			var version = deck.Version.ToString("{M}.{m}");
			Log.Info("uploading version " + version + " of " + deck);
			var url = BaseUrl + "/decks/create_version?auth_token=" + _authToken;
			var cards = deck.Cards.Where(Database.IsActualCard).Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {deck_id = hearthStatsId, version, cards});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					deck.HearthStatsDeckVersionId = json.data.id;
					deck.HearthStatsIdForUploading = hearthStatsId;
					Log.Info("assigned id to version: " + deck.HearthStatsDeckVersionId);
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostGameResultAsync(GameStats game, Deck deck)
		{
			if(!IsValidGame(game))
				return PostResult.Failed;
			if(!deck.HasHearthStatsId)
			{
				Log.Warn("can not upload game, deck has no hearthstats id");
				return PostResult.Failed;
			}
			long versionId;
			if(!long.TryParse(deck.HearthStatsDeckVersionId, out versionId))
			{
				Log.Error("invalid HearthStatsDeckVersionId");
				return PostResult.Failed;
			}
			Log.Info("uploading match: " + game);
			var url = BaseUrl + "/matches?auth_token=" + _authToken;

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
			var opponentCards = game.GetOpponentDeck().Cards;
			if(opponentCards.Where(Database.IsActualCard).Any())
				gameObj.oppcards = opponentCards.Where(Database.IsActualCard).Select(c => new {id = c.Id, count = c.Count}).ToArray();
			gameObj.created_at = game.StartTime.ToUniversalTime().ToString("s");

			var data = JsonConvert.SerializeObject(gameObj);

			try
			{
				var response = await PostAsync(url, data);
				var json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					game.HearthStatsId = json.data.id;
					Log.Info("assigned id to match: " + game.HearthStatsId);
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
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostMultipleGameResultsAsync(IEnumerable<GameStats> games, Deck deck)
		{
			var validGames = games.Where(x => IsValidGame(x) && !x.HasHearthStatsId).ToList();
			long versionId;
			if(!long.TryParse(deck.HearthStatsDeckVersionId, out versionId))
			{
				Log.Error("invalid HearthStatsDeckVersionId");
				return PostResult.Failed;
			}

			var url = BaseUrl + "/matches/multi_create?auth_token=" + _authToken;
			dynamic gameObjs = new ExpandoObject[validGames.Count];
			for(int i = 0; i < validGames.Count; i++)
			{
				gameObjs[i] = new ExpandoObject();
				gameObjs[i].mode = validGames[i].GameMode.ToString();
				gameObjs[i].@class = string.IsNullOrEmpty(validGames[i].PlayerHero) ? deck.Class : validGames[i].PlayerHero;
				gameObjs[i].result = validGames[i].Result.ToString();
				gameObjs[i].coin = validGames[i].Coin.ToString().ToLower();
				gameObjs[i].numturns = validGames[i].Turns;
				gameObjs[i].duration = (int)(validGames[i].EndTime - validGames[i].StartTime).TotalSeconds;
				gameObjs[i].deck_id = deck.HearthStatsIdForUploading;
				gameObjs[i].deck_version_id = versionId;
				if(!string.IsNullOrEmpty(validGames[i].OpponentHero))
					gameObjs[i].oppclass = validGames[i].OpponentHero;
				if(!string.IsNullOrEmpty(validGames[i].OpponentName))
					gameObjs[i].oppname = validGames[i].OpponentName;
				if(!string.IsNullOrEmpty(validGames[i].Note))
					gameObjs[i].notes = validGames[i].Note;
				if(validGames[i].GameMode == GameMode.Ranked && validGames[i].HasRank)
					gameObjs[i].ranklvl = validGames[i].Rank.ToString();
				var opponentCards = validGames[i].GetOpponentDeck().Cards;
				if(opponentCards.Where(Database.IsActualCard).Any())
					gameObjs[i].oppcards = opponentCards.Where(Database.IsActualCard).Select(c => new {id = c.Id, count = c.Count}).ToArray();
				gameObjs[i].created_at = validGames[i].StartTime.ToUniversalTime().ToString("s");
			}


			var data = JsonConvert.SerializeObject(new {deck_id = deck.HearthStatsIdForUploading, matches = gameObjs});

			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "404")
				{
					//deck does not exist on hearthstats
					deck.ResetHearthstatsIds();
					DeckList.Save();
					deck.DeckStats.Games.ForEach(g => g.ResetHearthstatsIds());
					DeckStatsList.Save();
					return PostResult.Failed;
				}
				if(json.status.ToString() != "200")
					Log.Error($"{json.message.ToString()} (Status code: {json.status.ToString()})");

				for(int i = 0; i < validGames.Count; i++)
				{
					if(json.data[i].status == "200")
					{
						validGames[i].HearthStatsId = json.data[i].data.id;
						Log.Info("assigned id to match: " + validGames[i].HearthStatsId);
					}
					else
						Log.Error($"Error uploading match {validGames[i]}: {json.data[i].status}");
				}
				return PostResult.WasSuccess;
			}
			catch(Exception e)
			{
				Log.Error(e);
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
				Log.Error($"deck {deck.Name} has no HearthStatsId");
				filtered.Remove(deck);
			}
			var invalidId = filtered.Where(d => !Regex.IsMatch(d.HearthStatsId, @"^\d+$")).ToList();
			foreach(var deck in invalidId)
			{
				Log.Error("deck " + deck.Name + " has no valid HearthStatsId");
				filtered.Remove(deck);
			}
			if(!filtered.Any())
				return PostResult.Failed;

			Log.Info("deleting decks: " + filtered.Select(d => d.Name).Aggregate((c, n) => c + ", " + n));

			var ids = filtered.Select(d => long.Parse(d.HearthStatsId)).ToArray();

			var url = BaseUrl + "/decks/delete/?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {deck_id = ids});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					Log.Info("deleted decks");
					return PostResult.WasSuccess;
				}
				Log.Error(response);
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> DeleteMatchesAsync(List<GameStats> games)
		{
			var validGames = games.Where(g => g != null).ToList();
			if(!validGames.Any())
			{
				Log.Error("all games are null");
				return PostResult.Failed;
			}
			var noHearthStatsId = games.Where(g => !g.HasHearthStatsId).ToList();
			if(noHearthStatsId.Any())
			{
				foreach(var game in noHearthStatsId)
				{
					Log.Error("game has no HearthStatsId " + game);
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
				Log.Error("game has no valid HearthStatsId " + game);
				validGames.Remove(game);
			}
			if(!validGames.Any())
				return PostResult.Failed;

			Log.Info("deleting games: " + validGames.Select(g => g.ToString()).Aggregate((c, n) => c + ", " + n));

			var url = BaseUrl + "/matches/delete?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {match_id = validGames.Select(g => long.Parse(g.HearthStatsId))});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					Log.Info("deleted game");
					return PostResult.WasSuccess;
				}
				Log.Error(response);
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> MoveMatchAsync(GameStats game, Deck newDeck)
		{
			if(game == null)
			{
				Log.Error("game is null");
				return PostResult.Failed;
			}
			if(!game.HasHearthStatsId)
			{
				Log.Error("game has no HearthStatsId");
				return PostResult.Failed;
			}
			if(newDeck == null)
			{
				Log.Error("deck is null");
				return PostResult.Failed;
			}
			if(!newDeck.HasHearthStatsId)
			{
				Log.Error("deck has no HearthStatsId");
				return PostResult.Failed;
			}
			long deckId;
			if(!long.TryParse(newDeck.HearthStatsId, out deckId))
			{
				Log.Info("deck has invalid HearthStatsId");
				return PostResult.Failed;
			}

			long gameId;
			if(!long.TryParse(game.HearthStatsId, out gameId))
			{
				Log.Error("error: game has invalid HearthStatsId");
				return PostResult.Failed;
			}
			Log.Info("moving game: " + game);

			var url = BaseUrl + "/matches/move?auth_token=" + _authToken;
			var data = JsonConvert.SerializeObject(new {match_id = new[] {gameId}, deck_id = deckId});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					Log.Info("moved game");
					return PostResult.WasSuccess;
				}
				Log.Error(response);
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> UpdateDeckAsync(Deck editedDeck)
		{
			if(editedDeck == null)
			{
				Log.Error("deck is null");
				return PostResult.Failed;
			}
			if(!editedDeck.HasHearthStatsId)
			{
				Log.Info("deck does not exist yet, uploading");
				return await PostDeckAsync(editedDeck);
			}
			Log.Info("editing deck: " + editedDeck);
			var url = BaseUrl + "/decks/edit?auth_token=" + _authToken;
			var cards = editedDeck.Cards.Where(Database.IsActualCard).Select(x => new CardObject(x));
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
				if(json.status.ToString() == "200")
					return PostResult.WasSuccess;
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> CreatArenaRunAsync(Deck deck)
		{
			if(deck == null)
			{
				Log.Error("deck is null");
				return PostResult.Failed;
			}
			if(deck.IsArenaDeck == false)
			{
				Log.Error("deck is not a viable arena deck");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsArenaId)
			{
				Log.Warn("arena deck already posted");
				return PostResult.Failed;
			}
			if(deck.HasVersions)
			{
				Log.Error("arena deck cannot have versions");
				return PostResult.Failed;
			}
			if(deck.HasHearthStatsId)
			{
				if(deck.CheckIfArenaDeck() == false)
				{
					Log.Error("deck has non-arena games");
					return PostResult.Failed;
				}
				if(deck.DeckStats.Games.Count <= 1)
				{
					Log.Warn("deck has hearthstats id but no arena id. deleting and uploading as arena deck.");
					await DeleteDeckAsync(deck);
					deck.HearthStatsId = "";
					deck.HearthStatsDeckVersionId = "";
				}
				else
				{
					Log.Error("deck already has games but no arena id. cannot upload deck.");
					return PostResult.Failed;
				}
			}
			Log.Info("creating new arena run: " + deck);

			var url = BaseUrl + "/arena_runs/new?auth_token=" + _authToken;
			var cards = deck.Cards.Where(Database.IsActualCard).Select(x => new CardObject(x));
			var data = JsonConvert.SerializeObject(new {@class = deck.Class, cards});
			try
			{
				var response = await PostAsync(url, data);
				dynamic json = JsonConvert.DeserializeObject(response);
				if(json.status.ToString() == "200")
				{
					deck.HearthStatsArenaId = json.data.id;
					Log.Info("assigned arena id to deck: " + deck.HearthStatsArenaId);
					return PostResult.WasSuccess;
				}
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return PostResult.Failed;
			}
		}

		public static async Task<PostResult> PostArenaMatch(GameStats game, Deck deck)
		{
			if(!IsValidArenaGame(game))
				return PostResult.Failed;
			if(!deck.HasHearthStatsArenaId)
			{
				Log.Error("can not upload game, deck has no HearthStatsArenaId");
				return PostResult.Failed;
			}
			Log.Info("uploading arena match: " + game);

			var url = BaseUrl + "/matches?auth_token=" + _authToken;

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
				if(json.status.ToString() == "200")
					return PostResult.WasSuccess;
				return PostResult.Failed;
			}
			catch(Exception e)
			{
				Log.Error(e);
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
				Log.Warn(string.Format(baseMsg, "null"));
				return false;
			}
			if(game.IsClone)
			{
				Log.Warn(string.Format(baseMsg, "IsClone"));
				return false;
			}
			if(ValidGameModes.All(mode => game.GameMode != mode))
			{
				Log.Warn(string.Format(baseMsg, "invalid game mode: " + game.GameMode));
				return false;
			}
			if(game.Result == GameResult.None)
			{
				Log.Warn(string.Format(baseMsg, "invalid result: none"));
				return false;
			}
			if(game.HasHearthStatsId)
			{
				Log.Warn(string.Format(baseMsg, "already submitted"));
				return false;
			}
			return true;
		}

		public static bool IsValidArenaGame(GameStats game)
		{
			var baseMsg = "Game " + game + " is no valid arena game ({0})";
			if(game == null)
			{
				Log.Warn(string.Format(baseMsg, "null"));
				return false;
			}
			if(game.IsClone)
			{
				Log.Warn(string.Format(baseMsg, "IsClone"));
				return false;
			}
			if(game.GameMode != GameMode.Arena)
			{
				Log.Warn(string.Format(baseMsg, "invalid game mode: " + game.GameMode));
				return false;
			}
			if(game.HasHearthStatsId)
			{
				Log.Warn(string.Format(baseMsg, "already submitted"));
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