using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using ClientSettingsEnum = Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums.ClientSettings;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class ClientProperties
	{
		private readonly Dictionary<HDTGeneralSettings, bool> _hdtGeneralSettings;
		private readonly Dictionary<PersonalStatsSettings, bool>? _personalStatsSettings;

		public ClientProperties(bool withPersonalStatsSettings)
		{
			_hdtGeneralSettings = new Dictionary<HDTGeneralSettings, bool> {
				{ HDTGeneralSettings.UploadMyCollectionAutomatically, Config.Instance.SyncCollection },
				{ HDTGeneralSettings.UploadReplaysAutomatically, Config.Instance.HsReplayAutoUpload },
				{ HDTGeneralSettings.ShareNotification, Config.Instance.ShowReplayShareToast },
				{ HDTGeneralSettings.OverlayHideCompletely, Config.Instance.HideOverlay },
				{ HDTGeneralSettings.OverlayHideIfHSInBackground, Config.Instance.HideInBackground },
				{ HDTGeneralSettings.OverlayMenuHideIfHSInBackground, Config.Instance.HideMenuOverlayInBackground },
				{ HDTGeneralSettings.CardTooltips, Config.Instance.OverlayCardToolTips },
				{ HDTGeneralSettings.AnalyticsSubmitAnonymousData, Config.Instance.GoogleAnalytics },
				{ HDTGeneralSettings.StartWithWindows, Config.Instance.StartWithWindows },
				{ HDTGeneralSettings.StartMinimized, Config.Instance.StartMinimized },
				{ HDTGeneralSettings.CloseToTray, Config.Instance.CloseToTray },
				{ HDTGeneralSettings.MinimizeToTray, Config.Instance.MinimizeToTray },
				{ HDTGeneralSettings.ShowNewsBar, Config.Instance.IgnoreNewsId < (Remote.Config.Data?.News?.Id ?? 0) },
			};

			HasPersonalStatsSettings = withPersonalStatsSettings;
			if (withPersonalStatsSettings)
				_personalStatsSettings = new Dictionary<PersonalStatsSettings, bool> {
					{ PersonalStatsSettings.StatsRecordRanked, Config.Instance.RecordRanked },
					{ PersonalStatsSettings.StatsRecordArena, Config.Instance.RecordArena },
					{ PersonalStatsSettings.StatsRecordBrawl, Config.Instance.RecordBrawl },
					{ PersonalStatsSettings.StatsRecordCasual, Config.Instance.RecordCasual },
					{ PersonalStatsSettings.StatsRecordFriendly, Config.Instance.RecordFriendly },
					{ PersonalStatsSettings.StatsRecordAdventurePractice, Config.Instance.RecordPractice },
					{ PersonalStatsSettings.StatsRecordSpectator, Config.Instance.RecordSpectator },
					{ PersonalStatsSettings.StatsRecordDuels, Config.Instance.RecordDuels },
					{ PersonalStatsSettings.StatsRecordOther, Config.Instance.RecordOther },
				};
		}

		public Dictionary<ClientSettingsEnum, object> ClientSettings
		{
			get
			{
				var rect = Helper.GetHearthstoneMonitorRect();
				return new Dictionary<ClientSettingsEnum, object> {
					{ ClientSettingsEnum.IsAuthenticated , HSReplayNetOAuth.IsFullyAuthenticated },
					{ ClientSettingsEnum.ScreenHeight , rect.Height },
					{ ClientSettingsEnum.ScreenWidth , rect.Width },
					{ ClientSettingsEnum.CardLanguage , Config.Instance.SelectedLanguage.Substring(0, 2) },
					{ ClientSettingsEnum.AppearanceLanguage , Config.Instance.Localization.ToString().Substring(0, 2) },
					{ ClientSettingsEnum.HDTPlugins , PluginManager.Instance.Plugins.Where(x => x.IsEnabled).Select(x => x.Name) }
				};
			}
		}

		public HDTGeneralSettings[] HDTGeneralSettingsEnabled
		{
			get => _hdtGeneralSettings.Where(x => x.Value)
						.Select(x => x.Key)
						.ToArray();
		}

		public HDTGeneralSettings[] HDTGeneralSettingsDisabled
		{
			get => _hdtGeneralSettings.Where(x => !x.Value)
						.Select(x => x.Key)
						.ToArray();
		}

		public bool HasPersonalStatsSettings { get; }

		public PersonalStatsSettings[]? PersonalStatsSettingsEnabled
		{
			get => _personalStatsSettings?.Where(x => x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public PersonalStatsSettings[]? PersonalStatsSettingsDisabled
		{
			get => _personalStatsSettings?.Where(x => !x.Value)
				.Select(x => x.Key)
				.ToArray();
		}
	}
}
