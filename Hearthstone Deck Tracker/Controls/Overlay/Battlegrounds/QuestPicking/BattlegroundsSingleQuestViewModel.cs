using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking
{
	public partial class BattlegroundsSingleQuestViewModel : StatsHeaderViewModel
	{
		public BattlegroundsCompositionPopularityViewModel? CompVM { get; }

		public BattlegroundsSingleQuestViewModel(BattlegroundsQuestPickStats? stats) : base(stats?.TierV2, stats?.AvgFinalPlacement, stats?.FpPickRate)
		{
			if(stats != null)
				CompVM = new(stats.FirstPlaceComps);
		}
	}
}
