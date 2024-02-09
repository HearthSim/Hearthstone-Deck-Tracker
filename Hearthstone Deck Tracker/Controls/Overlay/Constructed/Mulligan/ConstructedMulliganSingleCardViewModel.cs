using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public class SingleCardStats : MulliganGuideData.CardStats
{
	public SingleCardStats(int dbfId)
	{
		DbfId = dbfId;
	}

	public int? Rank { get; set; }

	public double? BaseWinrate { get; set; }

	public static Dictionary<int, SingleCardStats> GroupCardStats(Dictionary<int, MulliganGuideData.CardStats> stats, double? baseWinrate)
	{
		var retval = new Dictionary<int, SingleCardStats>();

		retval = stats
			.OrderByDescending(x => x.Value.OpeningHandWinrate)
			.Select(
				(x, i) =>
				{
					var stats = x.Value;
					return new SingleCardStats(x.Key)
					{
						OpeningHandWinrate = stats.OpeningHandWinrate is float ohw ? Math.Max(Math.Min(ohw, 100.0f), 0.0f) : null,
						KeepPercentage = stats.KeepPercentage is float kp ? Math.Max(Math.Min(kp, 100.0f), 0.0f) : null,
						Rank = stats.KeepPercentage != null ? i + 1 : null,
						BaseWinrate = baseWinrate
					};
				}
			).ToDictionary(x => x.DbfId, x => x);

		return retval;
	}
}

public class ConstructedMulliganSingleCardViewModel : ViewModel
{
	public ConstructedMulliganSingleCardHeaderViewModel CardHeaderVM { get;  }

	public int? DbfId { get; }

	public ConstructedMulliganSingleCardViewModel(SingleCardStats? stats, int? maxRank)
	{
		DbfId = stats?.DbfId;
		CardHeaderVM = new(stats?.Rank, stats?.OpeningHandWinrate, stats?.KeepPercentage, maxRank, stats?.BaseWinrate);
	}
}
