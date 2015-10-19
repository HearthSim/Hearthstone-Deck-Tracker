using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class CompiledStats : INotifyPropertyChanged
	{
		private static readonly CompiledStats _instance = new CompiledStats();

		public static CompiledStats Instance
		{
			get { return _instance; }
		}

		private IEnumerable<Deck> ArenaDecks
		{
			get { return DeckList.Instance.Decks.Where(x => x != null && x.IsArenaDeck); }
		}

		public IEnumerable<ArenaRun> ArenaRuns
		{
			get { return ArenaDecks.Select(x => new ArenaRun(x)).OrderByDescending(x => x.StartTime); }
		}

		public IEnumerable<ClassStats> ArenaClasses
		{
			get { return GetFilteredArenaRuns(classFilter: false).GroupBy(x => x.Class).Select(x => new ClassStats(x.Key, x)).OrderBy(x => x.Class); }
		}

		public ClassStats ArenaClassStatsBest
		{
			get { return !ArenaClasses.Any() ? null :  ArenaClasses.OrderByDescending(x => x.WinRate).First(); }
		}

		public ClassStats ArenaClassStatsWorst
		{
			get { return !ArenaClasses.Any() ? null : ArenaClasses.OrderBy(x => x.WinRate).First(); }
		}

		public ClassStats ArenaClassStatsMostPicked
		{
			get { return !ArenaClasses.Any() ? null : ArenaClasses.OrderByDescending(x => x.Runs).First(); }
		}

		public ClassStats ArenaClassStatsLeastPicked
		{
			get { return !ArenaClasses.Any() ? null : ArenaClasses.OrderBy(x => x.Runs).First(); }
		}

		public ClassStats ArenaClassStatsDruid
		{
			get { return GetClassStats("Druid"); }
		}
		public ClassStats ArenaClassStatsHunter
		{
			get { return GetClassStats("Hunter"); }
		}
		public ClassStats ArenaClassStatsMage
		{
			get { return GetClassStats("Mage"); }
		}
		public ClassStats ArenaClassStatsPaladin
		{
			get { return GetClassStats("Paladin"); }
		}
		public ClassStats ArenaClassStatsPriest
		{
			get { return GetClassStats("Priest"); }
		}
		public ClassStats ArenaClassStatsRogue
		{
			get { return GetClassStats("Rogue"); }
		}
		public ClassStats ArenaClassStatsShaman
		{
			get { return GetClassStats("Shaman"); }
		}
		public ClassStats ArenaClassStatsWarlock
		{
			get { return GetClassStats("Warlock"); }
		}
		public ClassStats ArenaClassStatsWarrior
		{
			get { return GetClassStats("Warrior"); }
		}

		public ClassStats GetClassStats(string @class)
		{
			var runs = GetFilteredArenaRuns(classFilter: false).Where(x => x.Class == @class).ToList();
			if(!runs.Any())
				return null;
            return new ClassStats(@class, runs);
		}

		public ClassStats ArenaAllClasses
		{
			get { return GetFilteredArenaRuns(classFilter: false).GroupBy(x => true).Select(x => new ClassStats("All", x)).FirstOrDefault(); }
		}

		public int ArenaRunsCount
		{
			get { return GetFilteredArenaRuns().Count(); }
		}

		public int ArenaGamesCountTotal
		{
			get { return GetFilteredArenaRuns().Sum(x => x.Games.Count()); }
		}

		public int ArenaGamesCountWon
		{
			get { return GetFilteredArenaRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Win)); }
		}

		public int ArenaGamesCountLost
		{
			get { return GetFilteredArenaRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Loss)); }
		}

		public double AverageWinsPerRun
		{
			get { return (double)ArenaGamesCountWon / GetFilteredArenaRuns().Count(); }
		}

		public IEnumerable<ArenaRun> GetFilteredArenaRuns(bool archivedFilter = true, bool classFilter = true, bool regionFilter = true,
		                                          bool timeframeFilter = true)
		{

			var filtered = ArenaRuns;
			if(archivedFilter && !Config.Instance.ArenaStatsIncludeArchived)
			{
				filtered = filtered.Where(x => !x.Deck.Archived);
			}
			if(classFilter && Config.Instance.ArenaStatsClassFilter != HeroClassStatsFilter.All)
			{
				filtered = filtered.Where(x => x.Class == Config.Instance.ArenaStatsClassFilter.ToString());
			}
			if(regionFilter && Config.Instance.ArenaStatsRegionFilter != RegionAll.ALL)
			{
				var region = (Region)Enum.Parse(typeof(Region), Config.Instance.ArenaStatsRegionFilter.ToString());
				filtered = filtered.Where(x => x.Games.Any(g => g.Region == region));
			}
			if(timeframeFilter)
			{
				switch(Config.Instance.ArenaStatsTimeFrameFilter)
				{
					case DisplayedTimeFrame.AllTime:
						break;
					case DisplayedTimeFrame.CurrentSeason:
						filtered = filtered.Where(g => g.StartTime > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
						break;
					case DisplayedTimeFrame.ThisWeek:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1)));
						break;
					case DisplayedTimeFrame.Today:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today);
						break;
					case DisplayedTimeFrame.Custom:
						var start = (Config.Instance.ArenaStatsTimeFrameCustomStart ?? DateTime.MinValue).Date;
						var end = (Config.Instance.ArenaStatsTimeFrameCustomEnd ?? DateTime.MaxValue).Date;
						filtered = filtered.Where(g => g.EndTime.Date >= start && g.EndTime.Date <= end);
						break;
				}
			}
			return filtered;
		} 

		public IEnumerable<ArenaRun> FilteredArenaRuns
		{
			get { return GetFilteredArenaRuns(); }
		}

		public IEnumerable<ChartStats> ArenaPlayedClassesPercent
		{
			get
			{
				return
					GetFilteredArenaRuns().GroupBy(x => x.Class)
					                 .OrderBy(x => x.Key)
					                 .Select(
					                         x =>
					                         new ChartStats
					                         {
						                         Name = x.Key + " (" + Math.Round(100.0 * x.Count() / ArenaDecks.Count()) + "%)",
						                         Value = x.Count(),
						                         Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
					                         });
			}
		}

		public IEnumerable<ChartStats> ArenaOpponentClassesPercent
		{
			get
			{
				var opponents = GetFilteredArenaRuns().SelectMany(x => x.Deck.DeckStats.Games.Select(g => g.OpponentHero)).ToList();
				return
					opponents.GroupBy(x => x)
					         .OrderBy(x => x.Key)
					         .Select(
					                 g =>
					                 new ChartStats
					                 {
						                 Name = g.Key + " (" + Math.Round(100.0 * g.Count() / opponents.Count()) + "%)",
						                 Value = g.Count(),
						                 Brush = new SolidColorBrush(Helper.GetClassColor(g.Key, true))
					                 });
			}
		}


		public IEnumerable<ChartStats> ArenaWins
		{
			get
			{
				var groupedByWins =
					GetFilteredArenaRuns().GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
					                 .Select(x => new {Wins = x.Key, Count = x.Count(), Runs = x})
					                 .ToList();
				return Enumerable.Range(0, 13).Select(n =>
				{
					var runs = groupedByWins.FirstOrDefault(x => x.Wins == n);
					if(runs == null)
						return new ChartStats {Name = n + " wins", Value = 0};
					return new ChartStats {Name = n + " wins", Value = runs.Count};
				});
			}
		}

		public IEnumerable<ChartStats>[] ArenaWinsByClass
		{
			get
			{
				var groupedByWins =
					GetFilteredArenaRuns().GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
					                 .Select(x => new {Wins = x.Key, Count = x.Count(), Runs = x})
					                 .ToList();
				return Enumerable.Range(0, 13).Select(n =>
				{
					var runs = groupedByWins.FirstOrDefault(x => x.Wins == n);
					if(runs == null)
						return new[] {new ChartStats {Name = n.ToString(), Value = 0, Brush = new SolidColorBrush()}};
					return
						runs.Runs.GroupBy(x => x.Class)
						    .OrderBy(x => x.Key)
						    .Select(
						            x =>
						            new ChartStats
						            {
							            Name = n + " wins (" + x.Key + ")",
							            Value = x.Count(),
							            Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
						            });
				}).ToArray();
			}
		}

		public IEnumerable<ChartStats>[] ArenaWinLossByClass
		{
			get
			{
				var gamesGroupedByOppHero = GetFilteredArenaRuns().SelectMany(x => x.Deck.DeckStats.Games).GroupBy(x => x.OpponentHero);
				return Enum.GetNames(typeof(HeroClass)).Select(x =>
				{
					var classGames = gamesGroupedByOppHero.FirstOrDefault(g => g.Key == x);
					if(classGames == null)
						return new[] {new ChartStats {Name = x, Value = 0, Brush = new SolidColorBrush()}};
					return classGames.GroupBy(g => g.Result).OrderBy(g => g.Key).Select(g =>
					{
						var color = Helper.GetClassColor(x, true);
						if(g.Key == GameResult.Loss)
							color = Color.FromRgb((byte)(color.R * 0.7), (byte)(color.G * 0.7), (byte)(color.B * 0.7));
						return new ChartStats {Name = g.Key.ToString() + " vs " + x.ToString(), Value = g.Count(), Brush = new SolidColorBrush(color)};
					});

				}).ToArray();
			}
		}

		public IEnumerable<ChartStats> AvgWinsPerClass
		{
			get
			{
				return
					GetFilteredArenaRuns().GroupBy(x => x.Class)
					                 .Select(
					                         x =>
					                         new ChartStats
					                         {
						                         Name = x.Key,
						                         Value =
							                         Math.Round(
							                                    (double)x.Sum(d => d.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
							                                    / x.Count(), 1),
						                         Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
					                         })
					                 .OrderBy(x => x.Value);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}


		public void OnPropertyChanged(string[] properties)
		{
			foreach(var prop in properties)
				OnPropertyChanged(prop);
		}

		public void UpdateArenaStats()
		{
			OnPropertyChanged("ArenaRuns");
			OnPropertyChanged("ArenaOpponentClassesPercent");
			OnPropertyChanged("ArenaPlayedClassesPercent");
			OnPropertyChanged("ArenaWins");
			OnPropertyChanged("AvgWinsPerClass");
			OnPropertyChanged("FilteredArenaRuns");
		}

		public void UpdateArenaStatsHighlights()
		{
			OnPropertyChanged("ArenaClasses");
			OnPropertyChanged("ArenaClassStatsDruid");
			OnPropertyChanged("ArenaClassStatsHunter");
			OnPropertyChanged("ArenaClassStatsMage");
			OnPropertyChanged("ArenaClassStatsPaladin");
			OnPropertyChanged("ArenaClassStatsPriest");
			OnPropertyChanged("ArenaClassStatsRogue");
			OnPropertyChanged("ArenaClassStatsShaman");
			OnPropertyChanged("ArenaClassStatsWarlock");
			OnPropertyChanged("ArenaClassStatsWarrior");
			OnPropertyChanged("ArenaAllClasses");
			OnPropertyChanged("ArenaClassStatsBest");
			OnPropertyChanged("ArenaClassStatsWorst");
			OnPropertyChanged("ArenaClassStatsMostPicked");
			OnPropertyChanged("ArenaClassStatsLeastPicked");
		}

		public void UpdateArenaRuns()
		{
			OnPropertyChanged("ArenaRuns");
			OnPropertyChanged("FilteredArenaRuns");
		}

		public void UpdateExpensiveArenaStats()
		{
			OnPropertyChanged("ArenaWinLossByClass");
			OnPropertyChanged("ArenaWinsByClass");
		}
	}

	public class ChartStats
	{
		public string Name { get; set; }
		public double Value { get; set; }
		public Brush Brush { get; set; }
	}

	public class ArenaRun
	{
		private readonly Deck _deck;

		public Deck Deck
		{
			get { return _deck; }
		}

		public ArenaRun(Deck deck)
		{
			_deck = deck;
		}

		public string Class
		{
			get { return _deck.Class; }
		}

		public BitmapImage ClassImage
		{
			get { return _deck.ClassImage; }
		}

		public string StartTimeString
		{
			get { return StartTime == DateTime.MinValue ? "-" : StartTime.ToString("dd.MMM HH:mm"); }
		}

		public DateTime StartTime
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Min(g => g.StartTime) : DateTime.MinValue; }
		}

		public DateTime EndTime
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Max(g => g.EndTime) : DateTime.MinValue; }
		}

		public int Wins
		{
			get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Win); }
		}

		public int Losses
		{
			get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Loss); }
		}

		public int Gold
		{
			get { return _deck.ArenaReward.Gold; }
		}

		public int Dust
		{
			get { return _deck.ArenaReward.Dust; }
		}

		public int PackCount
		{
			get { return _deck.ArenaReward.Packs.Count(x => !string.IsNullOrEmpty(x)); }
		}

		public string PackString
		{
			get
			{
				var packs = _deck.ArenaReward.Packs.Where(x => !string.IsNullOrEmpty(x)).ToList();
				return packs.Any() ? packs.Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int CardCount
		{
			get { return _deck.ArenaReward.Cards.Count(x => x != null && !string.IsNullOrEmpty(x.CardId)); }
		}

		public string CardString
		{
			get
			{
				var cards = _deck.ArenaReward.Cards.Where(x => x != null && !string.IsNullOrEmpty(x.CardId)).ToList();
				return cards.Any()
					       ? cards.Select(x => (Database.GetCardFromId(x.CardId).LocalizedName) + (x.Golden ? " (golden)" : ""))
					              .Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int Duration
		{
			get { return _deck.DeckStats.Games.Sum(x => x.SortableDuration); }
		}

		public string DurationString
		{
			get { return Duration + " min"; }
		}

		public string Region
		{
			get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.First().Region.ToString() : "UNKNOWN"; }
		}

		public IEnumerable<GameStats> Games
		{
			get { return _deck.DeckStats.Games; }
		}
	}

	public class ClassStats
	{
		public string Class { get; set; }

		public IEnumerable<ArenaRun> ArenaRuns { get; set; }

		public IEnumerable<MatchupStats> Matchups { get { return ArenaRuns.SelectMany(r => r.Games).GroupBy(x => x.OpponentHero).Select(x => new MatchupStats(x.Key, x)); } }

		public MatchupStats BestMatchup { get { return Matchups.OrderByDescending(x => x.WinRate).FirstOrDefault(); } }

		public MatchupStats WorstMatchup { get { return Matchups.OrderBy(x => x.WinRate).FirstOrDefault(); } }

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
			get { return Math.Round(100.0 * Runs / CompiledStats.Instance.GetFilteredArenaRuns(classFilter: false).Count()); }
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
				if(double.IsNaN(WinRate))
					return new SolidColorBrush(Config.Instance.StatsInWindow ? Colors.Black : Colors.White);
				return new SolidColorBrush(WinRate >= 0.5 ? Colors.Green : Colors.Red);
			}
		}

		public SolidColorBrush BestRunTextBrush
		{
			get
			{
				if(BestRun == null)
					return new SolidColorBrush(Config.Instance.StatsInWindow ? Colors.Black : Colors.White);
				return new SolidColorBrush(BestRun.Wins >= 3 ? Colors.Green : Colors.Red);
			}
		}

		public ClassStats(string @class, IEnumerable<ArenaRun> arenaRuns)
		{
			Class = @class;
			ArenaRuns = arenaRuns;
		}

		public class MatchupStats
		{
			public IEnumerable<GameStats> Games { get; set; }

			public MatchupStats(string @class, IEnumerable<GameStats> games)
			{
				Class = @class;
				Games = games;
			}

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
					if(double.IsNaN(WinRate))
						return new SolidColorBrush(Config.Instance.StatsInWindow ? Colors.Black : Colors.White);
					return new SolidColorBrush(WinRate >= 0.5 ? Colors.Green : Colors.Red);
				}
			}
		}
	}
}
