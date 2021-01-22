using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public static class AssetDownloaders
	{
		public static AssetDownloader cardPortraitDownloader;
		public static AssetDownloader cardImageDownloader;
		private static bool _initialized = false;
		public static void SetupAssetDownloaders()
		{
			if(_initialized)
				return;
			_initialized = true;
			try
			{
				cardPortraitDownloader = new AssetDownloader(Path.Combine(Config.AppDataPath, "Images", "CardPortraits"), new List<string>() { "https://art.hearthstonejson.com/v1/256x" }, "jpg", (string cardId) => $"{ cardId}");
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card portraits: {e.Message}");
			}
			try
			{
				cardImageDownloader = new AssetDownloader(
					Path.Combine(Config.AppDataPath, "Images", "CardImages"),
					new List<string>() {
						"https://art.hearthstonejson.com/v1/render/latest",
						"https://art.hearthstonejson.com/v1/bgs/latest"
					},
					"png",
					(string cardId) => $"{Config.Instance.SelectedLanguage}/{(Config.Instance.HighResolutionCardImages ? "512x" : "256x")}/{cardId}", 5);
				ConfigWrapper.CardImageConfigs.CardResolutionChanged += () => cardImageDownloader.ClearStorage();
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card images: {e.Message}");
			}
		}
	}
}
