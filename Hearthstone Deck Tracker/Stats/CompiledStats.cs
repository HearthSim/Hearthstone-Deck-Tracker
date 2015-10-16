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

		public IEnumerable<ArenaRun> FilteredArenaRuns
		{
			get
			{
				var filtered = ArenaRuns;
				if(Config.Instance.ArenaStatsClassFilter != HeroClassStatsFilter.All)
				{
					filtered = filtered.Where(x => x.Class == Config.Instance.ArenaStatsClassFilter.ToString());
				}
				switch(Config.Instance.ArenaStatsTimeFrameFilter)
				{
					case DisplayedTimeFrame.AllTime:
						return filtered;
					case DisplayedTimeFrame.CurrentSeason:
						return filtered.Where(g => g.StartTime > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
					case DisplayedTimeFrame.ThisWeek:
						return filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1)));
					case DisplayedTimeFrame.Today:
						return filtered.Where(g => g.StartTime > DateTime.Today);
					case DisplayedTimeFrame.Custom:
						var start = (Config.Instance.ArenaStatsTimeFrameCustomStart ?? DateTime.MinValue).Date;
						var end = (Config.Instance.ArenaStatsTimeFrameCustomEnd ?? DateTime.MaxValue).Date;
						return filtered.Where(g => g.EndTime.Date >= start && g.EndTime.Date <= end);
					default:
						return filtered;
				}
			}
		}

		public IEnumerable<ChartStats> ArenaPlayedClassesPercent
		{
			get
			{
				return
					FilteredArenaRuns.GroupBy(x => x.Class)
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
				var opponents = FilteredArenaRuns.SelectMany(x => x.Deck.DeckStats.Games.Select(g => g.OpponentHero)).ToList();
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
					FilteredArenaRuns.GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
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
					FilteredArenaRuns.GroupBy(x => x.Deck.DeckStats.Games.Count(g => g.Result == GameResult.Win))
									 .Select(x => new { Wins = x.Key, Count = x.Count(), Runs = x })
									 .ToList();
				return Enumerable.Range(0, 13).Select(n =>
				{
					var runs = groupedByWins.FirstOrDefault(x => x.Wins == n);
					if(runs == null)
						return new[] { new ChartStats { Name = n.ToString(), Value = 0, Brush = new SolidColorBrush() } };
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

		public IEnumerable<ChartStats> AvgWinsPerClass
		{
			get
			{
				return
					FilteredArenaRuns.GroupBy(x => x.Class)
					                 .OrderBy(x => x.Key)
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
					                         });
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

		public void UpdateArenaStats()
		{
			OnPropertyChanged("ArenaRuns");
			OnPropertyChanged("ArenaOpponentClassesPercent");
			OnPropertyChanged("ArenaPlayedClassesPercent");
			OnPropertyChanged("ArenaWins");
			OnPropertyChanged("ArenaWinsByClass");
			OnPropertyChanged("AvgWinsPerClass");
			OnPropertyChanged("FilteredArenaRuns");
		}

		public void UpdateArenaRuns()
		{
			OnPropertyChanged("ArenaRuns");
			OnPropertyChanged("FilteredArenaRuns");
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
		public Deck Deck { get { return _deck; } }
		public ArenaRun(Deck deck)
		{
			_deck = deck;
		}

		public string Class { get { return _deck.Class; } }
		public BitmapImage ClassImage { get { return _deck.ClassImage; } }
		public string StartTimeString { get { return StartTime == DateTime.MinValue ? "-" :  StartTime.ToString("dd.MMM HH:mm"); } }
		public DateTime StartTime { get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Min(g => g.StartTime) : DateTime.MinValue; } }
		public DateTime EndTime { get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.Max(g => g.EndTime) : DateTime.MinValue; } }
		public int Wins { get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Win); } }
		public int Losses { get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Loss); } }
		public int Gold { get { return _deck.ArenaReward.Gold; } }
		public int Dust { get { return _deck.ArenaReward.Dust; } }
		public int PackCount { get { return _deck.ArenaReward.Packs.Count(x => !string.IsNullOrEmpty(x)); } }

		public string PackString
		{
			get
			{
				var packs = _deck.ArenaReward.Packs.Where(x => !string.IsNullOrEmpty(x)).ToList();
				return packs.Any() ? packs.Aggregate((c, n) => c + ", " + n) : "None";
			}
		}

		public int CardCount { get { return _deck.ArenaReward.Cards.Count(x => x != null && !string.IsNullOrEmpty(x.CardId)); } }

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
		
		public string DurationString { get { return Duration + " min"; } }

		public string Region { get { return _deck.DeckStats.Games.Any() ? _deck.DeckStats.Games.First().Region.ToString() : "UNKNOWN"; } }
		public IEnumerable<GameStats> Games { get { return _deck.DeckStats.Games; } } 
	}
}
