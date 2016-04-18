#region

using System;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	internal class HsReplayUploader
	{
		public static async Task<UploadResult> UploadXml(string xml)
		{
			Log.Info("Uploading...");
			try
			{
				var response = await Web.PostAsync(UploadUrl, xml, ApiManager.ApiKeyHeader, await ApiManager.GetUploadTokenHeader());
				Log.Info(response.StatusCode.ToString());
				using(var responseStream = response.GetResponseStream())
				using(var reader = new StreamReader(responseStream))
				{
					dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());
					var id = json.replay_uuid;
					Log.Info("Success!");
					return UploadResult.Successful(id);
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return UploadResult.Failed;
		}

		public static async Task<UploadResult> UploadXmlFromFile(string filePath)
		{
			string content;
			using(var sr = new StreamReader(filePath))
				content = sr.ReadToEnd();
			return await UploadXml(content);
		}
	}
}