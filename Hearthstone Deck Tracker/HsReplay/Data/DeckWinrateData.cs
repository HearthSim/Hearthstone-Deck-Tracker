using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	public class DeckWinrateData : HsReplayData
	{
		public double TotalWinrate { get; set; }
		public Dictionary<string, double> ClassWinrates { get; set; }
	}
}
