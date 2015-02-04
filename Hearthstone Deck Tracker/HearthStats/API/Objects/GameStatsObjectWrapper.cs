using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class GameStatsObjectWrapper
	{
		public GameStatsObject match { get; set; }
		public string deck_id { get; set; }
		public string version_id { get; set; }
		public int ranklvl { get; set; }

		public GameStats ToGameStats()
		{
			return match.ToGameStats(version_id, deck_id, ranklvl);
		}
	}
}