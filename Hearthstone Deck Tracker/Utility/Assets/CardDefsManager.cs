using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Assets;

public static class CardDefsManager
{
	private static readonly AssetDownloader<string, HearthDb.CardDefs.CardDefs>? _downloader;

	public static Action? CardsChanged;
	public static Action? InitialDefsLoaded;

	static CardDefsManager()
	{
		try
		{
			_downloader = new AssetDownloader<string, HearthDb.CardDefs.CardDefs>(
				Path.Combine(Config.AppDataPath, "CardDefs"),
				key => $"https://api.hearthstonejson.com/v1/latest/CardDefs.{key}.xml",
				key => $"CardDefs.{key}.xml",
				data =>
				{
					using var ms = new MemoryStream(data);
					return Cards.ParseCardDefs(ms);
				},
				alwaysKeepCached: new HashSet<string> { "base" },
				maxCacheSize: 2 // two languages besides base

			);
		}
		catch(ArgumentException e)
		{
			Log.Error($"Could not create asset downloader to download locale defs: {e.Message}");
		}
	}

	private static bool _loadingDefs;
	public static bool HasLoadedInitialBaseDefs { get; private set; }

	public static async void EnsureLatestCardDefs()
	{
		if(_loadingDefs)
			return;
		_loadingDefs = true;

		try
		{
			if(!HasLoadedInitialBaseDefs)
			{
				var cardDefs = await GetInitialBaseDefs();
				await Task.Run(() => Cards.LoadBaseData(cardDefs));
				HasLoadedInitialBaseDefs = true;
				Log.Info($"Loaded initial base CardDefs: Count={Cards.All.Count}, Build={Cards.Build}");
				InitialDefsLoaded?.Invoke(); // this does not need to wait for locales
				await LoadLocaleIfNeeded();
			}

			if(_downloader == null)
			{
				Log.Warn("No card defs downloader available");
				return;
			}

			_downloader.InvalidateCachedAssets();
			var asset = await Task.Run(async () => await _downloader.GetAssetEntry("base", true));
			if(asset?.Data == null)
			{
				Log.Warn("Could not download base CardDefs");
				return;
			}

			if(asset.NotModified)
				return;

			await Task.Run(() => Cards.LoadBaseData(asset.Data));
			Log.Info($"Successfully loaded base CardDefs: Count={Cards.All.Count}, Build={Cards.Build} (LastModified={asset.LastModified})");
			await LoadLocaleIfNeeded();
		}
		finally
		{
			_loadingDefs = false;
		}
	}

	private static async Task<HearthDb.CardDefs.CardDefs> GetInitialBaseDefs()
	{
		Log.Info("Loading bundled base card defs...");

		var bundled = Cards.GetBundledCardDefsETag();
		if(_downloader == null)
		{
			Log.Info($"Downloader not available, using bundled CardDefs ({bundled.LastModified})");
			return await Task.Run(Cards.GetBundledBaseData);
		}

		var asset = _downloader.GetAssetEntryMetadata("base") ?? _downloader.CreateEmptyAssetEntry("base");

		if(asset.ETag == bundled.ETag)
		{
			Log.Info($"Bundled CardDefs are up-to-date ({bundled.LastModified})");
			// Use bundled data. The data on disk may be out of date. See below.
			asset.Data = await Task.Run(Cards.GetBundledBaseData);
			return asset.Data;
		}

		var bundledLastModified = DateTime.Parse(bundled.LastModified);
		if(!DateTime.TryParse(asset.LastModified, out var assetLastModified) || bundledLastModified > assetLastModified)
		{
			Log.Info($"Bundled CardDefs ({bundled.LastModified}) are newer than downloaded CardDefs ({asset.LastModified})");
			// Overwrite asset with the bundled data so that ETag checks are made against the up-to-date version.
			// This will cause the metadata stored on disk to be out of sync with the actual data stored on disk.
			// For this reason we need to always use the bundled data if the ETag matches going forward (see above)
			asset.ETag = bundled.ETag;
			asset.LastModified = bundled.LastModified;
			asset.Data = await Task.Run(Cards.GetBundledBaseData);
			return asset.Data;
		}

		Log.Info($"Downloaded CardDefs ({asset.LastModified}) are newer than bundled CardDefs ({bundled.LastModified})");

		asset.Data = await Task.Run(() => _downloader.TryGetAssetData("base", false)); // Don't validate, we will invalidate after this anyway
		if(asset.Data != null)
			return asset.Data;

		Log.Info("Could not load downloaded CardDefs from disk, using bundled instead");
		return await Task.Run(Cards.GetBundledBaseData);

	}

	private static async Task LoadLocaleIfNeeded()
	{
		var cardLang = Helper.GetCardLanguage();
		if(IsLocaleInBase(cardLang))
			CardsChanged?.Invoke();
		else
		{
			_loadedLocales.Clear();
			await LoadLocale(cardLang, false); // This will emit the change event
		}
	}

	private static bool IsLocaleInBase(Locale locale) => IsLocaleInBase(locale.ToString());
	private static bool IsLocaleInBase(string locale) => locale is "enUS" or "zhCN";

	private static readonly HashSet<Locale> _loadedLocales = new();
	public static async Task LoadLocale(string langCode, bool allowCache = true)
	{
		if(!HasLoadedInitialBaseDefs || _downloader == null)
			return;
		if(!Enum.TryParse(langCode, out Locale locale) || IsLocaleInBase(locale) || _loadedLocales.Contains(locale))
			return;
		_loadedLocales.Add(locale);

		var cardDefs = await Task.Run(async () => await _downloader.GetAssetData(locale.ToString(), allowCache));
		if(cardDefs == null)
		{
			Log.Warn($"Could not load CardDefs for Locale={locale}");
			return;
		}

		await Task.Run(() => Cards.LoadLocaleData(cardDefs, locale));
		Log.Info($"Successfully loaded CardDefs for Locale={locale}");
		CardsChanged?.Invoke();
	}
}
