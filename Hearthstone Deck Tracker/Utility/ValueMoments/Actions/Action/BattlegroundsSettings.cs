using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class BattlegroundsSettings
	{

		[JsonProperty("bg_tiers")]
		public bool Tiers { get => Config.Instance.ShowBattlegroundsTiers; }

		[JsonProperty("bg_turn_counter")]
		public bool TurnCounter { get => Config.Instance.ShowBattlegroundsTurnCounter; }

		[JsonProperty("bb_combat_simulations")]
		public bool BobsBuddyCombatSimulations { get => Config.Instance.RunBobsBuddy; }

		[JsonProperty("bb_results_during_combat")]
		public bool BobsBuddyResultsDuringCombat { get => Config.Instance.ShowBobsBuddyDuringCombat; }

		[JsonProperty("bb_results_during_shopping")]
		public bool BobsBuddyResultsDuringShopping { get => Config.Instance.ShowBobsBuddyDuringShopping; }

		[JsonProperty("bb_always_show_average_damage")]
		public bool BobsBuddyAlwaysShowAverageDamage { get => Config.Instance.AlwaysShowAverageDamage; }

		[JsonProperty("session_recap")]
		public bool SessionRecap { get => Config.Instance.ShowSessionRecap; }

		[JsonProperty("session_recap_between_games")]
		public bool SessionRecapBetweenGames { get => Config.Instance.ShowSessionRecapBetweenGames; }

		[JsonProperty("minions_available")]
		public bool MinionsBanned { get => Config.Instance.ShowSessionRecapMinionsAvailable; }
		
		[JsonProperty("minions_banned")]
		public bool MinionsBanned { get => Config.Instance.ShowSessionRecapMinionsBanned; }

		[JsonProperty("start_and_current_mmr")]
		public bool StartAndCurrentMMR { get => Config.Instance.ShowSessionRecapStartCurrentMMR; }

		[JsonProperty("latest_10_game")]
		public bool Latest10Game { get => Config.Instance.ShowSessionRecapLatestGames; }

		[JsonProperty("tier7_overlay")]
		public bool Tier7Overlay { get => Config.Instance.EnableBattlegroundsTier7Overlay; }

		[JsonProperty("tier7_prelobby_overlay")]
		public bool Tier7PrelobbyOverlay { get => Config.Instance.ShowBattlegroundsTier7PreLobby; }

		[JsonProperty("tier7_prelobby_overlay_collapsed")]
		public bool Tier7PrelobbyOverlayCollapsed { get => Config.Instance.Tier7OverlayCollapsed; }

		[JsonProperty("tier7_hero_overlay")]
		public bool Tier7HeroOverlay { get => Config.Instance.ShowBattlegroundsHeroPicking; }

		[JsonProperty("tier7_quest_overlay")]
		public bool Tier7QuestOverlay { get => Config.Instance.ShowBattlegroundsQuestPicking; }

		[JsonProperty("tier7_quest_overlay_compositions")]
		public bool Tier7QuestOverlayCompositions { get => Config.Instance.ShowBattlegroundsQuestPickingComps; }
	}
}
