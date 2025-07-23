using System.Collections.Generic;
using HearthDb.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		internal class Config
		{
			[JsonProperty("news")]
			public NewsData? News { get; set; }

			[JsonProperty("collection_banner")]
			public CollectionBannerData? CollectionBanner { get; set; }

			[JsonProperty("battlegrounds_short_names")]
			public List<CardShortName>? BattlegroundsShortNames { get; set; }

			[JsonProperty("battlegrounds_tag_overrides")]
			public List<TagOverride>? BattlegroundsTagOverrides { get; set; }

			[JsonProperty("bobs_buddy")]
			public BobsBuddyData? BobsBuddy { get; set; }

			[JsonProperty("tier7")]
			public Tier7Data? Tier7 { get; set; }

			[JsonProperty("mulligan_guide")]
			public MulliganGuideData? MulliganGuide { get; set; }

			[JsonProperty("arenasmith")]
			public ArenasmithData? Arenasmith { get; set; }

			[JsonProperty("draw_card_blacklist")]
			public List<CardInfo>? DrawCardBlacklist { get; set; }
		}

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

		internal class RemoteConfigCard
		{
			[JsonProperty("dbf_id")]
			public int DbfId { get; set; }

			[JsonProperty("count")]
			public int Count { get; set; }
		}

		internal class CardShortName
		{
			[JsonProperty("dbf_id")]
			public int DbfId { get; set; }

			[JsonProperty("short_name")]
			public string? ShortName { get; set; }
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
			public string? MinRequiredVersion { get; set; }

			[JsonProperty("sentry_min_required_version")]
			public string? SentryMinRequiredVersion { get; set; }

			[JsonProperty("metric_sampling")]
			public double MetricSampling { get; set; }

			[JsonProperty("log_lines_kept")]
			public int LogLinesKept { get; set; }

			[JsonProperty("data_quality_warning")]
			public bool DataQualityWarning { get; set; }
		}

		internal class Tier7Data
		{
			[JsonProperty("disabled")]
			public bool Disabled { get; set; }
		}

		internal class MulliganGuideData
		{
			[JsonProperty("disabled")]
			public bool Disabled { get; set; }
		}

		internal class ArenasmithData
		{
			[JsonProperty("disabled")]
			public bool Disabled { get; set; }
		}

		internal class CardInfo
		{
			[JsonProperty("dbf_id")]
			public int DbfId { get; set; }
		}
	}
}
