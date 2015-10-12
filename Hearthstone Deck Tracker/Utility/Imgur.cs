using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker
{
	public static class Imgur
	{
		public static async Task<string> Upload(string clientId, MemoryStream image, string name = null)
		{
			const string url = @"https://api.imgur.com/3/upload";

			WebClient web = new WebClient();
			web.Headers.Add("Authorization", "Client-ID " + clientId);

			NameValueCollection Keys = new NameValueCollection();
			try 
			{
				var imgBase64 = Convert.ToBase64String(image.GetBuffer());
				Keys.Add("image", imgBase64);
				if (name != null)
					Keys.Add("name", name);

				byte[] responseArray = await web.UploadValuesTaskAsync(url, Keys);

				var reader = new StreamReader(new MemoryStream(responseArray), Encoding.Default);
				var json = reader.ReadToEnd();
				var resp = JsonConvert.DeserializeObject<ImgurResponse>(json);

				Logger.WriteLine("Response (" + resp.status + ") " +  resp.data.link, "Imgur");
				if (resp.success && resp.status == 200)
				{
					return resp.data.link;
				}
				else
				{
					throw new Exception("response code " + resp.status);
				}
			}
			catch (Exception s) 
			{ 
				Logger.WriteLine("Upload to imgur failed: " + s.Message); 
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
