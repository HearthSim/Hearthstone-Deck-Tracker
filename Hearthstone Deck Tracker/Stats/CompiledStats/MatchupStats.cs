#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class MatchupStats
	{
		public MatchupStats(string @class, IEnumerable<GameStats> games):this(ConvertToEnum(@class), games)
		{
			
        }

		public MatchupStats(HeroClassAll @class, IEnumerable<GameStats> games)
		{
			Class = EnumDescriptionConverter.GetDescription(@class);
			Games = games;
		}

		private static HeroClassAll ConvertToEnum(string @class)
		{
			@class = @class == "Total" ? "All" : @class;
			HeroClassAll heroClass = (HeroClassAll)Enum.Parse(typeof(HeroClassAll), @class, true);
			return heroClass;
        }

		public IEnumerable<GameStats> Games { get; set; }

		public int Wins => Games.Count(x => x.Result == GameResult.Win);

		public int Losses => Games.Count(x => x.Result == GameResult.Loss);

		public double WinRate => (double)Wins / (Wins + Losses);

		public string Class { get; set; }

		public double WinRatePercent => Math.Round(WinRate * 100);

		public SolidColorBrush WinRateTextBrush
		{
			get
			{
				if(double.IsNaN(WinRate) || !Config.Instance.ArenaStatsTextColoring)
					return new SolidColorBrush(Config.Instance.StatsInWindow && Config.Instance.AppTheme != MetroTheme.BaseDark ? Colors.Black : Colors.White);
				return new SolidColorBrush(WinRate >= 0.5 ? Colors.Green : Colors.Red);
			}
		}

		public string Summary => Config.Instance.ConstructedStatsAsPercent ? (double.IsNaN(WinRate) ? "-" : $" {WinRatePercent}%") : $"{Wins} - {Losses}";
	}
}