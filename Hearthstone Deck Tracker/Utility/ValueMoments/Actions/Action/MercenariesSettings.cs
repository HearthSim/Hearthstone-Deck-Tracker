using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public class MercenariesSettings
	{

		[JsonProperty("opponent_merc_abilities_on_hover")]
		public bool OpponentMercAbilitiesOnHover { get => Config.Instance.ShowMercsOpponentHover; }

		[JsonProperty("player_merc_abilities_on_hover")]
		public bool PlayerMercAbilitiesOnHover { get => Config.Instance.ShowMercsPlayerHover; }

		[JsonProperty("ability_icons_above_opponent_mercs")]
		public bool AbilityIconsAboveOpponentMercs { get => Config.Instance.ShowMercsOpponentAbilityIcons; }

		[JsonProperty("ability_icons_below_player_mercs")]
		public bool AbilityIconsBelowPlayerMercs { get => Config.Instance.ShowMercsPlayerAbilityIcons; }

		[JsonProperty("tasks_panel")]
		public bool TasksPanel { get => Config.Instance.ShowMercsTasks; }

	}
}
