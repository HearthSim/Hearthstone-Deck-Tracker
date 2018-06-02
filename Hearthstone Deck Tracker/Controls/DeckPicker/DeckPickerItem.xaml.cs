#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPickerItem.xaml
	/// </summary>
	public partial class DeckPickerItem : INotifyPropertyChanged
	{
		private const string LocUse = "DeckPicker_Deck_Label_Use";
		private const string LocActive = "DeckPicker_Deck_Label_Active";

		private static Type _deckPickerItem = typeof(DeckPickerItemLayout1);

		public DeckPickerItem()
		{
			InitializeComponent();
			Deck = DataContext as Deck;
			SetLayout();
		}

		public DeckPickerItem(Deck deck, Type deckPickerItemLayout)
		{
			InitializeComponent();
			DataContext = deck;
			Deck = deck;
			_deckPickerItem = deckPickerItemLayout;
			SetLayout();
		}

		public Deck Deck { get; set; }

		public FontWeight FontWeightActiveDeck => Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : FontWeights.Regular;

		public FontWeight FontWeightSelected => Equals(Deck, DeckList.Instance.ActiveDeck)
													? FontWeights.Bold
													: (Core.MainWindow.DeckPickerList.SelectedDecks.Contains(Deck) ? FontWeights.SemiBold : FontWeights.Regular);

		public string TextUseButton => Deck.Equals(DeckList.Instance.ActiveDeck) ? LocUtil.Get(LocActive, true) : LocUtil.Get(LocUse, true);

		public Visibility HsReplayDataIndicatorVisibility => HsReplayDataManager.Decks.AvailableDecks.Contains(Deck.GetSelectedDeckVersion().ShortId) ? Visibility.Visible : Visibility.Collapsed;

		public event PropertyChangedEventHandler PropertyChanged;

		public void SetLayout() => Content = Activator.CreateInstance(_deckPickerItem);

		public string DataIndicatorTooltip => LocUtil.Get("DeckCharts_Tooltip_Uploaded");

		public string WildIndicatorTooltip => LocUtil.Get("DeckPicker_Deck_Wild_Tooltip");

		public string ArchivedTooltip => LocUtil.Get("DeckPicker_Deck_Archived_Tooltip");

		public string LegacyNoStatsNo => LocUtil.Get("DeckPicker_Deck_Legacy_NoStats_No");

		public string LegacyNoStatsStats => LocUtil.Get("DeckPicker_Deck_Legacy_NoStats_Stats");

		public string StatsString => Deck.StatsString;

		public string DateShownOnDeckTooltip
		{
			get
			{
				switch (Config.Instance.SelectedDateOnDecks)
				{
					case DeckDateType.LastPlayed:
						return LocUtil.Get("DeckPicker_Deck_LastPlayed_Tooltip");
					case DeckDateType.LastEdited:
						return LocUtil.Get("DeckPicker_Deck_LastEdited_Tooltip");
					default:
						return null;
				}
			}
		}

		public DateTime DateShownOnDeck
		{
			get
			{
				switch(Config.Instance.SelectedDateOnDecks)
				{
					case DeckDateType.LastPlayed:
						return Deck.LastPlayed;
					case DeckDateType.LastEdited:
						return Deck.LastEdited;
					default:
						return DateTime.MinValue;
				}
			}
		}

		public void RefreshProperties()
		{
			OnPropertyChanged(nameof(FontWeightSelected));
			OnPropertyChanged(nameof(FontWeightActiveDeck));
			OnPropertyChanged(nameof(TextUseButton));
			OnPropertyChanged(nameof(LastPlayed));
			OnPropertyChanged(nameof(HsReplayDataIndicatorVisibility));
			OnPropertyChanged(nameof(StatsString));
			Deck.UpdateWildIndicatorVisibility();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region sorting properties

		public string Class => Deck.GetClass;

		public DateTime LastEdited => Deck.LastEdited;

		public DateTime LastPlayed => Deck.LastPlayed;

		public DateTime LastPlayedNewFirst => Deck.LastPlayedNewFirst;

		public double WinPercent => Deck.WinPercent;

		public string DeckName => Deck.Name;

		public string TagList => Deck.TagList;

		public int NumGames => Deck.GetRelevantGames().Count;

		public bool Favorite => Deck.Tags.Any(x => x.ToUpperInvariant() == "FAVORITE");

		#endregion
	}
}
