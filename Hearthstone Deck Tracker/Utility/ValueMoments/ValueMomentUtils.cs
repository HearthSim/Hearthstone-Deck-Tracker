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
		public const string TRIALS_ACTIVATED = "trials_activated";
		public const string TRIALS_REMAINING = "trials_remaining";

		// Trial names
		public const string TIER7_OVERLAY_TRIAL = "Tier7Overlay";

		// Tier7
		public const string TIER7_HERO_OVERLAY_DISPLAYED = "tier7_hero_overlay_displayed";
		public const string TIER7_QUEST_OVERLAY_DISPLAYED = "tier7_quest_overlay_displayed";

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
		public const string TIER7_OVERLAY = "tier7_overlay";
		public const string TIER7_OVERLAY_PRELOBBY = "tier7_prelobby_overlay";
		public const string TIER7_OVERLAY_HEROPICKING = "tier7_hero_overlay";
		public const string TIER7_OVERLAY_QUESTPICKING = "tier7_quest_overlay";
		public const string TIER7_OVERLAY_QUESTPICKING_COMPOSITIONS = "tier7_quest_overlay_compositions";

		// Mercenaries Settings
		public const string MERC_GENERAL_SETTINGS_ENABLED = "hdt_mercenaries_settings_enabled";
		public const string MERC_GENERAL_SETTINGS_DISABLED = "hdt_mercenaries_settings_disabled";
		public const string OPPONENT_MERC_ABILITIES_ON_HOVER = "opponent_merc_abilities_on_hover";
		public const string PLAYER_MERC_ABILITIES_ON_HOVER = "player_merc_abilities_on_hover";
		public const string ABILITY_ICONS_ABOVE_OPPONENT_MERCS = "ability_icons_above_opponent_mercs";
		public const string ABILITY_ICONS_BELOW_PLAYER_MERCS = "ability_icons_below_player_mercs";
		public const string TASKS_PANEL = "tasks_panel";

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
					{ TIER7_OVERLAY, Config.Instance.EnableBattlegroundsTier7Overlay },
					{ TIER7_OVERLAY_PRELOBBY, Config.Instance.ShowBattlegroundsTier7PreLobby },
					{ TIER7_OVERLAY_HEROPICKING, Config.Instance.ShowBattlegroundsHeroPicking },
					{ TIER7_OVERLAY_QUESTPICKING, Config.Instance.ShowBattlegroundsQuestPicking },
					{ TIER7_OVERLAY_QUESTPICKING_COMPOSITIONS, Config.Instance.ShowBattlegroundsQuestPickingComps },
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
