using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay;
using HSReplay.Requests;
using HSReplay.Responses;
using Newtonsoft.Json.Linq;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class ApiWrapper
	{
		private static readonly HsReplayClient Client = new HsReplayClient("089b2bc6-3c26-4aab-adbe-bcfd5bb48671", Helper.GetUserAgent(), config: TryGetConfig());
		private static bool _requestedNewToken;

		private static async Task<string> GetUploadToken()
		{
			if(!string.IsNullOrEmpty(Account.Instance.UploadToken))
				return Account.Instance.UploadToken!;
			UploadTokenHistory.Write("Trying to request new token");
			string token;
			try
			{
				Log.Info("Requesting new upload token...");
				token = await Client.CreateUploadToken();
				if(string.IsNullOrEmpty(token))
					throw new Exception("Reponse contained no upload-token.");
			}
			catch(Exception e)
			{
				Log.Error(e);
				UploadTokenHistory.Write("Requesting new token failed:\n" + e);
				throw new Exception("Webrequest to obtain upload-token failed.", e);
			}
			Account.Instance.UploadToken = token;
			Account.Instance.TokenClaimed = false;
			Account.Save();
			UploadTokenHistory.Write("Received " + token);
			Log.Info("Received new upload-token.");
			return token;
		}

		public static async Task UpdateUploadTokenStatus()
		{
			Log.Info("Checking token status...");
			try
			{
				var token = await GetUploadToken();
				var accountStatus = await Client.GetAccountStatus(token);
				Account.Instance.TokenClaimed = accountStatus?.User != null;
				Account.Save();
				Log.Info($"Token is {(Account.Instance.TokenClaimed == true ? "" : "not ")}claimed");
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				var response = ex.Response as HttpWebResponse;
				if(response?.StatusCode == HttpStatusCode.NotFound && !_requestedNewToken)
				{
					Log.Info("Requesting new token");
					_requestedNewToken = true;
					Account.Instance.Reset();
					await UpdateUploadTokenStatus();
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public static async Task<LogUploadRequest> CreateUploadRequest(UploadMetaData metaData)
			=> await Client.CreateUploadRequest(metaData, await GetUploadToken());


		public static async Task UploadLog(LogUploadRequest uploadRequest, string[] logLines)
			=> await Client.UploadLog(uploadRequest, logLines);

		private static ClientConfig? TryGetConfig()
		{
			var file = new FileInfo(Path.Combine(Config.AppDataPath, "hsreplaynet.xml"));
			if(!file.Exists)
				return null;
			try
			{
				Log.Warn("Loading custom hsreplaynet config!");
				return XmlManager<ClientConfig>.Load(file.FullName);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		internal static async Task<DecksData?> GetAvailableDecks()
		{
			Log.Info("Fetching available decks...");
			try
			{
				var token = await GetUploadToken();
				var data = await Client.GetAvailableDecks(token);
				if(data == null)
					return null;
				return new DecksData
				{
					ClientTimeStamp = DateTime.Now,
					ServerTimeStamp = data.ServerTimeStamp,
					Decks = data.Data.Properties().Select(p => p.Name).ToArray()
				};
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		// Todo: Add classic support
		internal static async Task<DeckWinrateData?> GetDeckWinrates(string deckId, bool wild)
		{
			Log.Info("Fetching winrates for deck " + deckId);
			try
			{
				var token = await GetUploadToken();
				var data = await Client.GetDeckWinrates(deckId, wild, token);
				if(data == null)
					return null;

				var winrates = data.Data["data"]?.Children().OfType<JProperty>().Where(x => x.Values().Any());
				var dict = winrates.ToDictionary(
					x => x.Name,
					x => x.Value[0]?["winrate"]?.Value<double>() ?? 0.0
				);

				var totalWinrate = data.Data?.SelectToken("metadata.total_winrate")?.Value<double>();

				return new DeckWinrateData
				{
					ClientTimeStamp = DateTime.Now,
					ServerTimeStamp = data.ServerTimeStamp,
					TotalWinrate = totalWinrate ?? 0.0,
					ClassWinrates = dict
				};
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task<PlayerTrialStatus?> GetPlayerTrialStatus(string name, ulong accountHi, ulong accountLo)
		{
			try
			{
				return await Client.GetPlayerTrialStatus(name, accountHi, accountLo);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task<string?> ActivatePlayerTrial(string name, ulong accountHi, ulong accountLo)
		{
			try
			{
				var response = await Client.ActivatePlayerTrial(name, accountHi, accountLo);
				return response?.Token;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task<BattlegroundsQuestStats[]?> GetTier7QuestStats(string token, BattlegroundsQuestStatsParams parameters)
		{
			try
			{
				return await Client.GetTier7QuestStats(token, parameters);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task<BattlegroundsHeroPickStats?> GetTier7HeroPickStats(string token, BattlegroundsHeroPickStatsParams parameters)
		{
			try
			{
				return await Client.GetTier7HeroPickStats(token, parameters);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		public static async Task PostMulliganGuideFeedback(MulliganGuideFeedbackParams parameters)
		{
			try
			{
				await Client.PostMulliganGuideFeedback(parameters);
			}
			catch(Exception e)
			{
#if(DEBUG)
				Log.Error(e);
#endif
			}
		}

		public static async Task PostBattlegroundsHeroPickFeedback(
			BattlegroundsHeroPickFeedbackParams parameters, bool isDuos
		)
		{
			try
			{
				if(isDuos)
					await Client.PostBattlegroundsDuosHeroPickFeedback(parameters);
				else
					await Client.PostBattlegroundsHeroPickFeedback(parameters);
			}
			catch(Exception e)
			{
#if(DEBUG)
				Log.Error(e);
#endif
			}
		}

		public static async Task<MulliganGuideStatusData?> GetMulliganGuideStatus(MulliganGuideStatusParams parameters)
		{
			try
			{
				return await Client.GetMulliganGuideStatus(parameters);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}
	}
}
