using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public enum CardAssetType
	{
		FullImage,
		Portrait,
		Tile,
		Hero,
	}

	public static class AssetDownloaders
	{
		public static AssetDownloader<Hearthstone.Card, BitmapImage>? cardPortraitDownloader;
		public static AssetDownloader<Hearthstone.Card, BitmapImage>? cardTileDownloader;
		public static AssetDownloader<Hearthstone.Card, BitmapImage>? cardImageDownloader;
		public static AssetDownloader<Hearthstone.Card, BitmapImage>? heroImageDownloader;

		static AssetDownloaders()
		{
			try
			{
				cardPortraitDownloader = new AssetDownloader<Hearthstone.Card, BitmapImage>(
					Path.Combine(Config.AppDataPath, "Images", "CardPortraits"),
					(Hearthstone.Card card) => $"https://art.hearthstonejson.com/v1/256x/{card.Id}.jpg",
					(Hearthstone.Card card) => $"{card.Id}.jpg",
					Helper.BitmapImageFromBytes,
					maxCacheSize: 500,
					placeholderAsset: "pack://application:,,,/Resources/faceless_manipulator.png"
				);
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card portraits: {e.Message}");
			}

			try
			{
				cardTileDownloader = new AssetDownloader<Hearthstone.Card, BitmapImage>(
					Path.Combine(Config.AppDataPath, "Images", "CardTiles"),
					(Hearthstone.Card card) => $"https://art.hearthstonejson.com/v1/tiles/{card.Id}.jpg",
					(Hearthstone.Card card) => $"{card.Id}.jpg",
					Helper.BitmapImageFromBytes,
					maxCacheSize: 10_000, // About 2KB per tile. Caching up to 20MB.
					placeholderAsset: "pack://application:,,,/Resources/card-tile-placeholder.jpg"
				);
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card tiles: {e.Message}");
			}

			try
			{
				cardImageDownloader = new AssetDownloader<Hearthstone.Card, BitmapImage>(
					Path.Combine(Config.AppDataPath, "Images", "CardImages"),
					card => $"https://art.hearthstonejson.com/v1/{(card.BaconCard ? "bgs" : "render")}/latest" +
					        $"/{Helper.GetCardLanguage()}/{(Config.Instance.HighResolutionCardImages ? "512x" : "256x")}" +
					        $"/{card.Id}{(card.BaconTriple ? "_triple" : "")}.png",
					card => $"{card.Id}{(card.BaconTriple ? "_triple" : "")}.png",
					Helper.BitmapImageFromBytes,
					maxCacheSize: 200,
					placeholderAsset: "pack://application:,,,/Resources/faceless_manipulator.png"
				);
				ConfigWrapper.Bindable.CardResolutionChanged += () => cardImageDownloader.ClearStorage();
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card images: {e.Message}");
			}

			try
			{
				heroImageDownloader = new AssetDownloader<Hearthstone.Card, BitmapImage>(
					Path.Combine(Config.AppDataPath, "Images", "Heroes"),
					card => $"https://art.hearthstonejson.com/v1/heroes/latest/256x/{card.Id}.png",
					card => $"{card.Id}.png",
					Helper.BitmapImageFromBytes,
					maxCacheSize: 200,
					placeholderAsset: "pack://application:,,,/Resources/faceless_manipulator.png"
				);
			}
			catch(ArgumentException e)
			{
				Log.Error($"Could not create asset downloader to download card images: {e.Message}");
			}
		}


		public static AssetDownloader<Hearthstone.Card, BitmapImage>? GetCardAssetDownloader(CardAssetType type)
		{
			switch(type)
			{
				case CardAssetType.FullImage:
					return cardImageDownloader;
				case CardAssetType.Portrait:
					return cardPortraitDownloader;
				case CardAssetType.Tile:
					return cardTileDownloader;
				case CardAssetType.Hero:
					return heroImageDownloader;
				default:
					throw new NotImplementedException($"CardAssetType {type} is not implemented");
			}
		}
	}
}
