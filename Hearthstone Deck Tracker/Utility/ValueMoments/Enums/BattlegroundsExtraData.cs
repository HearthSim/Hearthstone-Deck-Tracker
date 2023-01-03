using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum BattlegroundsExtraData
	{
		[MixpanelProperty("hero_dbf_id")]
		HeroDbfId,

		[MixpanelProperty("hero_name")]
		HeroName,

		[MixpanelProperty("final_placement")]
		FinalPlacement,

		[MixpanelProperty("game_type")]
		GameType,

		[MixpanelProperty("battlegrounds_rating")]
		BattlegroundsRating,

		[MixpanelProperty("trials_activated")]
		TrialsActivated,

		[MixpanelProperty("trials_remaining")]
		TrialsRemaining,

		[MixpanelProperty("tier7_hero_overlay_displayed")]
		Tier7HeroOverlayDisplayed,

		[MixpanelProperty("tier7_quest_overlay_displayed")]
		Tier7QuestOverlayDisplayed,

		[MixpanelProperty("num_click_battlegrounds_minion_tab")]
		NumClickBattlegroundsMinionTab,
	}
}
