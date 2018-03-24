using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay;
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
				return Account.Instance.UploadToken;
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

		public static async Task UploadPack(PackData data) => await Client.UploadPack(data, await GetUploadToken());

		private static ClientConfig TryGetConfig()
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

		internal static async Task<DecksData> GetAvailableDecks()
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

		internal static async Task<DeckWinrateData> GetDeckWinrates(string deckId, bool wild)
		{
			Log.Info("Fetching winrates for deck " + deckId);
			try
			{
				var token = await GetUploadToken();
				var data = await Client.GetDeckWinrates(deckId, wild, token);
				if(data == null)
					return null;

				var winrates = data.Data["data"].Children().OfType<JProperty>().Where(x => x.Values().Any());
				var dict = winrates.ToDictionary(
					x => x.Name,
					x => x.Value[0]["winrate"].Value<double>()
				);

				var totalWinrate = data.Data.SelectToken("metadata.total_winrate").Value<double>();

				return new DeckWinrateData
				{
					ClientTimeStamp = DateTime.Now,
					ServerTimeStamp = data.ServerTimeStamp,
					TotalWinrate = totalWinrate,
					ClassWinrates = dict
				};
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}
	}
}
