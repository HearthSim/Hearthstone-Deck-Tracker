using System;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan
{
	public class ConstructedMulliganSingleCardHeaderViewModel : StatsHeaderViewModel
	{
		public ConstructedMulliganSingleCardHeaderViewModel(int? rank, double? mulliganWr, double? keepRate, int? maxRank, double? baseWinrate) : base(rank, mulliganWr, keepRate, maxRank, baseWinrate)
		{
		}
	}
}
