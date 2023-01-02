
namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum HDTGeneralSettings
	{
		[MixpanelProperty("hdt_general_settings_enabled")]
		HdtGeneralSettingsEnabled,

		[MixpanelProperty("hdt_general_settings_disabled")]
		HdtGeneralSettingsDisabled,

		[MixpanelProperty("upload_my_collection_automatically")]
		UploadMyCollectionAutomatically,

		[MixpanelProperty("upload_replays_automatically")]
		UploadReplaysAutomatically,

		[MixpanelProperty("share_notification")]
		ShareNotification,

		[MixpanelProperty("overlay_hide_completely")]
		OverlayHideCompletely,

		[MixpanelProperty("overlay_hide_if_hs_in_background")]
		OverlayHideIfHSInBackground,

		[MixpanelProperty("overlay_menu_hide_if_hs_in_background")]
		OverlayMenuHideIfHSInBackground,

		[MixpanelProperty("card_tooltips")]
		CardTooltips,

		[MixpanelProperty("analytics_submit_anonymous_data")]
		AnalyticsSubmitAnonymousData,

		[MixpanelProperty("start_with_windows")]
		StartWithWindows,

		[MixpanelProperty("start_minimized")]
		StartMinimized,

		[MixpanelProperty("close_to_tray")]
		CloseToTray,

		[MixpanelProperty("minimize_to_tray")]
		MinimizeToTray,

		[MixpanelProperty("show_news_bar")]
		ShowNewsBar,
	}
}
