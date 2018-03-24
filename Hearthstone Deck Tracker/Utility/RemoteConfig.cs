using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class RemoteConfig : Singleton<RemoteConfig>
	{
		private readonly DataLoader<ConfigData> _loader;

		private RemoteConfig()
		{
			_loader = DataLoader<ConfigData>.JsonFromWeb("https://hsdecktracker.net/config.json");
			_loader.Loaded += d => Loaded?.Invoke(d);
		}

		public ConfigData Data => _loader.TryGetData(out var data) ? data : null;

		public event Action<ConfigData> Loaded;

		public void Load() => _loader.Load();

		internal class ConfigData
		{
			[JsonProperty("news")]
			public NewsData News { get; set; }

			[JsonProperty("collection_banner")]
			public CollectionBannerData CollectionBanner { get; set; }

			internal class NewsData
			{
				[JsonProperty("id")]
				public int Id { get; set; }

				[JsonProperty("items")]
				public List<string> Items { get; set; } = new List<string>();
			}

			internal class CollectionBannerData
			{
				[JsonProperty("visible")]
				public bool Visible { get; set; }

				[JsonProperty("removable_pre_sync")]
				public bool RemovablePreSync { get; set; }

				[JsonProperty("removable_post_sync")]
				public bool RemovablePostSync { get; set; }

				[JsonProperty("removal_id")]
				public int RemovalId { get; set; }
			}
		}
	}
}