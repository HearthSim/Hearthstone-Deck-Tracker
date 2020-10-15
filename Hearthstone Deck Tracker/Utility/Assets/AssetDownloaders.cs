using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public static class AssetDownloaders
	{
		public static AssetDownloader cardImageDownloader;

		static AssetDownloaders()
		{
			try
			{
				cardImageDownloader = new AssetDownloader(Path.Combine(Config.AppDataPath, "Images", "CardPortraits"), "https://art.hearthstonejson.com/v1/256x", (string cardId) => $"{cardId}.jpg");
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download cardimages: {e.Message}");
			}
		}
	}
}
