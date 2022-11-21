using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMomentUtils
	{
		// Basic Properties
		public const string NUM_CLICK_BATTLEGROUNDS_MINION_TAB = "num_click_battlegrounds_minion_tab";
		public const string NUM_HOVER_OPPONENT_MERC_ABILITY = "num_hover_opponent_merc_ability";
		public const string NUM_HOVER_MERC_TASK_OVERLAY = "num_hover_merc_task_overlay";

		public const string CURRENT_DAILY_OCCURRENCES = "cur_daily_occurrences";
		public const string MAXIMUM_DAILY_OCCURRENCES = "max_daily_occurrences";
		public const string PREVIOUS_DAILY_OCCURRENCES = "prev_daily_occurrences";

		// General Settings
		public const string HDT_GENERAL_SETTINGS_ENABLED = "hdt_general_settings_enabled";
		public const string HDT_GENERAL_SETTINGS_DISABLED = "hdt_general_settings_disabled";
		public const string UPLOAD_MY_COLLECTION_AUTOMATICALLY = "upload_my_collection_automatically";
		public const string UPLOAD_REPLAYS_AUTOMATICALLY = "upload_replays_automatically";
		public const string SHARE_NOTIFICATION = "share_notification";
		public const string OVERLAY_HIDE_COMPLETELY = "overlay_hide_completely";
		public const string OVERLAY_HIDE_IF_HS_IN_BACKGROUND = "overlay_hide_if_hs_in_background";
		public const string CARD_TOOLTIPS = "card_tooltips";
		public const string ANALYTICS_SUBMIT_ANONYMOUS_DATA = "analytics_submit_anonymous_data";
		public const string START_WITH_WINDOWS = "start_with_windows";
		public const string START_MINIMIZED = "start_minimized";
		public const string CLOSE_TO_TRAY = "close_to_tray";
		public const string MINIMIZE_TO_TRAY = "minimize_to_tray";
		public const string SHOW_NEWS_BAR = "show_news_bar";

		// Battlegrounds Settings
		public const string BG_GENERAL_SETTINGS_ENABLED = "hdt_battlegrounds_settings_enabled";
		public const string BG_GENERAL_SETTINGS_DISABLED = "hdt_battlegrounds_settings_disabled";
		public const string BG_TIERS = "bg_tiers";
		public const string BG_TURN_COUNTER = "bg_turn_counter";
		public const string BB_COMBAT_SIMULATIONS = "bb_combat_simulations";
		public const string BB_RESULTS_DURING_COMBAT = "bb_results_during_combat";
		public const string BB_RESULTS_DURING_SHOPPING = "bb_results_during_shopping";
		public const string BB_ALWAYS_SHOW_AVERAGE_DAMAGE = "bb_always_show_average_damage";
		public const string SESSION_RECAP = "session_recap";
		public const string SESSION_RECAP_BETWEEN_GAMES = "session_recap_between_games";
		public const string MINIONS_BANNED = "minions_banned";
		public const string START_AND_CURRENT_MMR = "start_and_current_mmr";
		public const string LATEST_10_GAME = "latest_10_game";

		// Mercenaries Settings
		public const string MERC_GENERAL_SETTINGS_ENABLED = "hdt_mercenaries_settings_enabled";
		public const string MERC_GENERAL_SETTINGS_DISABLED = "hdt_mercenaries_settings_disabled";
		public const string OPPONENT_MERC_ABILITIES_ON_HOVER = "opponent_merc_abilities_on_hover";
		public const string PLAYER_MERC_ABILITIES_ON_HOVER = "player_merc_abilities_on_hover";
		public const string ABILITY_ICONS_ABOVE_OPPONENT_MERCS = "ability_icons_above_opponent_mercs";
		public const string ABILITY_ICONS_BELOW_PLAYER_MERCS = "ability_icons_below_player_mercs";
		public const string TASKS_PANEL = "tasks_panel";

		private static string GetEventId(VMAction action)
		{
			action.Properties.TryGetValue("franchise", out var franchise);
			action.Properties.TryGetValue("sub_franchise", out var subFranchise);
			var id = action.EventName;

			if (franchise != null)
				id += $"_{((string[])franchise)[0].ToLower()}";

			if (subFranchise != null)
			{
				string[] subFranchiseArray = (string[])subFranchise;
				if (subFranchiseArray.Length > 0)
					id += $"_{((string[])subFranchise)[0].ToLower()}";
			}

			return id;
		}

		internal static Dictionary<string, object> EnrichedEventProperties(VMAction action)
		{
			var eventId = GetEventId(action);

			Rectangle rect = Helper.GetHearthstoneMonitorRect();
			var hdtGeneralSettings = new Dictionary<string, bool> {
				{ UPLOAD_MY_COLLECTION_AUTOMATICALLY, Config.Instance.SyncCollection },
				{ UPLOAD_REPLAYS_AUTOMATICALLY, Config.Instance.HsReplayAutoUpload },
				{ SHARE_NOTIFICATION, Config.Instance.ShowReplayShareToast },
				{ OVERLAY_HIDE_COMPLETELY, Config.Instance.HideOverlay },
				{ OVERLAY_HIDE_IF_HS_IN_BACKGROUND, Config.Instance.HideInBackground },
				{ CARD_TOOLTIPS, Config.Instance.OverlayCardToolTips },
				{ ANALYTICS_SUBMIT_ANONYMOUS_DATA, Config.Instance.GoogleAnalytics },
				{ START_WITH_WINDOWS, Config.Instance.StartWithWindows },
				{ START_MINIMIZED, Config.Instance.StartMinimized },
				{ CLOSE_TO_TRAY, Config.Instance.CloseToTray },
				{ MINIMIZE_TO_TRAY, Config.Instance.MinimizeToTray },
				{ SHOW_NEWS_BAR, Config.Instance.IgnoreNewsId < (Remote.Config.Data?.News?.Id ?? 0) },
			};

			action.Properties.TryGetValue("sub_franchise", out var subFranchise);
			var enrichedProperties = new Dictionary<string, object>(
				GetEnabledDisabledFranchiseSettings("general", hdtGeneralSettings)
			)
			{
				{ "domain", "hsreplay.net" },
				{ "is_authenticated", HSReplayNetOAuth.IsFullyAuthenticated },
				{ "screen_height", rect.Height },
				{ "screen_width", rect.Width },
				{ "card_language", Config.Instance.SelectedLanguage.Substring(0, 2) },
				{ "appearance_language", Config.Instance.Localization.ToString().Substring(0, 2) },
				{ "hdt_plugins", PluginManager.Instance.Plugins.Where(x => x.IsEnabled).Select(x => x.Name) }
			};

			if (action.MaxDailyOccurrences != null)
			{
				var curEventDailyCount = DailyEventsCount.Instance.GetEventDailyCount(eventId);
				var newCurrentDailyCount = DailyEventsCount.Instance.UpdateEventDailyCount(eventId);
				var eventCounterWasReset = curEventDailyCount > 0 && newCurrentDailyCount == 1;

				enrichedProperties.Add(CURRENT_DAILY_OCCURRENCES, newCurrentDailyCount);
				enrichedProperties.Add(MAXIMUM_DAILY_OCCURRENCES, action.MaxDailyOccurrences);
				if(eventCounterWasReset)
					enrichedProperties.Add(PREVIOUS_DAILY_OCCURRENCES, curEventDailyCount);
			}

			if (subFranchise == null)
				enrichedProperties.Add("sub_franchise", new string[] { } );

			return enrichedProperties;
		}

		internal static Dictionary<string, object> GetFranchiseProperties(Franchise franchise)
		{
			if(franchise == Franchise.HSConstructed)
			{
				var hsSettings = new Dictionary<string, bool> {
					{ "hide_decks", Config.Instance.HideDecksInOverlay },
					{ "hide_timers", Config.Instance.HideTimers },
				};
				return GetEnabledDisabledFranchiseSettings("hsconstructed", hsSettings);
			}

			if(franchise == Franchise.Battlegrounds)
			{
				var bgSettings = new Dictionary<string, bool> {
					{ BG_TIERS, Config.Instance.ShowBattlegroundsTiers },
					{ BG_TURN_COUNTER, Config.Instance.ShowBattlegroundsTurnCounter },
					{ BB_COMBAT_SIMULATIONS, Config.Instance.RunBobsBuddy },
					{ BB_RESULTS_DURING_COMBAT, Config.Instance.ShowBobsBuddyDuringCombat },
					{ BB_RESULTS_DURING_SHOPPING, Config.Instance.ShowBobsBuddyDuringShopping },
					{ BB_ALWAYS_SHOW_AVERAGE_DAMAGE, Config.Instance.AlwaysShowAverageDamage },
					{ SESSION_RECAP, Config.Instance.ShowSessionRecap },
					{ SESSION_RECAP_BETWEEN_GAMES, Config.Instance.ShowSessionRecapBetweenGames },
					{ MINIONS_BANNED, Config.Instance.ShowSessionRecapMinionsBanned },
					{ START_AND_CURRENT_MMR, Config.Instance.ShowSessionRecapStartCurrentMMR },
					{ LATEST_10_GAME, Config.Instance.ShowSessionRecapLatestGames },
				};
				return GetEnabledDisabledFranchiseSettings("battlegrounds", bgSettings);
			}

			if(franchise == Franchise.Mercenaries)
			{
				var hsSettings = new Dictionary<string, bool> {
					{OPPONENT_MERC_ABILITIES_ON_HOVER, Config.Instance.ShowMercsOpponentHover },
					{PLAYER_MERC_ABILITIES_ON_HOVER, Config.Instance.ShowMercsPlayerHover },
					{ABILITY_ICONS_ABOVE_OPPONENT_MERCS, Config.Instance.ShowMercsOpponentAbilityIcons },
					{ABILITY_ICONS_BELOW_PLAYER_MERCS, Config.Instance.ShowMercsPlayerAbilityIcons },
					{TASKS_PANEL, Config.Instance.ShowMercsTasks },

				};
				return GetEnabledDisabledFranchiseSettings("mercenaries", hsSettings);
			}

			return new Dictionary<string, object>();
		}

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
