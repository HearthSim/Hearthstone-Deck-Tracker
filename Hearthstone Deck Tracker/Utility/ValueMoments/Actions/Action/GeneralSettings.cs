using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class GeneralSettings
	{
		[JsonProperty("upload_my_collection_automatically")]
		public bool UploadMyCollectionAutomatically { get => Config.Instance.SyncCollection; }

		[JsonProperty("upload_replays_automatically")]
		public bool UploadReplaysAutomatically { get => Config.Instance.HsReplayAutoUpload; }

		[JsonProperty("share_notification")]
		public bool ShareNotification { get => Config.Instance.ShowReplayShareToast; }

		[JsonProperty("overlay_hide_completely")]
		public bool OverlayHideCompletely { get => Config.Instance.HideOverlay; }

		[JsonProperty("overlay_hide_if_hs_in_background")]
		public bool OverlayHideIfHSInBackground { get => Config.Instance.HideInBackground; }

		[JsonProperty("overlay_menu_hide_if_hs_in_background")]
		public bool OverlayMenuHideIfHSInBackground { get => Config.Instance.HideMenuOverlayInBackground; }

		[JsonProperty("card_tooltips")]
		public bool CardTooltips { get => Config.Instance.OverlayCardToolTips; }

		[JsonProperty("analytics_submit_anonymous_data")]
		public bool AnalyticsSubmitAnonymousData { get => Config.Instance.GoogleAnalytics; }

		[JsonProperty("start_with_windows")]
		public bool StartWithWindows { get => Config.Instance.StartWithWindows; }

		[JsonProperty("start_minimized")]
		public bool StartMinimized { get => Config.Instance.StartMinimized; }

		[JsonProperty("close_to_tray")]
		public bool CloseToTray { get => Config.Instance.CloseToTray; }

		[JsonProperty("minimize_to_tray")]
		public bool MinimizeToTray { get => Config.Instance.MinimizeToTray; }

		[JsonProperty("show_news_bar")]
		public bool ShowNewsBar { get => Config.Instance.IgnoreNewsId < (Remote.Config.Data?.News?.Id ?? 0); }
	}
}
