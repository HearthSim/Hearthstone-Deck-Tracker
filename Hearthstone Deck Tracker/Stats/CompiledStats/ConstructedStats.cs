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
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedStats : INotifyPropertyChanged
	{
		public static ConstructedStats Instance { get; } = new ConstructedStats();
		public IEnumerable<GameStats> FilteredGames => GetFilteredGames();

		public event PropertyChangedEventHandler PropertyChanged;

		public IEnumerable<GameStats> GetFilteredGames(bool archived = true, bool playerClass = true, bool region = true,
													   bool timeFrame = true, bool mode = true, bool rank = true, bool format = true, 
													   bool turns = true, bool coin = true, bool result = true, bool oppClass = true, 
													   bool oppName = true, bool note = true, bool tags = true, bool includeNoDeck = true)
		{
			var decks = Config.Instance.ConstructedStatsActiveDeckOnly && DeckList.Instance.ActiveDeck != null ? new[] {DeckList.Instance.ActiveDeck} : DeckList.Instance.Decks.ToArray();
			return GetFilteredGames(decks, archived, playerClass, region, timeFrame, mode, rank, format, turns, coin, result,
									oppClass, oppName, note, tags, includeNoDeck);
		}

		public IEnumerable<GameStats> GetFilteredGames(IEnumerable<Deck> decks, bool archived = true, bool playerClass = true, bool region = true,
													   bool timeframe = true, bool mode = true, bool rank = true, 
													   bool format = true, bool turns = true, bool coin = true,
													   bool result = true, bool oppClass = true, bool oppName = true,
													   bool note = true, bool tags = true, bool includeNoDeck = true)
		{
			decks = decks.Where(x => !x.IsArenaDeck);

			if(archived && !Config.Instance.ConstructedStatsIncludeArchived && !Config.Instance.ConstructedStatsActiveDeckOnly)
				decks = decks.Where(x => !x.Archived);

			if(tags && Config.Instance.ConstructedStatsApplyTagFilters && !Config.Instance.SelectedTags.Contains("All") && !Config.Instance.ConstructedStatsActiveDeckOnly)
				decks = decks.Where(d => d.Tags.Any(t => Config.Instance.SelectedTags.Contains(t)));

			var filtered = decks.SelectMany(x => x.DeckStats.Games).Where(x => !x.IsClone);

			if(!Config.Instance.ConstructedStatsActiveDeckOnly && includeNoDeck)
				filtered = filtered.Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games).Where(x => !x.IsClone));

			if(playerClass && Config.Instance.ConstructedStatsClassFilter != HeroClassStatsFilter.All && !Config.Instance.ConstructedStatsActiveDeckOnly)
				filtered = filtered.Where(x => x.PlayerHero == Config.Instance.ConstructedStatsClassFilter.ToString());
			if(region && Config.Instance.ConstructedStatsRegionFilter != RegionAll.ALL)
			{
				var parsed = (Region)Enum.Parse(typeof(Region), Config.Instance.ConstructedStatsRegionFilter.ToString());
				filtered = filtered.Where(x => x.Region == parsed);
			}
			if(timeframe)
			{
				switch(Config.Instance.ConstructedStatsTimeFrameFilter)
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
																		.AddMonths(Config.Instance.ConstructedStatsCustomSeasonMin - current)
													&& g.StartTime < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
																		.AddMonths(Config.Instance.ConstructedStatsCustomSeasonMax - current + 1));
						break;
					case DisplayedTimeFrame.ThisWeek:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1)));
						break;
					case DisplayedTimeFrame.Today:
						filtered = filtered.Where(g => g.StartTime > DateTime.Today);
						break;
					case DisplayedTimeFrame.Custom:
						var start = (Config.Instance.ConstructedStatsTimeFrameCustomStart ?? DateTime.MinValue).Date;
						var end = (Config.Instance.ConstructedStatsTimeFrameCustomEnd ?? DateTime.MaxValue).Date;
						filtered = filtered.Where(g => g.EndTime.Date >= start && g.EndTime.Date <= end);
						break;
				}
			}
			if(mode && Config.Instance.ConstructedStatsModeFilter != GameMode.All)
				filtered = filtered.Where(x => x.GameMode == Config.Instance.ConstructedStatsModeFilter);
			if(rank)
			{
				var min = Config.Instance.ConstructedStatsRankFilterMin;
				if(min != "L1")
				{
					int minValue;
					if(min.StartsWith("L"))
					{
						if(int.TryParse(min.Substring(1), out minValue))
							filtered = filtered.Where(x => !x.HasLegendRank ||  x.LegendRank >= minValue);
					}
					else if(int.TryParse(min, out minValue))
						filtered = filtered.Where(x => !x.HasLegendRank && x.HasRank && x.Rank >= minValue);
					
				}
				var max = Config.Instance.ConstructedStatsRankFilterMax;
				if(max != "25")
				{
					int maxValue;
					if(max.StartsWith("L"))
					{
						if(int.TryParse(min.Substring(1), out maxValue))
							filtered = filtered.Where(x => x.HasLegendRank && x.LegendRank <= maxValue);
					}
					else if(int.TryParse(max, out maxValue))
						filtered = filtered.Where(x => x.HasLegendRank || x.HasRank && x.Rank <= maxValue);
					
				}
			}
			//if(format && Config.Instance.ConstructedStatsFormatFilter != Format.All)
			//{
			//	//TODO
			//}
			if(turns)
			{
				if(Config.Instance.ConstructedStatsTurnsFilterMin > 0)
					filtered = filtered.Where(x => x.Turns >= Config.Instance.ConstructedStatsTurnsFilterMin);
				if(Config.Instance.ConstructedStatsTurnsFilterMax < 99)
					filtered = filtered.Where(x => x.Turns <= Config.Instance.ConstructedStatsTurnsFilterMax);
			}
			if(coin && Config.Instance.ConstructedStatsCoinFilter != AllYesNo.All)
				filtered = filtered.Where(x => x.Coin == (Config.Instance.ConstructedStatsCoinFilter == AllYesNo.Yes));
			if(result && Config.Instance.ConstructedStatsResultFilter != GameResultAll.All)
			{
				var parsed = (GameResult)Enum.Parse(typeof(GameResult), Config.Instance.ConstructedStatsResultFilter.ToString());
				filtered = filtered.Where(x => x.Result == parsed);
			}
			if(oppClass && Config.Instance.ConstructedStatsOpponentClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.OpponentHero == Config.Instance.ConstructedStatsOpponentClassFilter.ToString());
			if(oppName && !string.IsNullOrEmpty(Config.Instance.ConstructedStatsOpponentNameFilter))
				filtered = filtered.Where(x => x.OpponentName?.ToUpperInvariant().Contains(Config.Instance.ConstructedStatsOpponentNameFilter.ToUpperInvariant()) ?? false);
			if(note && !string.IsNullOrEmpty(Config.Instance.ConstructedStatsNoteFilter))
				filtered = filtered.Where(x => x.Note?.ToUpperInvariant().Contains(Config.Instance.ConstructedStatsNoteFilter.ToUpperInvariant()) ?? false);

			return filtered.OrderByDescending(x => x.StartTime);
		}

		public IEnumerable<ChartStats> PlayedClassesPercent
		{
			get
			{
				var games = GetFilteredGames().ToList();
				return
					games.GroupBy(x => ClassSelector(x.PlayerHero))
						 .OrderBy(x => x.Key)
						 .Select(x =>
								 new ChartStats
								 {
									 Name = x.Key + " (" + Math.Round(100.0 * x.Count() / games.Count) + "%)",
									 Value = x.Count(),
									 Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
								 });
			}
		}

		public IEnumerable<ChartStats> Winrate
		{
			get
			{
				var games = GetFilteredGames().ToList();
				var wins = games.Where(x => x.Result == GameResult.Win).ToList();
				return wins.Count > 0
						   ? wins.Select(x => new ChartStats {Name = "Wins", Value = Math.Round(100.0 * wins.Count() / games.Count, 1)})
						   : EmptyChartStats("Wins");
			}
		}

		public IEnumerable<ChartStats> WinrateByCoin
		{
			get
			{
				var games = GetFilteredGames(coin: false).ToList();
				var wins = games.Where(x => x.Result == GameResult.Win).ToList();
				var gamesCoin = games.Where(x => x.Coin);
				var winsCoin = wins.Where(x => x.Coin).ToList();
				var gamesNoCoin = games.Where(x => !x.Coin);
				var winsNoCoin = wins.Where(x => !x.Coin).ToList();
				var total = wins.Count > 0
								? wins.Select(x => new ChartStats {Name = "Total", Value = Math.Round(100.0 * wins.Count() / games.Count, 1)})
								: EmptyChartStats("Wins");
				var coin = winsCoin.Count > 0
								? winsCoin.Select(x => new ChartStats {Name = "With Coin", Value = 100.0 * winsCoin.Count() / gamesCoin.Count()})
								: EmptyChartStats("With Coin");
				var noCoin = winsNoCoin.Count > 0
								 ? winsNoCoin.Select(x => new ChartStats {Name = "Without Coin", Value = 100.0 * winsNoCoin.Count() / gamesNoCoin.Count()})
								 : EmptyChartStats("Without Coin");
				return total.Concat(coin).Concat(noCoin);
			}
		}

		private ChartStats[] EmptyChartStats(string name) => new[] {new ChartStats() {Name = name, Value = 0}};

		public IEnumerable<ChartStats> OpponentClassesPercent
		{
			get
			{
				var games = GetFilteredGames(oppClass: false).ToList();
				return
					games.GroupBy(x => ClassSelector(x.OpponentHero))
						 .OrderBy(x => x.Key)
						 .Select(g =>
								 new ChartStats
								 {
									 Name = g.Key + " (" + Math.Round(100.0 * g.Count() / games.Count) + "%)",
									 Value = g.Count(),
									 Brush = new SolidColorBrush(Helper.GetClassColor(g.Key, true))
								 });
			}
		}

		private static string ClassSelector(string heroClass) => (Enum.GetNames(typeof(HeroClass)).Any(c => c == heroClass) ? heroClass : "Other");

		public IEnumerable<ChartStats> AvgWinratePerClass
			=> GetFilteredGames(DeckList.Instance.Decks, playerClass: false)
					.GroupBy(x => ClassSelector(x.PlayerHero))
					.OrderBy(x => x.Key)
					.Select(x =>
							new ChartStats
							{
								Name = x.Key,
								Value = Math.Round(100.0 * x.Count(g => g.Result == GameResult.Win) / x.Count(), 1),
								Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
							});

		public IEnumerable<ChartStats> WinrateAgainstClass
			=> GetFilteredGames()
					.GroupBy(x => ClassSelector(x.OpponentHero))
					.OrderBy(x => x.Key)
					.Select(x =>
							new ChartStats
							{
								Name = x.Key,
								Value = Math.Round(100.0 * x.Count(g => g.Result == GameResult.Win) / x.Count(), 1),
								Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
							});


		public IEnumerable<ConstructedMatchup> Matchups
		{
			get
			{
				var games = GetFilteredGames(playerClass: false, oppClass: false).ToList();
				foreach(var c in Enum.GetValues(typeof(HeroClass)).Cast<HeroClass>())
					yield return new ConstructedMatchup(c, games);
				yield return new ConstructedMatchup(games);
			}
		}

		public IEnumerable<ConstructedDeckDetails> DeckDetails
		{
			get
			{
				var deck = DeckList.Instance.ActiveDeck;
				if(deck == null)
					yield break;
				var games = GetFilteredGames(new[] {deck}, playerClass: false, oppClass: false).ToList();
				if(deck.HasVersions)
					yield return new ConstructedDeckDetails("All", games);
				foreach(var v in deck.VersionsIncludingSelf)
					yield return new ConstructedDeckDetails(v.ShortVersionString, games.Where(x => x.BelongsToDeckVerion(deck.GetVersion(v))));
			}
		} 


		public void UpdateConstructedStats()
		{
			OnPropertyChanged(nameof(FilteredGames));
			OnPropertyChanged(nameof(PlayedClassesPercent));
			OnPropertyChanged(nameof(OpponentClassesPercent));
			OnPropertyChanged(nameof(Winrate));
			OnPropertyChanged(nameof(DeckStatsTotal));
			OnPropertyChanged(nameof(HighestRank));
			if(!Config.Instance.ConstructedStatsActiveDeckOnly)
			{
				OnPropertyChanged(nameof(DeckStatsMostPlayed));
				OnPropertyChanged(nameof(DeckStatsBest));
				OnPropertyChanged(nameof(DeckStatsFastest));
				OnPropertyChanged(nameof(DeckStatsSlowest));
			}
			UpdateMatchups();
		}

		public void UpdateConstructedCharts()
		{
			OnPropertyChanged(nameof(WinrateByCoin));
			OnPropertyChanged(nameof(AvgWinratePerClass));
			OnPropertyChanged(nameof(WinrateAgainstClass));
		}

		public void UpdateMatchups()
		{
			if(Config.Instance.ConstructedStatsActiveDeckOnly)
				OnPropertyChanged(nameof(DeckDetails));
			else
				OnPropertyChanged(nameof(Matchups));
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateGames() => OnPropertyChanged(nameof(FilteredGames));

		public ConstructedDeckStats DeckStatsBest
			=> GetFilteredGames(includeNoDeck: false)
					.GroupBy(x => x.DeckId)
					.Select(x => new { grouping = x, WinRate = WeightedWinrate(x) })
					.OrderByDescending(x => x.WinRate)
					.FirstOrDefault(x => x.grouping.Any())?
					.grouping?.ToConstructedDeckStats();

		public ConstructedDeckStats DeckStatsMostPlayed
			=> GetFilteredGames(includeNoDeck: false)
					.GroupBy(x => x.DeckId)
					.Select(x => new { grouping = x, Count = x.Count() })
					.OrderByDescending(x => x.Count)
					.FirstOrDefault(x => x.grouping.Any())?
					.grouping?.ToConstructedDeckStats();

		public ConstructedDeckStats DeckStatsFastest
			=> GetFilteredGames(includeNoDeck: false)
					.GroupBy(x => x.DeckId)
					.Where(x => x.Count() > 1)
					.OrderBy(x => x.Average(g => g.Turns))
					.FirstOrDefault()?.ToConstructedDeckStats();

		public ConstructedDeckStats DeckStatsSlowest
			=> GetFilteredGames(includeNoDeck: false)
					.GroupBy(x => x.DeckId)
					.Where(x => x.Count() > 1)
					.OrderByDescending(x => x.Average(g => g.Turns))
					.FirstOrDefault()?.ToConstructedDeckStats();

		public ConstructedDeckStats DeckStatsTotal
		{
			get
			{
				var games = GetFilteredGames().ToArray();
				return games.Length > 0 ? new ConstructedDeckStats(games) : null;
			}
		}

		public string HighestRank => GetFilteredGames().OrderBy(x => x.SortableRank).FirstOrDefault()?.RankString;

		public double WeightedWinrate(IEnumerable<GameStats> matches)
		{
			if(matches == null)
				return 0;
			var heroes = Enum.GetValues(typeof(HeroClass)).Cast<HeroClass>().ToArray();
			return heroes.Select(hero => matches.Where(x => x.OpponentHero == hero.ToString()))
							 .Average(x =>
							 {
								 var games = x.ToArray();
								 return (games.Length > 0 ? (double)games.Count(g => g.Result == GameResult.Win) / games.Length : 0.5) * (games.Length + 1);
							 });
		}
	}
}