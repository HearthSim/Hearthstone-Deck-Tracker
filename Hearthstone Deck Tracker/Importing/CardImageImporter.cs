using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class CardImageImporter
	{
		const string imageurl = "https://art.hearthstonejson.com/v1/256x/SCH_614.jpg";
		const string HearthstoneArtUrl = "https://art.hearthstonejson.com/v1/256x";
		private static string StoreImagesPath = Config.AppDataPath + "\\Images";

		static CardImageImporter()
		{
			CheckForAndAddImageFolder();
		}

		public static async Task DownloadCardAsync(string cardId)
		{
			var requestUrl = string.Format($"{HearthstoneArtUrl}/{cardId}.jpg");
			var storageUrl = string.Format($"{StoreImagesPath}\\{cardId}.jpg");
			Log.Info($"Starting download for {cardId}");
			try
			{
				using(WebClient client = new WebClient())
				{
					await client.DownloadFileTaskAsync(new Uri(requestUrl), storageUrl);
					Log.Info($"Finished downloading {cardId}");
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message + " was the error :(");
			}
		}

		private static bool ImageFolderExists() => Directory.Exists(StoreImagesPath);

		private static void CheckForAndAddImageFolder()
		{
			if(!ImageFolderExists())
				Directory.CreateDirectory(StoreImagesPath);
		}

	}
}
