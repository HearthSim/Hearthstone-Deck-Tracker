using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class ApiManager
	{
		private const string ApiKey = "d1050cd9e8ed4ff7853dd109ee428505";
		private const string ApiKeyHeaderName = "x-hsreplay-api-key";
		private const string ApiUploadTokenHeaderName = "x-hsreplay-upload-token";

		public static Header ApiKeyHeader => new Header(ApiKeyHeaderName, ApiKey);
		public static async Task<Header> GetUploadTokenHeader() => new Header(ApiUploadTokenHeaderName, await GetUploadToken());
		
		private static string _uploadToken;
		private static async Task<string> GetUploadToken()
		{
			if(!string.IsNullOrEmpty(_uploadToken))
				return _uploadToken;
			string token;
			try
			{
				if(File.Exists(UploadTokenFilePath))
				{
					using(var reader = new StreamReader(UploadTokenFilePath))
						token = reader.ReadToEnd();
					if(!string.IsNullOrEmpty(token))
					{
						Log.Info("Loaded upload-token from file.");
						_uploadToken = token;
						return token;
					}
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			try
			{
				var response = await Web.PostAsync(GenerateUploadTokenUrl, "", ApiKeyHeader);
				using(var responseStream = response.GetResponseStream())
				using(var reader = new StreamReader(responseStream))
				{
					dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());
					token = (string)json.single_site_upload_token;
				}
				if(string.IsNullOrEmpty(token))
					throw new Exception("Reponse contained no upload-token.");
			}
			catch(Exception e)
			{
				Log.Error(e);
				throw new Exception("Webrequest to obtain upload-token failed.", e);
			}
			try
			{
				using(var writer = new StreamWriter(UploadTokenFilePath))
					writer.Write(token);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			if(string.IsNullOrEmpty(token))
				throw new Exception("Could not obtain an upload-token.");
			Log.Info("Obtained new upload-token.");
			_uploadToken = token;
			return token;
		}
	}
}