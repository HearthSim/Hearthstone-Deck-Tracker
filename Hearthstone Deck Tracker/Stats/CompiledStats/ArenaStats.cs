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
		public static ArenaStats Instance { get; } = new ArenaStats();

		private IEnumerable<Deck> ArenaDecks => !Core.Initialized ? new List<Deck>() : DeckList.Instance.Decks.Where(x => x != null && x.IsArenaDeck);

		public IEnumerable<ArenaRun> Runs => ArenaDecks.Select(x => new ArenaRun(x)).OrderByDescending(x => x.StartTime);

		public IEnumerable<ClassStats> ClassStats => GetFilteredRuns(classFilter: false).GroupBy(x => x.Class).Select(x => new ClassStats(x.Key, x)).OrderBy(x => x.Class);

		public int PacksCountClassic => GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.Classic));

		public int PacksCountGvg => GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.GoblinsVsGnomes));

		public int PacksCountTgt => GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.TheGrandTournament));

		public int PacksCountWotog => GetFilteredRuns().Sum(x => x.Packs.Count(p => p == ArenaRewardPacks.WhispersOfTheOldGods));

		public int PacksCountTotal => GetFilteredRuns().Sum(x => x.PackCount);

		public double PacksCountAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns(requirePackReward: true).Count();
				return count == 0 ? 0 : Math.Round(1.0 * PacksCountTotal / count, 2);
			}
		}

		public int GoldTotal => GetFilteredRuns().Sum(x => x.Gold);

		public double GoldAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns(requirePackReward: true).Count();
				return count == 0 ? 0 : Math.Round(1.0 * GoldTotal / count, 2);
			}
		}

		public int GoldSpent => GetFilteredRuns().Count(x => x.Deck.ArenaReward.PaymentMethod == ArenaPaymentMethod.Gold) * 150;

		public int DustTotal => GetFilteredRuns().Sum(x => x.Dust);

		public double DustAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns(requirePackReward: true).Count();
				return count == 0 ? 0 : Math.Round(1.0 * DustTotal / count, 2);
			}
		}

		public int CardCountTotal => GetFilteredRuns().Sum(x => x.CardCount);

		public double CardCountAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns(requirePackReward: true).Count();
				return count == 0 ? 0 : Math.Round(1.0 * CardCountTotal / count, 2);
			}
		}

		public int CardCountGolden => GetFilteredRuns().Sum(x => x.CardCountGolden);

		public double CardCountGoldenAveragePerRun
		{
			get
			{
				var count = GetFilteredRuns(requirePackReward: true).Count();
				return count == 0 ? 0 : Math.Round(1.0 * CardCountGolden / count, 2);
			}
		}

		public ClassStats ClassStatsBest => !ClassStats.Any() ? null : ClassStats.OrderByDescending(x => x.WinRate).First();
		public ClassStats ClassStatsWorst => !ClassStats.Any() ? null : ClassStats.OrderBy(x => x.WinRate).First();
		public ClassStats ClassStatsMostPicked => !ClassStats.Any() ? null : ClassStats.OrderByDescending(x => x.Runs).First();
		public ClassStats ClassStatsLeastPicked => !ClassStats.Any() ? null : ClassStats.OrderBy(x => x.Runs).First();

		public ClassStats ClassStatsDruid => GetClassStats("Druid");
		public ClassStats ClassStatsHunter => GetClassStats("Hunter");
		public ClassStats ClassStatsMage => GetClassStats("Mage");
		public ClassStats ClassStatsPaladin => GetClassStats("Paladin");
		public ClassStats ClassStatsPriest => GetClassStats("Priest");
		public ClassStats ClassStatsRogue => GetClassStats("Rogue");
		public ClassStats ClassStatsShaman => GetClassStats("Shaman");
		public ClassStats ClassStatsWarlock => GetClassStats("Warlock");
		public ClassStats ClassStatsWarrior => GetClassStats("Warrior");

		public ClassStats ClassStatsAll => GetFilteredRuns(classFilter: false).GroupBy(x => true).Select(x => new ClassStats("All", x)).FirstOrDefault();

		public int RunsCount => GetFilteredRuns().Count();
		public int GamesCountTotal => GetFilteredRuns().Sum(x => x.Games.Count());
		public int GamesCountWon => GetFilteredRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Win));
		public int GamesCountLost => GetFilteredRuns().Sum(x => x.Games.Count(g => g.Result == GameResult.Loss));

		public double AverageWinsPerRun => (double)GamesCountWon / GetFilteredRuns().Count();

		public IEnumerable<ArenaRun> FilteredRuns => GetFilteredRuns();

		public IEnumerable<ChartStats> PlayedClassesPercent => GetFilteredRuns()
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
			return !runs.Any() ? null : new ClassStats(@class, runs);
		}

		public IEnumerable<ArenaRun> GetFilteredRuns(bool archivedFilter = true, bool classFilter = true, bool regionFilter = true,
		                                             bool timeframeFilter = true, bool requirePackReward = false)
		{
			var filtered = Runs;
			if(requirePackReward)
				filtered = filtered.Where(x => x.PackCount > 0);
			if (archivedFilter && !Config.Instance.ArenaStatsIncludeArchived)
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
						filtered = filtered.Where(g => g.StartTime >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
						break;
					case DisplayedTimeFrame.LastSeason:
						filtered = filtered.Where(g => g.StartTime >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1)
													&& g.StartTime < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
						break;
					case DisplayedTimeFrame.CustomSeason:
						var current = Helper.CurrentSeason;
						filtered = filtered.Where(g => g.StartTime >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
																		.AddMonths(Config.Instance.ArenaStatsCustomSeasonMin - current)
													&& g.StartTime < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
																		.AddMonths(Config.Instance.ArenaStatsCustomSeasonMax - current + 1));
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void OnPropertyChanged(string[] properties)
		{
			foreach(var prop in properties)
				OnPropertyChanged(prop);
		}

		public void UpdateArenaStats()
		{
			OnPropertyChanged(nameof(Runs));
			OnPropertyChanged(nameof(OpponentClassesPercent));
			OnPropertyChanged(nameof(PlayedClassesPercent));
			OnPropertyChanged(nameof(Wins));
			OnPropertyChanged(nameof(AvgWinsPerClass));
			OnPropertyChanged(nameof(FilteredRuns));
		}

		public void UpdateArenaStatsHighlights()
		{
			OnPropertyChanged(nameof(ClassStats));
			OnPropertyChanged(nameof(ClassStatsDruid));
			OnPropertyChanged(nameof(ClassStatsHunter));
			OnPropertyChanged(nameof(ClassStatsMage));
			OnPropertyChanged(nameof(ClassStatsPaladin));
			OnPropertyChanged(nameof(ClassStatsPriest));
			OnPropertyChanged(nameof(ClassStatsRogue));
			OnPropertyChanged(nameof(ClassStatsShaman));
			OnPropertyChanged(nameof(ClassStatsWarlock));
			OnPropertyChanged(nameof(ClassStatsWarrior));
			OnPropertyChanged(nameof(ClassStatsAll));
			OnPropertyChanged(nameof(ClassStatsBest));
			OnPropertyChanged(nameof(ClassStatsWorst));
			OnPropertyChanged(nameof(ClassStatsMostPicked));
			OnPropertyChanged(nameof(ClassStatsLeastPicked));
		}

		public void UpdateArenaRewards()
		{
			OnPropertyChanged(nameof(GoldTotal));
			OnPropertyChanged(nameof(GoldAveragePerRun));
			OnPropertyChanged(nameof(GoldSpent));
			OnPropertyChanged(nameof(DustTotal));
			OnPropertyChanged(nameof(DustAveragePerRun));
			OnPropertyChanged(nameof(PacksCountClassic));
			OnPropertyChanged(nameof(PacksCountGvg));
			OnPropertyChanged(nameof(PacksCountTgt));
			OnPropertyChanged(nameof(PacksCountTotal));
			OnPropertyChanged(nameof(PacksCountAveragePerRun));
			OnPropertyChanged(nameof(CardCountTotal));
			OnPropertyChanged(nameof(CardCountGolden));
			OnPropertyChanged(nameof(CardCountAveragePerRun));
			OnPropertyChanged(nameof(CardCountGoldenAveragePerRun));
		}

		public void UpdateArenaRuns()
		{
			OnPropertyChanged(nameof(Runs));
			OnPropertyChanged(nameof(FilteredRuns));
		}

		public void UpdateExpensiveArenaStats()
		{
			OnPropertyChanged(nameof(WinLossVsClass));
			OnPropertyChanged(nameof(WinsByClass));
		}
	}
}