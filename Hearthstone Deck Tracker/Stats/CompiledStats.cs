using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class CompiledStats
	{
		private static IEnumerable<Deck> ArenaDecks { get { return DeckList.Instance.Decks.Where(x => x != null && x.IsArenaDeck); } } 
		public static IEnumerable<ArenaRun> ArenaRuns { get { return ArenaDecks.Select(x => new ArenaRun(x)); } }

		public static IEnumerable<ChartStats> ArenaPlayedClassesPercent
		{
			get
			{
				return
					ArenaDecks.GroupBy(x => x.Class).OrderBy(x => x.Key)
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

		public static IEnumerable<ChartStats> ArenaOpponentClassesPercent
		{
			get
			{
				var opponents = ArenaDecks.SelectMany(x => x.DeckStats.Games.Select(g => g.OpponentHero)).ToList();
				return
					opponents.GroupBy(x => x).OrderBy(x => x.Key)
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

		public static ChartStats[][] ArenaWins
		{
			get
			{
				var groupedByWins =
					ArenaDecks.GroupBy(x => x.DeckStats.Games.Count(g => g.Result == GameResult.Win))
					          .Select(x => new {Wins = x.Key, Count = x.Count(), Runs = x})
					          .ToList();
				return Enumerable.Range(0, 13).Select(n =>
				{
					var runs = groupedByWins.FirstOrDefault(x => x.Wins == n);
					if(runs == null)
						return new[] {new ChartStats {Name = n.ToString(), Value = 0, Brush = new SolidColorBrush()}};
					return
						runs.Runs.GroupBy(x => x.Class).OrderBy(x => x.Key)
						    .Select(
						            x =>
						            new ChartStats
						            {
							            Name = n + "wins (" + x.Key + ")",
							            Value = x.Count(),
							            Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
						            })
						    .ToArray();
				}).ToArray();
			}
		}

		public static IEnumerable<ChartStats> AvgWinsPerClass
		{
			get
			{
				return
					ArenaDecks.GroupBy(x => x.Class).OrderBy(x => x.Key)
					          .Select(
					                  x =>
					                  new ChartStats
					                  {
						                  Name = x.Key,
						                  Value = Math.Round((double)x.Sum(d => d.DeckStats.Games.Count(g => g.Result == GameResult.Win)) / x.Count(),1),
						                  Brush = new SolidColorBrush(Helper.GetClassColor(x.Key, true))
					                  });
			}
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
		public string Wins { get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Win).ToString(); } }
		public string Losses { get { return _deck.DeckStats.Games.Count(x => x.Result == GameResult.Loss).ToString(); } }
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
