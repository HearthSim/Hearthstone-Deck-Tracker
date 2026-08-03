using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class GeneralSettings : IVMDynamicSettings
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

		[JsonProperty("player_active_effects")]
		public bool PlayerActiveEffects { get => !Config.Instance.HidePlayerActiveEffects; }

		[JsonProperty("opponent_active_effects")]
		public bool OpponentActiveEffects { get => !Config.Instance.HideOpponentActiveEffects; }

		[JsonProperty("player_counters")]
		public bool PlayerCounters { get => !Config.Instance.HidePlayerCounters; }

		[JsonProperty("opponent_counters")]
		public bool OpponentCounters { get => !Config.Instance.HideOpponentCounters; }

		[JsonProperty("player_related_cards")]
		public bool PlayerRelatedCards { get => !Config.Instance.HidePlayerRelatedCards; }

		[JsonProperty("opponent_related_cards")]
		public bool OpponentRelatedCards { get => !Config.Instance.HideOpponentRelatedCards; }

		[JsonProperty("highlight_deck_synergies")]
		public bool HighlightDeckSynergies { get => !Config.Instance.HidePlayerHighlightSynergies; }

		[JsonProperty("player_max_resources_widget")]
		public bool PlayerMaxResourceWidget { get => !Config.Instance.HidePlayerMaxResourcesWidget; }

		[JsonProperty("opponent_max_resources_widget")]
		public bool OpponentMaxResourceWidget { get => !Config.Instance.HideOpponentMaxResourcesWidget; }

		[JsonProperty("mulligan_overlay")]
		public bool MulliganOverlay { get => Config.Instance.EnableMulliganGuide; }

		[JsonProperty("mulligan_gv2_overlay")]
		public bool MulliganGV2Overlay { get => Config.Instance.EnableMulliganGV2; }

		// Counters the user forced on or off. Counters left on Auto appear in neither list, so a
		// user who has not touched the setting adds nothing to the payload.
		[JsonIgnore]
		public IEnumerable<string> DynamicEnabledSettings => CounterSettings(CounterVisibility.Enabled);

		[JsonIgnore]
		public IEnumerable<string> DynamicDisabledSettings => CounterSettings(CounterVisibility.Disabled);

		private static IEnumerable<string> CounterSettings(CounterVisibility visibility)
		{
			var settings = CounterVisibilitySettings.Instance;
			return settings.GetCounterIds(true, visibility).Select(id => $"player_counter_{MetricName(id)}")
				.Concat(settings.GetCounterIds(false, visibility).Select(id => $"opponent_counter_{MetricName(id)}"))
				.OrderBy(x => x, System.StringComparer.Ordinal);
		}

		private static string MetricName(string counterId)
		{
			if(counterId.EndsWith("Counter") && counterId.Length > "Counter".Length)
				counterId = counterId.Substring(0, counterId.Length - "Counter".Length);
			return Regex.Replace(counterId, "(?<=[a-z0-9])([A-Z])", "_$1").ToLowerInvariant();
		}
	}
}
