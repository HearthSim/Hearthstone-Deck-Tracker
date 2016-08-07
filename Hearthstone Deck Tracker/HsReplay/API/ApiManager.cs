using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class ApiManager
	{
		private const string ApiKey = "089b2bc6-3c26-4aab-adbe-bcfd5bb48671";
		private const string ApiKeyHeaderName = "X-Api-Key";

		public static Header ApiKeyHeader => new Header(ApiKeyHeaderName, ApiKey);
		public static async Task<Header> GetUploadTokenHeader() => new Header("Authorization", "Token " + await GetUploadToken());

		private static async Task<string> GetUploadToken()
		{
			if(!string.IsNullOrEmpty(Account.Instance.UploadToken))
				return Account.Instance.UploadToken;
			string token;
			try
			{
				var content = JsonConvert.SerializeObject(new {api_key = ApiKey});
				var response = await Web.PostJsonAsync($"{TokensUrl}/", content, false, true);
				using(var responseStream = response.GetResponseStream())
				using(var reader = new StreamReader(responseStream))
				{
					dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());
					token = (string)json.key;
				}
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
			Log.Info("Obtained new upload-token.");
			return token;
		}

		private static async Task<string> GetAccountUrl() => $"{TokensUrl}/{await GetUploadToken()}/";

		public static async Task ClaimAccount()
		{
			try
			{
				var token = await GetUploadToken();
				Log.Info("Getting claim url...");
				var response = await Web.PostAsync(ClaimAccountUrl, string.Empty, false, true, new Header("Authorization", $"Token {token}"));
				using(var responseStream = response.GetResponseStream())
				using(var reader = new StreamReader(responseStream))
				{
					dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());
					Log.Info("Opening browser to claim account...");
					Process.Start($"{BaseUrl}{json.url}");
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
				ErrorManager.AddError("Error claiming account", e.Message);
			}
		}

		public static async Task UpdateAccountStatus()
		{
			Log.Info("Checking account status...");
			try
			{
				var response = await Web.GetAsync(await GetAccountUrl(), true);
				if(response.StatusCode == HttpStatusCode.OK)
				{
					using(var responseStream = response.GetResponseStream())
					using(var reader = new StreamReader(responseStream))
					{
						dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());
						var user = json.user;
						Account.Instance.Id = user != null ? user.id : 0;
						Account.Instance.Username = user != null ? user.username : string.Empty;
						Account.Instance.Status = user != null ? AccountStatus.Registered : AccountStatus.Anonymous;
						Account.Instance.LastUpdated = DateTime.Now;
						Account.Save();
					}
				}
				Log.Info($"Response={response.StatusCode}, Id={Account.Instance.Id}, Username={Account.Instance.Username}, Status={Account.Instance.Status}");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				ErrorManager.AddError("Error retrieving HSReplay account status", ex.ToString());
			}
		}
	}
}
