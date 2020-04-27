using System;
using System.Collections.Generic;
using HearthDb.Enums;
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

			[JsonProperty("arena")]
			public ArenaData Arena { get; set; }

			[JsonProperty("whizbang_decks")]
			public List<WhizbangDeck> WhizbangDecks { get; set; }

			[JsonProperty("battlegrounds_tag_overrides")]
			public List <TagOverride> BattlegroundsTagOverrides { get; set; }

			[JsonProperty("bobs_buddy")]
			public BobsBuddyData BobsBuddy { get; set; }

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

			internal class ArenaData
			{
				[JsonProperty("current_sets")]
				public List<CardSet> CurrentSets { get; set; }

				[JsonProperty("exclusive_secrets")]
				public List<string> ExclusiveSecrets { get; set; }

				[JsonProperty("banned_secrets")]
				public List<string> BannedSecrets { get; set; }
			}

			internal class WhizbangDeck
			{

				[JsonProperty("title")]
				public string Title { get; set; }

				[JsonProperty("class")]
				public CardClass Class { get; set; }

				[JsonProperty("deck_id")]
				public int DeckId { get; set; }

				[JsonProperty("cards")]
				public List<RemoteConfigCard> Cards { get; set; }
			}

			internal class RemoteConfigCard
			{
				[JsonProperty("dbf_id")]
				public int DbfId { get; set; }

				[JsonProperty("count")]
				public int Count { get; set; }
			}


			internal class TagOverride
			{
				[JsonProperty("dbf_id")]
				public int DbfId { get; set; }

				[JsonProperty("tag")]
				public GameTag Tag { get; set; }

				[JsonProperty("value")]
				public int Value { get; set; }
			}

			internal class BobsBuddyData
			{
				[JsonProperty("disabled")]
				public bool Disabled { get; set; }

				[JsonProperty("min_required_version")]
				public string MinRequiredVersion { get; set; }

				[JsonProperty("sentry_reporting")]
				public bool SentryReporting { get; set; }

				[JsonProperty("metric_sampling")]
				public double MetricSampling { get; set; }
			}
		}
	}
}
