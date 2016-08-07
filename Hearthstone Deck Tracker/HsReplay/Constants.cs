#region

using System.IO;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class Constants
	{
		public const string BaseUrl = "https://hsreplay.net";
		public const string BaseUploadUrl = "https://upload.hsreplay.net";
		private const string BaseApi = "/api/v1";
		private const string UploadRequestApi = "/replay/upload/request";
		private const string TokenApi = "/tokens";
		private const string ClaimAccountApi = "/claim_account/";
		private const string CacheFile = "hsreplay.cache";

		public static string BaseApiUrl => BaseUrl + BaseApi;
		public static string BaseUploadApiUrl => BaseUploadUrl + BaseApi;
		public static string UploadRequestUrl => BaseUploadApiUrl + UploadRequestApi;
		public static string TokensUrl => BaseApiUrl + TokenApi;
		public static string CacheFilePath => Path.Combine(Config.Instance.DataDir, CacheFile);
		public static string ClaimAccountUrl => BaseApiUrl + ClaimAccountApi;
	}
}
