using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking
{
	public partial class BattlegroundsSingleQuestViewModel : StatsHeaderViewModel
	{
		public BattlegroundsSingleQuestViewModel(BattlegroundsQuestPickStats? stats) : base(stats?.TierV2, stats?.AvgFinalPlacement, stats?.FpPickRate)
		{
		}
	}
}
