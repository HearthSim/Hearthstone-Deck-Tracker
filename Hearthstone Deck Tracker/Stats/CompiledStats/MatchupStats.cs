#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class MatchupStats
	{
		public MatchupStats(string @class, IEnumerable<GameStats> games)
		{
			Class = @class;
			Games = games;
		}

		public IEnumerable<GameStats> Games { get; set; }

		public int Wins
		{
			get { return Games.Count(x => x.Result == GameResult.Win); }
		}

		public int Losses
		{
			get { return Games.Count(x => x.Result == GameResult.Loss); }
		}

		public double WinRate
		{
			get { return (double)Wins / (Wins + Losses); }
		}

		public string Class { get; set; }

		public double WinRatePercent
		{
			get { return Math.Round(WinRate * 100); }
		}

		public SolidColorBrush WinRateTextBrush
		{
			get
			{
				if(double.IsNaN(WinRate) || !Config.Instance.ArenaStatsTextColoring)
					return new SolidColorBrush(Config.Instance.StatsInWindow ? Colors.Black : Colors.White);
				return new SolidColorBrush(WinRate >= 0.5 ? Colors.Green : Colors.Red);
			}
		}
	}
}