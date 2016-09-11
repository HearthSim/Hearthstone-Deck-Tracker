using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class ApiWrapper
	{
		private static readonly HsReplayClient Client = new HsReplayClient("089b2bc6-3c26-4aab-adbe-bcfd5bb48671", "HDT/" + Helper.GetCurrentVersion(), config: TryGetConfig());

		private static async Task<string> GetUploadToken()
		{
			if(!string.IsNullOrEmpty(Account.Instance.UploadToken))
				return Account.Instance.UploadToken;
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
				throw new Exception("Webrequest to obtain upload-token failed.", e);
			}
			Account.Instance.UploadToken = token;
			Account.Save();
			Log.Info("Received new upload-token.");
			return token;
		}

		public static async Task<string> GetClaimAccountUrl()
		{
			try
			{
				var token = await GetUploadToken();
				Log.Info("Getting claim url...");
				return "https://hsreplay.net" + await Client.GetClaimAccountUrl(token);
			}
			catch(Exception e)
			{
				Log.Error(e);
				ErrorManager.AddError("Error claiming account", e.Message);
			}
			return null;
		}

		public static async Task UpdateAccountStatus()
		{
			Log.Info("Checking account status...");
			try
			{
				var token = await GetUploadToken();
				var accountStatus = await Client.GetAccountStatus(token);
				Account.Instance.Id = accountStatus?.User?.Id ?? 0;
				Account.Instance.Username = accountStatus?.User?.Username;
				Account.Instance.Status = accountStatus?.User != null ? AccountStatus.Registered : AccountStatus.Anonymous;
				Account.Instance.LastUpdated = DateTime.Now;
				Account.Save();
				Log.Info($"Id={Account.Instance.Id}, Username={Account.Instance.Username}, Status={Account.Instance.Status}");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				ErrorManager.AddError("Error retrieving HSReplay account status", ex.Message);
			}
		}

		public static async Task<LogUploadRequest> CreateUploadRequest(UploadMetaData metaData) 
			=> await Client.CreateUploadRequest(metaData, await GetUploadToken());


		public static async Task UploadLog(LogUploadRequest uploadRequest, string[] logLines) 
			=> await Client.UploadLog(uploadRequest, logLines);

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
	}
}
