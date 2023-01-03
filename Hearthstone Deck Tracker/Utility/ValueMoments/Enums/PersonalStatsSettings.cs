
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum PersonalStatsSettings
	{
		[MixpanelProperty("stats_record_ranked")]
		StatsRecordRanked,

		[MixpanelProperty("stats_record_arena")]
		StatsRecordArena,

		[MixpanelProperty("stats_record_brawl")]
		StatsRecordBrawl,

		[MixpanelProperty("stats_record_casual")]
		StatsRecordCasual,

		[MixpanelProperty("stats_record_friendly")]
		StatsRecordFriendly,

		[MixpanelProperty("stats_record_adventure_practice")]
		StatsRecordAdventurePractice,

		[MixpanelProperty("stats_record_spectator")]
		StatsRecordSpectator,

		[MixpanelProperty("stats_record_duels")]
		StatsRecordDuels,

		[MixpanelProperty("stats_record_other")]
		StatsRecordOther,
	}
}
