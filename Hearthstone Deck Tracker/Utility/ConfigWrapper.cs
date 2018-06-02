#region

using System;
using System.Text.RegularExpressions;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Analytics;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class ConfigWrapper
	{
		public static event Action ReplayAutoUploadChanged;
		public static event Action CollectionSyncingChanged;
		public static event Action IgnoreNewsIdChanged;

		public static bool CardDbIncludeWildOnlyCards
		{
			get { return Config.Instance.CardDbIncludeWildOnlyCards; }
			set
			{
				Config.Instance.CardDbIncludeWildOnlyCards = value;
				Config.Save();
			}
		}

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

		public static bool ConstructedSummaryChartsExpanded
		{
			get { return Config.Instance.ConstructedSummaryChartsExpanded; }
			set
			{
				Config.Instance.ConstructedSummaryChartsExpanded = value;
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

		public static bool ConstructedStatsIncludeArchived
		{
			get { return Config.Instance.ConstructedStatsIncludeArchived; }
			set
			{
				Config.Instance.ConstructedStatsIncludeArchived = value;
				Config.Save();
			}
		}


		public static bool ConstructedStatsAsPercent
		{
			get { return Config.Instance.ConstructedStatsAsPercent; }
			set
			{
				Config.Instance.ConstructedStatsAsPercent = value;
				Config.Save();
			}
		}
		public static DateTime? ConstructedStatsTimeFrameCustomStart
		{
			get { return Config.Instance.ConstructedStatsTimeFrameCustomStart; }
			set
			{
				Config.Instance.ConstructedStatsTimeFrameCustomStart = value;
				Config.Save();
			}
		}

		public static DateTime? ConstructedStatsTimeFrameCustomEnd
		{
			get { return Config.Instance.ConstructedStatsTimeFrameCustomEnd; }
			set
			{
				Config.Instance.ConstructedStatsTimeFrameCustomEnd = value;
				Config.Save();
			}
		}

		public static bool StatsAutoRefresh
		{
			get { return Config.Instance.StatsAutoRefresh; }
			set
			{
				Config.Instance.StatsAutoRefresh = value;
				Config.Save();
			}
		}

		public static bool ConstructedStatsApplyTagFilters
		{
			get { return Config.Instance.ConstructedStatsApplyTagFilters; }
			set
			{
				Config.Instance.ConstructedStatsApplyTagFilters = value;
				Config.Save();
			}
		}

		public static bool CollectionSyncingEnabled
		{
			get { return Config.Instance.SyncCollection; }
			set
			{
				if(Config.Instance.SyncCollection != value)
				{
					Config.Instance.SyncCollection = value;
					Config.Save();
					Influx.OnCollectionSyncingEnabled(value);
				}
				CollectionSyncingChanged?.Invoke();
			}
		}

		public static bool HsReplayAutoUpload
		{
			get { return Config.Instance.HsReplayAutoUpload; }
			set
			{
				Config.Instance.HsReplayAutoUpload = value;
				Config.Save();
				Influx.OnHsReplayAutoUploadChanged(value);
				ReplayAutoUploadChanged?.Invoke();
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

		public static Visibility ShowDateOnDeckVisibility => Config.Instance.ShowDateOnDeck ? Visibility.Visible : Visibility.Collapsed;

		public static Visibility UseButtonVisiblity => Config.Instance.AutoUseDeck ? Visibility.Collapsed : Visibility.Visible;

		public static bool ConstructedStatsActiveDeckOnly
		{
			get
			{
				if(DeckList.Instance.ActiveDeck != null)
					return Config.Instance.ConstructedStatsActiveDeckOnly;
				if(Config.Instance.ConstructedStatsActiveDeckOnly)
				{
					Config.Instance.ConstructedStatsActiveDeckOnly = false;
					Config.Save();
				}
				return Config.Instance.ConstructedStatsActiveDeckOnly;
			}
			set
			{
				var newValue = DeckList.Instance.ActiveDeck != null && value;
				if(Config.Instance.ConstructedStatsActiveDeckOnly == newValue)
					return;
				Config.Instance.ConstructedStatsActiveDeckOnly = newValue;
				Config.Save();
			}
		}

		public static string ConstructedStatsRankFilterMin
		{
			get { return Config.Instance.ConstructedStatsRankFilterMin; }
			set
			{
				Config.Instance.ConstructedStatsRankFilterMin = ValidateRank(value, false);
				Config.Save();
			}
		}
		public static string ConstructedStatsRankFilterMax
		{
			get { return Config.Instance.ConstructedStatsRankFilterMax; }
			set
			{
				Config.Instance.ConstructedStatsRankFilterMax = ValidateRank(value, true);
				Config.Save();
			}
		}

		public static string ConstructedStatsCustomSeasonMin
		{
			get { return Config.Instance.ConstructedStatsCustomSeasonMin.ToString(); }
			set
			{
				Config.Instance.ConstructedStatsCustomSeasonMin = ValidateSeason(value, false) ?? 1;
				Config.Save();
			}
		}
		public static string ConstructedStatsCustomSeasonMax
		{
			get { return Config.Instance.ConstructedStatsCustomSeasonMax.ToString(); }
			set
			{
				Config.Instance.ConstructedStatsCustomSeasonMax = ValidateSeason(value, true);
				Config.Save();
			}
		}

		public static string ArenaStatsCustomSeasonMin
		{
			get { return Config.Instance.ArenaStatsCustomSeasonMin.ToString(); }
			set
			{
				Config.Instance.ArenaStatsCustomSeasonMin = ValidateSeason(value, false) ?? 1;
				Config.Save();
			}
		}
		public static string ArenaStatsCustomSeasonMax
		{
			get { return Config.Instance.ArenaStatsCustomSeasonMax.ToString(); }
			set
			{
				Config.Instance.ArenaStatsCustomSeasonMax = ValidateSeason(value, true);
				Config.Save();
			}
		}

		public bool HsReplayShareToast
		{
			get { return Config.Instance.ShowReplayShareToast; }
			set
			{
				Config.Instance.ShowReplayShareToast = value;
				Config.Save();
			}
		}

		public bool CheckForDevUpdates
		{
			get { return Config.Instance.CheckForDevUpdates; }
			set
			{
				Config.Instance.CheckForDevUpdates = value;
				Config.Instance.AllowDevUpdates = null;
				Config.Save();
			}
		}

		public static int IgnoreNewsId
		{
			get => Config.Instance.IgnoreNewsId;
			set
			{
				Config.Instance.IgnoreNewsId = value;
				Config.Save();
				IgnoreNewsIdChanged?.Invoke();
			}
		}

		public bool WindowCardToolTips => Config.Instance.WindowCardToolTips;

		private static int? ValidateSeason(string value, bool allowEmpty)
		{
			if(allowEmpty && string.IsNullOrEmpty(value))
				return null;
			if(!int.TryParse(value, out var season))
				throw new ApplicationException("Invalid season");
			if(season < 1)
				throw new ApplicationException("Invalid season. Minimum value: 1");
			var currentSeaon = Helper.CurrentSeason;
			if(season > currentSeaon)
				throw new ApplicationException("Invalid season. Current season: " + currentSeaon);
			return season;
		}

		private static string ValidateRank(string value, bool allowEmpty)
		{
			if(allowEmpty && string.IsNullOrEmpty(value))
				return value;
			var match = Regex.Match(value, @"(?<legend>([lL]))?(?<rank>(\d+))");
			if(!match.Success)
				throw new ApplicationException("Invalid rank");
			var legend = match.Groups["legend"].Success;
			if(int.TryParse(match.Groups["rank"].Value, out var rank))
			{
				if(!legend && rank > 25)
					throw new ApplicationException("Rank can not be higher than 25");
				if(rank < 1)
					throw new ApplicationException("Rank can not be lower than 1");
			}
			else
				throw new ApplicationException("Invalid rank");
			return (legend ? "L" : "") + rank;
		}
	}
}
