using Hearthstone_Deck_Tracker.Utility;
using System;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class RankTranslator
	{
		private static readonly string[] _leagues = new[] { "Rank_League_Bronze", "Rank_League_Silver", "Rank_League_Gold", "Rank_League_Platinum", "Rank_League_Diamond" };

		public static string GetRankString(int leagueId, int starLevel, int rank, int legendRank)
		{
			if(legendRank > 0)
				return string.Format(LocUtil.Get("Rank_Legend"), legendRank);
			if(leagueId == 5)
			{
				if(starLevel == 0)
					return "-";
				starLevel = Math.Max(1, Math.Min(50, starLevel));
				var league = _leagues[(starLevel - 1) / 10];
				var leagueRank = 10 - ((starLevel - 1) % 10);
				return string.Format(LocUtil.Get(league), leagueRank);
			}
			if(rank == 0)
				return "-";
			return rank.ToString();
		}
	}
}
