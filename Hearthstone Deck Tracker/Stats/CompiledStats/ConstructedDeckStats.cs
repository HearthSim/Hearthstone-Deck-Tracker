using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedDeckStats
	{
		private readonly Deck _deck;
		private readonly IEnumerable<GameStats> _games;

		public ConstructedDeckStats(IGrouping<Guid, GameStats> grouping)
		{
			if(grouping != null)
				_deck = DeckList.Instance.Decks.FirstOrDefault(x => x.DeckId == grouping.Key);
			_games = grouping;
		}

		public ConstructedDeckStats(IEnumerable<GameStats> games)
		{
			_games = games;
		}

		public string DeckName => _deck?.Name ?? _games?.FirstOrDefault(x => !string.IsNullOrEmpty(x.DeckName))?.DeckName;

		public string Class => _deck?.Class ?? _games?.FirstOrDefault()?.PlayerHero;

		public IEnumerable<MatchupStats> Matchups => _games?.GroupBy(x => x.OpponentHero).Select(x => new MatchupStats(x.Key, x));

		public MatchupStats BestMatchup => Matchups.OrderByDescending(x => x.WinRate).ThenBy(x => x.Losses).ThenByDescending(x => x.Wins).FirstOrDefault();

		public MatchupStats WorstMatchup => Matchups.OrderBy(x => x.WinRate).ThenByDescending(x => x.Losses).ThenBy(x => x.Wins).FirstOrDefault();

		public int Wins => _games?.Count(x => x.Result == GameResult.Win) ?? 0;

		public int Losses => _games?.Count(x => x.Result == GameResult.Loss) ?? 0;

		public double WinRate => (double)Wins / (Wins + Losses);

		public double WinRatePercent => Math.Round(WinRate * 100);

		public BitmapImage ClassImage => ImageCache.GetClassIcon(Class ?? "");

		public TimeSpan TotalDuration => TimeSpan.FromMinutes(_games?.Sum(x => (x.EndTime - x.StartTime).Minutes) ?? 0);

		public TimeSpan AverageDuration => TimeSpan.FromMinutes(_games?.Where(x => x.EndTime > x.StartTime).Select(x => (x.EndTime - x.StartTime).Minutes).DefaultIfEmpty(0).Average() ?? 0);

		public double AverageTurns => Math.Round(_games?.Where(x => x.Turns > 0).Select(x => x.Turns).DefaultIfEmpty(0).Average() ?? 0, 1);

		public SolidColorBrush WinRateTextBrush
		{
			get
			{
				if(double.IsNaN(WinRate) || !Config.Instance.ArenaStatsTextColoring)
					return new SolidColorBrush(Config.Instance.StatsInWindow && Config.Instance.AppTheme != MetroTheme.BaseDark ? Colors.Black : Colors.White);
				return new SolidColorBrush(WinRate >= 0.5 ? Colors.Green : Colors.Red);
			}
		}
	}
}