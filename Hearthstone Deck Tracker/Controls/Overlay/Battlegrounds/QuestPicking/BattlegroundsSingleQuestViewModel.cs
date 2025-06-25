using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking
{
	public partial class BattlegroundsSingleQuestViewModel : StatsHeaderViewModel
	{
		public BattlegroundsCompositionPopularityViewModel? CompVM { get; }

		public BattlegroundsSingleQuestViewModel(BattlegroundsQuestPickStats? stats) : base(stats?.Tier, stats?.AvgFinalPlacement, stats?.FpPickRate)
		{
			if(stats != null)
				CompVM = new(stats.FirstPlaceComps);
		}

		public string TierTooltipTitle => Tier switch
		{
			// This re-uses the hero picking titles.
			(>= 1) and (<= 4) => LocUtil.Get($"BattlegroundsHeroPicking_Header_Tier{Tier}Tooltip_Title"),
			_ => "",
		};

		public string TierTooltipText => Tier switch
		{
			(>= 1) and (<= 4) => LocUtil.Get($"BattlegroundsQuestPicking_Header_Tier{Tier}Tooltip_Desc"),
			_ => "",
		};
	}
}
