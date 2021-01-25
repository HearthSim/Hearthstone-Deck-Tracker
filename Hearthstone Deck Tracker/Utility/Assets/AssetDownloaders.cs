using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public static class AssetDownloaders
	{
		public static AssetDownloader<string> cardPortraitDownloader;
		public static AssetDownloader<Hearthstone.Card> cardImageDownloader;
		private static bool _initialized = false;
		public static void SetupAssetDownloaders()
		{
			if(_initialized)
				return;
			_initialized = true;
			try
			{
				cardPortraitDownloader = new AssetDownloader<string>(
					Path.Combine(Config.AppDataPath, "Images", "CardPortraits"),
					(string cardId) => $"https://art.hearthstonejson.com/v1/256x/{cardId}.jpg",
					(string cardId) => $"{cardId}.jpg"
				);
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card portraits: {e.Message}");
			}
			try
			{
				cardImageDownloader = new AssetDownloader<Hearthstone.Card>(
					Path.Combine(Config.AppDataPath, "Images", "CardImages"),
					(Hearthstone.Card card) => $"https://art.hearthstonejson.com/v1/{(card.BaconCard ? "bgs" : "render")}/latest" +
					$"/{Config.Instance.SelectedLanguage}/{(Config.Instance.HighResolutionCardImages ? "512x" : "256x")}" +
					$"/{card.Id}.png",
					(Hearthstone.Card card) => $"{card.Id}.png",
					200
				);
				ConfigWrapper.CardImageConfigs.CardResolutionChanged += () => cardImageDownloader.ClearStorage();
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card images: {e.Message}");
			}
		}
	}
}
