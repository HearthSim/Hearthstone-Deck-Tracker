using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class PersonalStatsSettings
	{

		[JsonProperty("stats_record_ranked")]
		public bool StatsRecordRanked { get => Config.Instance.RecordRanked; }

		[JsonProperty("stats_record_arena")]
		public bool StatsRecordArena { get => Config.Instance.RecordArena; }

		[JsonProperty("stats_record_brawl")]
		public bool StatsRecordBrawl { get => Config.Instance.RecordBrawl; }

		[JsonProperty("stats_record_casual")]
		public bool StatsRecordCasual { get => Config.Instance.RecordCasual; }

		[JsonProperty("stats_record_friendly")]
		public bool StatsRecordFriendly { get => Config.Instance.RecordFriendly; }

		[JsonProperty("stats_record_adventure_practice")]
		public bool StatsRecordAdventurePractice { get => Config.Instance.RecordPractice; }

		[JsonProperty("stats_record_spectator")]
		public bool StatsRecordSpectator { get => Config.Instance.RecordSpectator; }

		[JsonProperty("stats_record_duels")]
		public bool StatsRecordDuels { get => Config.Instance.RecordDuels; }

		[JsonProperty("stats_record_other")]
		public bool StatsRecordOther { get => Config.Instance.RecordOther; }
	}
}
