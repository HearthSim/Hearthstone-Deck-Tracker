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

		public static bool DeckPickerWildIncludesStandard
		{
			get { return Config.Instance.DeckPickerWildIncludesStandard; }
			set
			{
				Config.Instance.DeckPickerWildIncludesStandard = value;
				Config.Save();
			}
		}

		public static bool ForceLocalReplayViewer
		{
			get { return Config.Instance.ForceLocalReplayViewer; }
			set
			{
				Config.Instance.ForceLocalReplayViewer = value;
				Config.Save();
			}
		}

		public static bool HsReplayAutoUpload
		{
			get { return Config.Instance.HsReplayAutoUpload; }
			set
			{
				Config.Instance.HsReplayAutoUpload = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadRanked
		{
			get { return Config.Instance.HsReplayUploadRanked; }
			set
			{
				Config.Instance.HsReplayUploadRanked = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadCasual
		{
			get { return Config.Instance.HsReplayUploadCasual; }
			set
			{
				Config.Instance.HsReplayUploadCasual = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadArena
		{
			get { return Config.Instance.HsReplayUploadArena; }
			set
			{
				Config.Instance.HsReplayUploadArena = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadBrawl
		{
			get { return Config.Instance.HsReplayUploadBrawl; }
			set
			{
				Config.Instance.HsReplayUploadBrawl = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadFriendly
		{
			get { return Config.Instance.HsReplayUploadFriendly; }
			set
			{
				Config.Instance.HsReplayUploadFriendly = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadPractice
		{
			get { return Config.Instance.HsReplayUploadPractice; }
			set
			{
				Config.Instance.HsReplayUploadPractice = value;
				Config.Save();
			}
		}

		public static bool HsReplayUploadSpectator
		{
			get { return Config.Instance.HsReplayUploadSpectator; }
			set
			{
				Config.Instance.HsReplayUploadSpectator = value;
				Config.Save();
			}
		}

		public static Visibility ShowLastPlayedDateOnDeckVisibility => Config.Instance.ShowLastPlayedDateOnDeck ? Visibility.Visible : Visibility.Collapsed;

		public static Visibility UseButtonVisiblity => Config.Instance.AutoUseDeck ? Visibility.Collapsed : Visibility.Visible;
	}
}