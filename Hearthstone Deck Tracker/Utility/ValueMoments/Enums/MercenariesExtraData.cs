using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum MercenariesExtraData
	{
		[MixpanelProperty("match_result")]
		MatchResult,

		[MixpanelProperty("game_type")]
		GameType,

		[MixpanelProperty("num_hover_opponent_merc_ability")]
		NumHoverOpponentMercAbility,

		[MixpanelProperty("num_hover_merc_task_overlay")]
		NumHoverMercTaskOverlay,
	}
}
