using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class ConfigWrapper
	{
		public static bool ArenaStatsShowLegends
		{
			get { return Config.Instance.ArenaStatsShowLegends; }
			set
			{
				Config.Instance.ArenaStatsShowLegends = value;
				Config.Save();
			}
		}

		public static DateTime? ArenaStatsTimeFrameCustomStart
		{
			get { return Config.Instance.ArenaStatsTimeFrameCustomStart; }
			set
			{
				Config.Instance.ArenaStatsTimeFrameCustomStart = value;
				Config.Save();
			}
		}

		public static DateTime? ArenaStatsTimeFrameCustomEnd
		{
			get { return Config.Instance.ArenaStatsTimeFrameCustomEnd; }
			set
			{
				Config.Instance.ArenaStatsTimeFrameCustomEnd = value;
				Config.Save();
			}
		}

		public static bool ArenaStatsIncludeArchived
		{
			get { return Config.Instance.ArenaStatsIncludeArchived; }
			set
			{
				Config.Instance.ArenaStatsIncludeArchived = value;
				Config.Save();
			}
		}

		public static bool ArenaRewardDialog
		{
			get { return Config.Instance.ArenaRewardDialog; }
			set
			{
				Config.Instance.ArenaRewardDialog = value;
				Config.Save();
			}
		}

		public static int StatsWindowHeight
		{
			get { return Config.Instance.StatsWindowHeight; }
			set
			{
				Config.Instance.StatsWindowHeight = value;
				Config.Save();
			}
		}

		public static int StatsWindowWidth
		{
			get { return Config.Instance.StatsWindowWidth; }
			set
			{
				Config.Instance.StatsWindowWidth = value;
				Config.Save();
			}
		}
	}
}
