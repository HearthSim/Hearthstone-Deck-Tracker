
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum MercenariesSettings
	{
		[MixpanelProperty("opponent_merc_abilities_on_hover")]
		OpponentMercAbilitiesOnHover,

		[MixpanelProperty("player_merc_abilities_on_hover")]
		PlayerMercAbilitiesOnHover,

		[MixpanelProperty("ability_icons_above_opponent_mercs")]
		AbilityIconsAboveOpponentMercs,

		[MixpanelProperty("ability_icons_below_player_mercs")]
		AbilityIconsBelowPlayerMercs,

		[MixpanelProperty("tasks_panel")]
		TasksPanel,
	}
}
