#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ArenaStats : INotifyPropertyChanged
	{
		private static readonly ArenaStats _instance = new ArenaStats();

		public static ArenaStats Instance
		{
			get { return _instance; }
		}

		private IEnumerable<Deck> ArenaDecks
		{
			get
			{
				if(!Core.Initialized)
					return new List<Deck>();
				return DeckList.Instance.Decks.Where(x => x != null && x.IsArenaDeck);
			}
		}

		public IEnumerable<ArenaRun> Runs
		{
			get { return ArenaDecks.Select(x => new ArenaRun(x)).OrderByDescending(x => x.StartTime); }
		}

		public IEnumerable<ClassStats> ClassStats
		{
			get
			{
				return GetFilteredRuns(classFilter: false).GroupBy(x => x.Class).Select(x => new ClassStats(x.Key, x)).OrderBy(x => x.Class);
			}
		}

		public int PacksCountClassic
		{
			get { return GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.Classic)); }
		}

		public int PacksCountGvg
		{
			get { return GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.GoblinsVsGnomes)); }
		}

		public int PacksCountTgt
		{
			get { return GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.TheGrandTournament)); }
		}

		public int PacksCountTotal
		{
			get { return GetFilteredRuns().Sum(x => x.PackCount); }
		}

		public double PacksCountAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns().Count();
				return count == 0 ? 0 : Math.Round(1.0 * PacksCountTotal / GetFilteredRuns().Count(), 2);
			}
		}

		public int GoldTotal
		{
			get { return GetFilteredRuns().Sum(x => x.Gold); }
		}

		public double GoldAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns().Count();
				return count == 0 ? 0 : Math.Round(1.0 * GoldTotal / GetFilteredRuns().Count(), 2);
			}
		}

		public int GoldSpent
		{
			get {return GetFilteredRuns().Count(x => x.Deck.ArenaReward.PaymentMethod == ArenaPaymentMethod.Gold) * 150; }
		}

		public int DustTotal
		{
			get { return GetFilteredRuns().Sum(x => x.Dust); }
		}

		public double DustAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns().Count();
				return count == 0 ? 0 : Math.Round(1.0 * DustTotal / count, 2);
			}
		}

		public int CardCountTotal
		{
			get { return GetFilteredRuns().Sum(x => x.CardCount); }
		}

		public double CardCountAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns().Count();
				return count == 0 ? 0 : Math.Round(1.0 * CardCountTotal / GetFilteredRuns().Count(), 2);
			}
		}

		public int CardCountGolden
		{
			get { return GetFilteredRuns().Sum(x => x.CardCountGolden); }
		}

		public double CardCountGoldenAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns().Count();
				return count == 0 ? 0 : Math.Round(1.0 * CardCountGolden / GetFilteredRuns().Count(), 2);
			}
		}

		public ClassStats ClassStatsBest
		{
			get { return !ClassStats.Any() ? null : ClassStats.OrderByDescending(x => x.WinRate).First(); }
		}

		public ClassStats ClassStatsWorst
		{
			get { return !ClassStats.Any() ? null : ClassStats.OrderBy(x => x.WinRate).First(); }
		}

		public ClassStats ClassStatsMostPicked
		{
			get { return !ClassStats.Any() ? null : ClassStats.OrderByDescending(x => x.Runs).First(); }
		}

		public ClassStats ClassStatsLeastPicked
		{
			get { return !ClassStats.Any() ? null : ClassStats.OrderBy(x => x.Runs).First(); }
		}

		public ClassStats ClassStatsDruid
		{
			get { return GetClassStats("Druid"); }
		}

		public ClassStats ClassStatsHunter
		{
			get { return GetClassStats("Hunter"); }
		}

		public ClassStats ClassStatsMage
		{
			get { return GetClassStats("Mage"); }
		}

		public ClassStats ClassStatsPaladin
		{
			get { return GetClassStats("Paladin"); }
		}

		public ClassStats ClassStatsPriest
		{
			get { return GetClassStats("Priest"); }
		}

		public ClassStats ClassStatsRogue
		{
			get { return GetClassStats("Rogue"); }
		}

		public ClassStats ClassStatsShaman
		{
			get { return GetClassStats("Shaman"); }
		}

		public ClassStats ClassStatsWarlock
		{
			get { return GetClassStats("Warlock"); }
		}

		public ClassStats ClassStatsWarrior
		{
			get { return GetClassStats("Warrior"); }
		}

		public ClassStats ClassStatsAll
		{
			get { return GetFilteredRuns(classFilter: false).GroupBy(x => true).Select(x => new ClassStats("All", x)).FirstOrDefault(); }
		}

		public int RunsCount
		{
			get { return GetFilteredRuns().Count(); }
		}

		public int GamesCountTotal
		{
			get { return GetFilteredRuns().Sum(x => x.Games.Count()); }
		}

		public int GamesCountWon
		{
			get { return GetFilteredRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Win)); }
		}

		public int GamesCountLost
		{
			get { return GetFilteredRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Loss)); }
		}

		public double AverageWinsPerRun
		{
			get { return (double)GamesCountWon / GetFilteredRuns().Count(); }
		}

		public IEnumerable<ArenaRun> FilteredRuns
		{
			get { return GetFilteredRuns(); }
		}

		public IEnumerable<ChartStats> PlayedClassesPercent
		{
			get
			{
				return
					GetFilteredRuns()
						.GroupBy(x => x.Class)
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

		public IEnumerable<ChartStats> OpponentClassesPercent
		{
			get
			{
				var opponents = GetFilteredRuns().SelectMany(x => x.Deck.DeckStats.Games.Select(g => g.OpponentHero)).ToList();
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

		public IEnumerable<ChartStats> Wins
		{
			get
			{
				var groupedByWins =
					GetFilteredRuns()
						.GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
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

		public IEnumerable<ChartStats>[] WinsByClass
		{
			get
			{
				var groupedByWins =
					GetFilteredRuns()
						.GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
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

		public IEnumerable<ChartStats>[] WinLossVsClass
		{
			get
			{
				var gamesGroupedByOppHero = GetFilteredRuns().SelectMany(x => x.Deck.DeckStats.Games).GroupBy(x => x.OpponentHero);
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
					GetFilteredRuns()
						.GroupBy(x => x.Class)
						.Select(
						        x =>
						        new ChartStats
						        {
							        Name = x.Key,
							        Value = Math.Round((double)x.Sum(d => d.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win)) / x.Count(), 1),
							        Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
						        })
						.OrderBy(x => x.Value);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ClassStats GetClassStats(string @class)
		{
			var runs = GetFilteredRuns(classFilter: false).Where(x => x.Class == @class).ToList();
			if(!runs.Any())
				return null;
			return new ClassStats(@class, runs);
		}

		public IEnumerable<ArenaRun> GetFilteredRuns(bool archivedFilter = true, bool classFilter = true, bool regionFilter = true,
		                                             bool timeframeFilter = true)
		{
			var filtered = Runs;
			if(archivedFilter && !Config.Instance.ArenaStatsIncludeArchived)
				filtered = filtered.Where(x => !x.Deck.Archived);
			if(classFilter && Config.Instance.ArenaStatsClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.Class == Config.Instance.ArenaStatsClassFilter.ToString());
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
			OnPropertyChanged("Runs");
			OnPropertyChanged("OpponentClassesPercent");
			OnPropertyChanged("PlayedClassesPercent");
			OnPropertyChanged("Wins");
			OnPropertyChanged("AvgWinsPerClass");
			OnPropertyChanged("FilteredRuns");
		}

		public void UpdateArenaStatsHighlights()
		{
			OnPropertyChanged("ClassStats");
			OnPropertyChanged("ClassStatsDruid");
			OnPropertyChanged("ClassStatsHunter");
			OnPropertyChanged("ClassStatsMage");
			OnPropertyChanged("ClassStatsPaladin");
			OnPropertyChanged("ClassStatsPriest");
			OnPropertyChanged("ClassStatsRogue");
			OnPropertyChanged("ClassStatsShaman");
			OnPropertyChanged("ClassStatsWarlock");
			OnPropertyChanged("ClassStatsWarrior");
			OnPropertyChanged("ClassStatsAll");
			OnPropertyChanged("ClassStatsBest");
			OnPropertyChanged("ClassStatsWorst");
			OnPropertyChanged("ClassStatsMostPicked");
			OnPropertyChanged("ClassStatsLeastPicked");
		}

		public void UpdateArenaRewards()
		{
			OnPropertyChanged("GoldTotal");
			OnPropertyChanged("GoldAveragePerRun");
			OnPropertyChanged("GoldSpent");
			OnPropertyChanged("DustTotal");
			OnPropertyChanged("DustAveragePerRun");
			OnPropertyChanged("PacksCountClassic");
			OnPropertyChanged("PacksCountGvg");
			OnPropertyChanged("PacksCountTgt");
			OnPropertyChanged("PacksCountTotal");
			OnPropertyChanged("PacksCountAveragePerRun");
			OnPropertyChanged("CardCountTotal");
			OnPropertyChanged("CardCountGolden");
			OnPropertyChanged("CardCountAveragePerRun");
			OnPropertyChanged("CardCountGoldenAveragePerRun");
		}

		public void UpdateArenaRuns()
		{
			OnPropertyChanged("Runs");
			OnPropertyChanged("FilteredRuns");
		}

		public void UpdateExpensiveArenaStats()
		{
			OnPropertyChanged("WinLossVsClass");
			OnPropertyChanged("WinsByClass");
		}
	}
}