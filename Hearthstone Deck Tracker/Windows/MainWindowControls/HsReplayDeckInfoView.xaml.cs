using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class HsReplayDeckInfoView : UserControl
	{
		public HsReplayDeckInfoView()
		{
			InitializeComponent();
		}
		public void SetDeck(Deck deck) => ((HsReplayDeckInfoViewModel)DataContext).Deck = deck;
	}

	public class HsReplayDeckInfoViewModel : ViewModel
	{
		private Deck _deck;
		private double _winrate;
		private bool _hasHsReplayData;
		private bool _hasDeck;
		private MatchupData _matchupData;
		private bool _loading;

		public HsReplayDeckInfoViewModel()
		{
			HsReplayDataManager.Decks.OnLoaded += () =>
			{
				HasHsReplayData = !string.IsNullOrEmpty(ShortId) && HsReplayDataManager.Decks.AvailableDecks.Contains(ShortId);
				FetchData();
			};
		}

		public Deck Deck
		{
			get => _deck;
			set
			{
				_deck = value;
				HasDeck = _deck != null;
				ShortId = _deck?.GetSelectedDeckVersion().ShortId;
				OnPropertyChanged();
				UpdateWinrate();
				HasHsReplayData = !string.IsNullOrEmpty(ShortId) && HsReplayDataManager.Decks.AvailableDecks.Contains(ShortId);
				FetchData();
			}
		}

		private async void FetchData()
		{
			if(!HasHsReplayData)
				return;
			Loading = true;
			var data = await HsReplayDataManager.Winrates.Get(ShortId, _deck.IsWildDeck);
			MatchupData = new MatchupData(data);
			Loading = false;
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

		public string ShortId { get; set; }

		public bool HasHsReplayData
		{
			get => _hasHsReplayData;
			set
			{
				_hasHsReplayData = value; 
				OnPropertyChanged();
			}
		}

		public void UpdateWinrate()
		{
			if(Deck == null)
				return;

			var games = Deck.DeckStats.Games.Count;
			var wins = Deck.DeckStats.Games.Count(x => x.Result == GameResult.Win);

			Winrate = games > 0 ? Math.Round(100.0 * wins / games) : 0;
		}

		public double Winrate
		{
			get => _winrate;
			set
			{
				_winrate = value; 
				OnPropertyChanged();
			}
		}

		public MatchupData MatchupData
		{
			get => _matchupData;
			set
			{
				_matchupData = value; 
				OnPropertyChanged();
			}
		}

		public ICommand OpenDeckPageCommand	=> new Command(() =>
		{
			if(Deck?.ShortId != null)
			{
				var url = Helper.BuildHsReplayNetUrl($"/decks/{ShortId}", "mulliganguide");
				Helper.TryOpenUrl(url);
			}
		});

		public Func<double, string> EmptyFormatter { get; } = val => string.Empty;

		public bool Loading
		{
			get => _loading;
			set
			{
				_loading = value; 
				OnPropertyChanged();
			}
		}

		public ICommand OpenTrendingDecksCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("/decks/trending", "trending");
			Helper.TryOpenUrl(url);
		});
	}

	public class MatchupData
	{
		private readonly DeckWinrateData _data;

		public MatchupData(DeckWinrateData data)
		{
			_data = data;
		}

		public string Total => _data != null ? Math.Round(_data.TotalWinrate).ToString(CultureInfo.InvariantCulture) : "-";
		public string Druid => GetValue("DRUID");
		public string Hunter => GetValue("HUNTER");
		public string Mage => GetValue("MAGE");
		public string Paladin => GetValue("PALADIN");
		public string Priest => GetValue("PRIEST");
		public string Rogue => GetValue("ROGUE");
		public string Shaman => GetValue("SHAMAN");
		public string Warlock => GetValue("WARLOCK");
		public string Warrior => GetValue("WARRIOR");

		public double TotalValue => _data != null ? Math.Round(_data.TotalWinrate) : 0;

		public string GetValue(string playerClass)
		{
			if(_data?.ClassWinrates != null && _data.ClassWinrates.TryGetValue(playerClass, out var value))
				return Math.Round(value).ToString(CultureInfo.InvariantCulture);
			return "-";
		}
	}
}
