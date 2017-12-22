#region

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Imgur
	{
		public static async Task<string> Upload(string clientId, MemoryStream image, string name = null)
		{
			const string url = @"https://api.imgur.com/3/upload";

			var web = new WebClient();
			web.Headers.Add("Authorization", "Client-ID " + clientId);

			var keys = new NameValueCollection();
			try
			{
				var imgBase64 = Convert.ToBase64String(image.GetBuffer());
				keys.Add("image", imgBase64);
				if(name != null)
					keys.Add("name", name);

				var responseArray = await web.UploadValuesTaskAsync(url, keys);

				var reader = new StreamReader(new MemoryStream(responseArray), Encoding.Default);
				var json = reader.ReadToEnd();
				var resp = JsonConvert.DeserializeObject<ImgurResponse>(json);

				Log.Info("Response (" + resp.status + ") " + resp.data.link);
				if(resp.success && resp.status == 200)
					return resp.data.link;
				throw new Exception("response code " + resp.status);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return null;
		}

		private class ImgurResponse
		{
			public ImgurDataImage data { get; set; }
			public bool success { get; set; }
			public int status { get; set; }

			public class ImgurDataImage
			{
				public string id { get; set; }
				public string title { get; set; }
				public string name { get; set; }
				public string deletehash { get; set; }
				public string link { get; set; }
			}
		}
	}
}
