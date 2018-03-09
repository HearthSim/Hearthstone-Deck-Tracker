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

			internal class NewsData
			{
				[JsonProperty("id")]
				public int Id { get; set; }

				[JsonProperty("items")]
				public List<string> Items { get; set; } = new List<string>();
			}
		}
	}
}