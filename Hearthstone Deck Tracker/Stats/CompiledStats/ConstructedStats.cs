#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public class ConstructedStats : INotifyPropertyChanged
	{
		public static ConstructedStats Instance { get; } = new ConstructedStats();
		public IEnumerable<GameStats> FilteredGames => GetFilteredGames();

		public event PropertyChangedEventHandler PropertyChanged;


		public IEnumerable<GameStats> GetFilteredGames(bool archivedFilter = true, bool classFilter = true, bool regionFilter = true,
													   bool timeframeFilter = true, bool modeFilter = true, bool rankFilter = true, 
													   bool filterFormat = true, bool turnsFilter = true, bool coinFilter = true,
													   bool resultFilter = true, bool oppClassFilter = true, bool oppNameFilter = true,
													   bool noteFilter = true, bool applyTagFilters = true)
		{
			IEnumerable<Deck> decks = DeckList.Instance.Decks;
			if(archivedFilter && !Config.Instance.ConstructedStatsIncludeArchived)
				decks = decks.Where(x => !x.Archived);

			var filtered = decks.SelectMany(x => x.DeckStats.Games);

			filtered = filtered.Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games));


			if(classFilter && Config.Instance.ConstructedStatsClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.PlayerHero == Config.Instance.ConstructedStatsClassFilter.ToString());
			if(regionFilter && Config.Instance.ConstructedStatsRegionFilter != RegionAll.ALL)
			{
				var region = (Region)Enum.Parse(typeof(Region), Config.Instance.ConstructedStatsRegionFilter.ToString());
				filtered = filtered.Where(x => x.Region == region);
			}
			if(timeframeFilter)
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
			if(modeFilter && Config.Instance.ConstructedStatsModeFilter != GameMode.All)
				filtered = filtered.Where(x => x.GameMode == Config.Instance.ConstructedStatsModeFilter);
			if(rankFilter)
			{
				var min = Config.Instance.ConstructedStatsRankFilterMin;
				int rank;
				if(min != "L1")
				{
					if(min.StartsWith("L"))
					{
						//if(int.TryParse(min.Substring(1), out rank))
						//	filtered = filtered.Where(x => x.LegendRank >= rank);
					}
					else
					{
						if(int.TryParse(min, out rank))
							filtered = filtered.Where(x => x.Rank >= rank);
					}
				}
				var max = Config.Instance.ConstructedStatsRankFilterMax;
				if(max != "25")
				{
					if(max.StartsWith("L"))
					{
						//if(int.TryParse(min.Substring(1), out rank))
						//	filtered = filtered.Where(x => x.LegendRank <= rank);
					}
					else
					{
						if(int.TryParse(max, out rank))
							filtered = filtered.Where(x => x.Rank <= rank);
					}
				}
			}
			//if(filterFormat && Config.Instance.ConstructedStatsFormatFilter != Format.All)
			//{
			//	//TODO
			//}
			if(turnsFilter)
			{
				if(Config.Instance.ConstructedStatsTurnsFilterMin > 0)
					filtered = filtered.Where(x => x.Turns >= Config.Instance.ConstructedStatsTurnsFilterMin);
				if(Config.Instance.ConstructedStatsTurnsFilterMax < 99)
					filtered = filtered.Where(x => x.Turns <= Config.Instance.ConstructedStatsTurnsFilterMax);
			}
			if(coinFilter && Config.Instance.ConstructedStatsCoinFilter != AllYesNo.All)
				filtered = filtered.Where(x => x.Coin == (Config.Instance.ConstructedStatsCoinFilter == AllYesNo.Yes));
			if(resultFilter && Config.Instance.ConstructedStatsResultFilter != GameResultAll.All)
			{
				var result = (GameResult)Enum.Parse(typeof(GameResult), Config.Instance.ConstructedStatsResultFilter.ToString());
				filtered = filtered.Where(x => x.Result == result);
			}
			if(oppClassFilter && Config.Instance.ConstructedStatsOpponentClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.OpponentHero == Config.Instance.ConstructedStatsOpponentClassFilter.ToString());
			if(oppNameFilter && !string.IsNullOrEmpty(Config.Instance.ConstructedStatsOpponentNameFilter))
				filtered = filtered.Where(x => x.OpponentName?.Contains(Config.Instance.ConstructedStatsOpponentNameFilter) ?? false);
			if(noteFilter && !string.IsNullOrEmpty(Config.Instance.ConstructedStatsNoteFilter))
				filtered = filtered.Where(x => x.Note?.Contains(Config.Instance.ConstructedStatsNoteFilter) ?? false);
			if(applyTagFilters && Config.Instance.ConstructedStatsApplyTagFilters && !Config.Instance.SelectedTags.Contains("All"))
				filtered = filtered.Where(x => DeckList.Instance.Decks.Any(d => d.DeckId == x.DeckId && d.Tags.Any(t => Config.Instance.SelectedTags.Contains(t))));

			return filtered;
		}

		public IEnumerable<ChartStats> PlayedClassesPercent
		{
			get
			{
				var games = GetFilteredGames().ToList();
				return
					games.GroupBy(x => ClassSelector(x.PlayerHero))
						 .OrderBy(x => x.Key)
						 .Select(
								 x =>
								 new ChartStats
								 {
									 Name = x.Key + " (" + Math.Round(100.0 * x.Count() / games.Count) + "%)",
									 Value = x.Count(),
									 Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
								 });
			}
		}

		public IEnumerable<ChartStats> OpponentClassesPercent
		{
			get
			{
				var games = GetFilteredGames().ToList();
				return
					games.GroupBy(x => ClassSelector(x.OpponentHero))
						 .OrderBy(x => x.Key)
						 .Select(
								 g =>
								 new ChartStats
								 {
									 Name = g.Key + " (" + Math.Round(100.0 * g.Count() / games.Count) + "%)",
									 Value = g.Count(),
									 Brush = new SolidColorBrush(Helper.GetClassColor(g.Key, true))
								 });
			}
		}

		private static string ClassSelector(string heroClass) => (Enum.GetNames(typeof(HeroClass)).Any(c => c == heroClass) ? heroClass : "Other");

		public IEnumerable<ChartStats> AvgWinratePerClass => GetFilteredGames()
			.GroupBy(x => ClassSelector(x.PlayerHero))
			.Select(
				    x =>
					new ChartStats
					{
						Name = x.Key,
						Value = Math.Round(100.0 * x.Count(g => g.Result == GameResult.Win) / x.Count(), 1),
						Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
					})
			.OrderBy(x => x.Value);


		public IEnumerable<Matchup> Matchups
		{
			get
			{
				var games = GetFilteredGames(classFilter: false).ToList();
				return new[]
				{
					new Matchup(HeroClass.Druid, games),
					new Matchup(HeroClass.Hunter, games),
					new Matchup(HeroClass.Mage, games),
					new Matchup(HeroClass.Paladin, games),
					new Matchup(HeroClass.Priest, games),
					new Matchup(HeroClass.Rogue, games),
					new Matchup(HeroClass.Shaman, games),
					new Matchup(HeroClass.Warlock, games),
					new Matchup(HeroClass.Warrior, games),
					new Matchup(games)
				};
			}
		}

		public class Matchup
		{
			private readonly HeroClass? _player;
			private readonly IEnumerable<GameStats> _games;

			public string Class => _player?.ToString() ?? "Total";
			public BitmapImage ClassImage => _player != null ? ImageCache.GetClassIcon(_player.ToString()) : new BitmapImage();

			public Matchup(HeroClass player, IEnumerable<GameStats> games)
			{
				_player = player;
				_games = games.Where(x => x.PlayerHero == player.ToString());
			}

			public Matchup(IEnumerable<GameStats> games)
			{
				_games = games;
			}

			public MatchupStats Druid => GetMatchupStats(HeroClass.Druid);
			public MatchupStats Hunter => GetMatchupStats(HeroClass.Hunter);
			public MatchupStats Mage => GetMatchupStats(HeroClass.Mage);
			public MatchupStats Paladin => GetMatchupStats(HeroClass.Paladin);
			public MatchupStats Priest => GetMatchupStats(HeroClass.Priest);
			public MatchupStats Rogue => GetMatchupStats(HeroClass.Rogue);
			public MatchupStats Shaman => GetMatchupStats(HeroClass.Shaman);
			public MatchupStats Warlock => GetMatchupStats(HeroClass.Warlock);
			public MatchupStats Warrior => GetMatchupStats(HeroClass.Warrior);
			public MatchupStats Total => new MatchupStats("Total", _games);

			public MatchupStats GetMatchupStats(HeroClass opponent)
				=> new MatchupStats(opponent.ToString(), _games.Where(x => x.OpponentHero == opponent.ToString()).Select(x => x));
		}

		public void UpdateConstructedStats()
		{
			OnPropertyChanged(nameof(FilteredGames));
			OnPropertyChanged(nameof(PlayedClassesPercent));
			OnPropertyChanged(nameof(OpponentClassesPercent));
			OnPropertyChanged(nameof(AvgWinratePerClass));
			OnPropertyChanged(nameof(Matchups));
		}

		public void UpdateMatchups() => OnPropertyChanged(nameof(Matchups));

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}