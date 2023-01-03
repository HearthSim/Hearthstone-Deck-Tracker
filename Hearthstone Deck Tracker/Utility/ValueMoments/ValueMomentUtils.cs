using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMomentUtils
	{
		internal static Dictionary<string, object> GetPersonalStatsProperties()
		{
			var hsSettings = new Dictionary<string, bool> {
				{ "stats_record_ranked", Config.Instance.RecordRanked },
				{ "stats_record_arena", Config.Instance.RecordArena },
				{ "stats_record_brawl", Config.Instance.RecordBrawl },
				{ "stats_record_casual", Config.Instance.RecordCasual },
				{ "stats_record_friendly", Config.Instance.RecordFriendly },
				{ "stats_record_adventure_practice", Config.Instance.RecordPractice },
				{ "stats_record_spectator", Config.Instance.RecordSpectator },
				{ "stats_record_duels", Config.Instance.RecordDuels },
				{ "stats_record_other", Config.Instance.RecordOther },
			};
			return GetEnabledDisabledFranchiseSettings("personal_stats", hsSettings);
		}

		private static Dictionary<string, object> GetEnabledDisabledFranchiseSettings(
			string settingsName,
			Dictionary<string, bool> franchiseProperties
		)
		{
			return new Dictionary<string, object>
			{
				{
					$"hdt_{settingsName.ToLower()}_settings_enabled",
					franchiseProperties.Where(x => x.Value == true)
						.Select(x => x.Key)
						.ToArray()
				},
				{
					$"hdt_{settingsName.ToLower()}_settings_disabled",
					franchiseProperties.Where(x => x.Value == false)
						.Select(x => x.Key)
						.ToArray()
				}
			};
		}
	}
}
