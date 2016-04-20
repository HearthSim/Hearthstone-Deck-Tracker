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
													   bool timeframeFilter = true)
		{
			IEnumerable<Deck> decks = DeckList.Instance.Decks;
			if(archivedFilter && !Config.Instance.ArenaStatsIncludeArchived)
				decks = decks.Where(x => !x.Archived);

			var filtered = decks.SelectMany(x => x.DeckStats.Games);

			//if(includenodeck)
			filtered = filtered.Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games));


			if(classFilter && Config.Instance.ArenaStatsClassFilter != HeroClassStatsFilter.All)
				filtered = filtered.Where(x => x.PlayerHero == Config.Instance.ArenaStatsClassFilter.ToString());
			if(regionFilter && Config.Instance.ArenaStatsRegionFilter != RegionAll.ALL)
			{
				var region = (Region)Enum.Parse(typeof(Region), Config.Instance.ArenaStatsRegionFilter.ToString());
				filtered = filtered.Where(x => x.Region == region);
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