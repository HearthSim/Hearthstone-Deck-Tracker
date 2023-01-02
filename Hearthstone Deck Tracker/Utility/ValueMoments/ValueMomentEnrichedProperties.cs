using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	public class ValueMomentEnrichedProperties
	{
		public enum BaseSettings
		{
			[MixpanelProperty("domain")]
			Domain,

			[MixpanelProperty("is_authenticated")]
			IsAuthenticated,

			[MixpanelProperty("screen_height")]
			ScreenHeight,

			[MixpanelProperty("screen_width")]
			ScreenWidth,

			[MixpanelProperty("card_language")]
			CardLanguage,

			[MixpanelProperty("appearance_language")]
			AppearanceLanguage,

			[MixpanelProperty("hdt_plugins")]
			HDTPlugins,
		}

		private readonly Dictionary<HDTGeneralSettings, bool> hdtGeneralSettings;

		public ValueMomentEnrichedProperties(string actionEventId, int? maxDailyOccurrences)
		{
			hdtGeneralSettings = new Dictionary<HDTGeneralSettings, bool> {
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

			if(maxDailyOccurrences != null)
			{
				var curEventDailyCount = DailyEventsCount.Instance.GetEventDailyCount(actionEventId);
				var newCurrentDailyCount = DailyEventsCount.Instance.UpdateEventDailyCount(actionEventId);
				var eventCounterWasReset = curEventDailyCount > 0 && newCurrentDailyCount == 1;

				CurrentDailyOccurrences = newCurrentDailyCount;
				MaximumDailyOccurrences = maxDailyOccurrences;
				if(eventCounterWasReset)
					PreviousDailyOccurrences = curEventDailyCount;
			}
		}

		public int? CurrentDailyOccurrences { get; private set; }
		public int? MaximumDailyOccurrences { get; private set; }
		public int? PreviousDailyOccurrences { get; private set; }

		public Dictionary<BaseSettings, object> ClientSettings
		{
			get
			{
				Rectangle rect = Helper.GetHearthstoneMonitorRect();
				return new Dictionary<BaseSettings, object> {
					{ BaseSettings.Domain , "hsreplay.net" },
					{ BaseSettings.IsAuthenticated , HSReplayNetOAuth.IsFullyAuthenticated },
					{ BaseSettings.ScreenHeight , rect.Height },
					{ BaseSettings.ScreenWidth , rect.Width },
					{ BaseSettings.CardLanguage , Config.Instance.SelectedLanguage.Substring(0, 2) },
					{ BaseSettings.AppearanceLanguage , Config.Instance.Localization.ToString().Substring(0, 2) },
					{ BaseSettings.HDTPlugins , PluginManager.Instance.Plugins.Where(x => x.IsEnabled).Select(x => x.Name) }
				};
			}
		}

		public HDTGeneralSettings[] HDTGeneralSettingsEnabled
		{
			get => hdtGeneralSettings.Where(x => x.Value == true)
						.Select(x => x.Key)
						.ToArray();
		}

		public HDTGeneralSettings[] HDTGeneralSettingsDisabled
		{
			get => hdtGeneralSettings.Where(x => x.Value == false)
						.Select(x => x.Key)
						.ToArray();
		}
	}
}
