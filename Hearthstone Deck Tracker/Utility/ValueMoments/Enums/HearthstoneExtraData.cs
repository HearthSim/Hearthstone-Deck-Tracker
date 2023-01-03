using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum HearthstoneExtraData
	{
		[MixpanelProperty("hero_dbf_id")]
		HeroDbfId,

		[MixpanelProperty("hero_name")]
		HeroName,

		[MixpanelProperty("sub_franchise")]
		SubFranchise,

		[MixpanelProperty("match_result")]
		MatchResult,

		[MixpanelProperty("game_type")]
		GameType,

		[MixpanelProperty("star_level")]
		StarLevel,
	}
}
