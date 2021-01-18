using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.IO;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public static class AssetDownloaders
	{
		public static AssetDownloader cardPortraitDownloader;
		public static AssetDownloader cardImageDownloader;
		static AssetDownloaders()
		{
			try
			{
				cardPortraitDownloader = new AssetDownloader(Path.Combine(Config.AppDataPath, "Images", "CardPortraits"), "https://art.hearthstonejson.com/v1/256x", "jpg", (string cardId) => $"{ cardId}");
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card portraits: {e.Message}");
			}
			try
			{
				cardImageDownloader = new AssetDownloader(Path.Combine(Config.AppDataPath, "Images", "CardImages"), "https://art.hearthstonejson.com/v1/render/latest", "png", (string cardId) => Path.Combine(Config.Instance.SelectedLanguage, "256x", $"{cardId}"));
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card images: {e.Message}");
			}

		}
	}
}
