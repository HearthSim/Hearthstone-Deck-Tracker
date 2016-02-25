#region

using System;
using System.Windows;

#endregion

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

		public static bool ArenaSummaryChartsExpanded
		{
			get { return Config.Instance.ArenaSummaryChartsExpanded; }
			set
			{
				Config.Instance.ArenaSummaryChartsExpanded = value;
				Config.Save();
			}
		}

		public static Visibility ShowLastPlayedDateOnDeckVisibility => Config.Instance.ShowLastPlayedDateOnDeck ? Visibility.Visible : Visibility.Collapsed;

		public static Visibility UseButtonVisiblity => Config.Instance.AutoUseDeck ? Visibility.Collapsed : Visibility.Visible;
	}
}