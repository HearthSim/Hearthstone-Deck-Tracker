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
	public class ConstructedStats : INotifyPropertyChanged
	{
		public static ConstructedStats Instance { get; } = new ConstructedStats();
		public IEnumerable<GameStats> FilteredGames => GetFilteredGames();

		public event PropertyChangedEventHandler PropertyChanged;

		public IEnumerable<GameStats> GetFilteredGames(bool archived = true, bool playerClass = true, bool region = true,
													   bool timeFrame = true, bool mode = true, bool rank = true, bool format = true, bool turns = true, bool coin = true,
													   bool result = true, bool oppClass = true, bool oppName = true, bool note = true, bool tags = true)
		{
			var decks = Config.Instance.ConstructedStatsActiveDeckOnly && DeckList.Instance.ActiveDeck != null ? new[] {DeckList.Instance.ActiveDeck} : DeckList.Instance.Decks.ToArray();
			return GetFilteredGames(decks, archived, playerClass, region, timeFrame, mode, rank, format, turns, coin, result,
									oppClass, oppName, note, tags);
		}

		public IEnumerable<GameStats> GetFilteredGames(IEnumerable<Deck> decks, bool archived = true, bool playerClass = true, bool region = true,
													   bool timeframe = true, bool mode = true, bool rank = true, 
													   bool format = true, bool turns = true, bool coin = true,
													   bool result = true, bool oppClass = true, bool oppName = true,
													   bool note = true, bool tags = true)
		{
			if(archived && !Config.Instance.ConstructedStatsIncludeArchived && !Config.Instance.ConstructedStatsActiveDeckOnly)
				decks = decks.Where(x => !x.Archived);

			if(tags && Config.Instance.ConstructedStatsApplyTagFilters && !Config.Instance.SelectedTags.Contains("All") && !Config.Instance.ConstructedStatsActiveDeckOnly)
				decks = decks.Where(d => d.Tags.Any(t => Config.Instance.SelectedTags.Contains(t)));

			var filtered = decks.SelectMany(x => x.DeckStats.Games);

			if(!Config.Instance.ConstructedStatsActiveDeckOnly)
				filtered = filtered.Concat(DefaultDeckStats.Instance.DeckStats.SelectMany(x => x.Games));

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
				int value;
				if(min != "L1")
				{
					if(min.StartsWith("L"))
					{
						//if(int.TryParse(min.Substring(1), out rank))
						//	filtered = filtered.Where(x => x.LegendRank >= rank);
					}
					else
					{
						if(int.TryParse(min, out value))
							filtered = filtered.Where(x => x.Rank >= value);
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
						if(int.TryParse(max, out value))
							filtered = filtered.Where(x => x.Rank <= value);
					}
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
				filtered = filtered.Where(x => x.OpponentName?.Contains(Config.Instance.ConstructedStatsOpponentNameFilter) ?? false);
			if(note && !string.IsNullOrEmpty(Config.Instance.ConstructedStatsNoteFilter))
				filtered = filtered.Where(x => x.Note?.Contains(Config.Instance.ConstructedStatsNoteFilter) ?? false);

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
				else
					yield return new ConstructedDeckDetails("1.0", games);
				foreach(var v in deck.Versions)
					yield return new ConstructedDeckDetails(v.Version.ShortVersionString, games.Where(x => x.BelongsToDeckVerion(v)));
			}
		} 


		public void UpdateConstructedStats()
		{
			OnPropertyChanged(nameof(FilteredGames));
			OnPropertyChanged(nameof(PlayedClassesPercent));
			OnPropertyChanged(nameof(OpponentClassesPercent));
			OnPropertyChanged(nameof(AvgWinratePerClass));
			UpdateMatchups();

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
	}
}