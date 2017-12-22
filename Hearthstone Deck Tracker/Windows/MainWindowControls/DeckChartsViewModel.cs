using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public class DeckChartsViewModel : ViewModel
	{
		private Deck _deck;
		private double _winrateTotal;
		private bool _hasData;
		private List<GameStats> _games;
		private int _wins;
		private int _losses;
		private bool _hasDeck;

		private readonly string[] _playerClasses =
			{ "Druid", "Hunter", "Mage", "Paladin", "Priest", "Shaman", "Rogue", "Warlock", "Warrior" };

		public SeriesCollection OpponentCollection { get; }

		public Deck Deck
		{
			get => _deck;
			set
			{
				if(_deck != null)
					_deck.OnStatsUpdated -= Update;
				_deck = value;
				if(_deck != null)
					_deck.OnStatsUpdated += Update;
				HasDeck = _deck != null;
				Update();
				OnPropertyChanged();
			}
		}

		public List<GameStats> Games
		{
			get => _games;
			set
			{
				_games = value;
				OnPropertyChanged();
			}
		}

		public int Wins
		{
			get => _wins;
			set
			{
				_wins = value; 
				OnPropertyChanged();
			}
		}

		public int Losses
		{
			get => _losses;
			set
			{
				_losses = value; 
				OnPropertyChanged();
			}
		}

		public bool HasData
		{
			get => _hasData;
			set
			{
				_hasData = value; 
				OnPropertyChanged();
			}
		}

		public double WinrateTotal
		{
			get => _winrateTotal;
			set
			{
				_winrateTotal = value;
				OnPropertyChanged();
			}
		}

		public bool HasDeck
		{
			get => _hasDeck;
			set
			{
				_hasDeck = value;
				OnPropertyChanged();
			}
		}

		public Func<ChartPoint, string> PointLabel { get; }

		public Func<double, string> EmptyFormatter { get; } = val => string.Empty;

		public DeckChartsViewModel()
		{
			OpponentCollection = new SeriesCollection();

			// % first because we have some nasty flow direction changing going on
			// to make the chart resize properly, depending on the width of the legend
			PointLabel = p => p.Participation == 0 ? "" : $"%{Math.Round(p.Participation * 100, 0)}";

			var series = _playerClasses.Select(p => new PieSeries
			{
				Title = p,
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				Fill = new SolidColorBrush(Helper.GetClassColor(p, true)),
				Foreground = Brushes.Black,
				LabelPoint = PointLabel,
				DataLabels = true,
			});
			OpponentCollection.AddRange(series);
		}

		public void Update()
		{
			Games = _deck?.GetRelevantGames().OrderByDescending(x => x.StartTime).ToList();
			HasData = Games?.Any() ?? false;

			if(!HasData)
				return;

			var wins = 0;
			var losses = 0;
			var opponents = _playerClasses.ToDictionary(x => x, x => 0);

			foreach(var game in Games)
			{
				if(opponents.ContainsKey(game.OpponentHero))
					opponents[game.OpponentHero]++;
				if(game.Result == GameResult.Win)
					wins++;
				else if(game.Result == GameResult.Loss)
					losses++;
			}

			foreach(var series in OpponentCollection)
				((ObservableValue)series.Values[0]).Value = opponents[series.Title];

			var total = wins + losses;
			Wins = wins;
			Losses = losses;
			WinrateTotal = total > 0 ? Math.Round(100.0 * wins/total) : 0;
		}
	}
}
