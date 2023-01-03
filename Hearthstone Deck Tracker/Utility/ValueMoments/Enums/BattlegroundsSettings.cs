
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum BattlegroundsSettings
	{
		[MixpanelProperty("bg_tiers")]
		Tiers,

		[MixpanelProperty("bg_turn_counter")]
		TurnCounter,

		[MixpanelProperty("bb_combat_simulations")]
		BobsBuddyCombatSimulations,

		[MixpanelProperty("bb_results_during_combat")]
		BobsBuddyResultsDuringCombat,

		[MixpanelProperty("bb_results_during_shopping")]
		BobsBuddyResultsDuringShopping,

		[MixpanelProperty("bb_always_show_average_damage")]
		BobsBuddyAlwaysShowAverageDamage,

		[MixpanelProperty("session_recap")]
		SessionRecap,

		[MixpanelProperty("session_recap_between_games")]
		SessionRecapBetweenGames,

		[MixpanelProperty("minions_banned")]
		MinionsBanned,

		[MixpanelProperty("start_and_current_mmr")]
		StartAndCurrentMMR,

		[MixpanelProperty("latest_10_game")]
		Latest10Game,

		[MixpanelProperty("tier7_overlay")]
		Tier7Overlay,

		[MixpanelProperty("tier7_prelobby_overlay")]
		Tier7PrelobbyOverlay,

		[MixpanelProperty("tier7_hero_overlay")]
		Tier7HeroOverlay,

		[MixpanelProperty("tier7_quest_overlay")]
		Tier7QuestOverlay,

		[MixpanelProperty("tier7_quest_overlay_compositions")]
		Tier7QuestOverlayCompositions,
	}
}
