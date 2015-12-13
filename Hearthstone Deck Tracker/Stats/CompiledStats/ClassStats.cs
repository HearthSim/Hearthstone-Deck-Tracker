#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ClassStats
	{
		public ClassStats(string @class, IEnumerable<ArenaRun> arenaRuns)
		{
			Class = @class;
			ArenaRuns = arenaRuns;
		}

		public string Class { get; set; }
		public IEnumerable<ArenaRun> ArenaRuns { get; set; }

		public IEnumerable<MatchupStats> Matchups
		{
			get { return ArenaRuns.SelectMany(r => r.Games).GroupBy(x => x.OpponentHero).Select(x => new MatchupStats(x.Key, x)); }
		}

		public MatchupStats BestMatchup
		{
			get { return Matchups.OrderByDescending(x => x.WinRate).FirstOrDefault(); }
		}

		public MatchupStats WorstMatchup
		{
			get { return Matchups.OrderBy(x => x.WinRate).FirstOrDefault(); }
		}

		public int Runs
		{
			get { return ArenaRuns.Count(); }
		}

		public ArenaRun BestRun
		{
			get { return ArenaRuns.OrderByDescending(x => x.Wins).ThenBy(x => x.Losses).FirstOrDefault(); }
		}

		public int Games
		{
			get { return ArenaRuns.Sum(runs => runs.Games.Count()); }
		}

		public int Wins
		{
			get { return ArenaRuns.Sum(runs => runs.Wins); }
		}

		public int Losses
		{
			get { return ArenaRuns.Sum(runs => runs.Losses); }
		}

		public double AverageWins
		{
			get { return Math.Round((double)Wins / Runs, 1); }
		}

		public double WinRate
		{
			get { return (double)Wins / (Wins + Losses); }
		}

		public double WinRatePercent
		{
			get { return Math.Round(WinRate * 100); }
		}

		public double PickedPercent
		{
			get { return Math.Round(100.0 * Runs / ArenaStats.Instance.GetFilteredRuns(classFilter: false).Count()); }
		}

		public BitmapImage ClassImage
		{
			get { return ImageCache.GetClassIcon(Class); }
		}

		public TimeSpan Duration
		{
			get { return TimeSpan.FromMinutes(ArenaRuns.Sum(x => x.Duration)); }
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

		public SolidColorBrush BestRunTextBrush
		{
			get
			{
				if(BestRun == null || !Config.Instance.ArenaStatsTextColoring)
					return new SolidColorBrush(Config.Instance.StatsInWindow ? Colors.Black : Colors.White);
				return new SolidColorBrush(BestRun.Wins >= 3 ? Colors.Green : Colors.Red);
			}
		}
	}
}