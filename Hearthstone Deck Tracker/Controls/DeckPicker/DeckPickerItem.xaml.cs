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
using Hearthstone_Deck_Tracker.Utility.MVVM;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker
{
	/// <summary>
	/// Interaction logic for DeckPickerItem.xaml
	/// </summary>
	public partial class DeckPickerItem
	{
		public DeckPickerItem()
		{
			InitializeComponent();
		}
	}

	public class DeckPickerItemViewModel : ViewModel
	{

		private const string LocUse = "DeckPicker_Deck_Label_Use";
		private const string LocActive = "DeckPicker_Deck_Label_Active";

		private readonly Type _deckPickerItem = typeof(DeckPickerItemLayout1);
		private FrameworkElement? _deckPickerItemLayout;
		public FrameworkElement? DeckPickerItemLayout => _deckPickerItemLayout ??= GetLayout();

		public DeckPickerItemViewModel(Deck deck, Type? deckPickerItemLayout = null)
		{
			Deck = deck;
			if (deckPickerItemLayout != null)
				_deckPickerItem = deckPickerItemLayout;
		}

		public Deck Deck { get; set; }

		public FontWeight FontWeightActiveDeck => Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : FontWeights.Regular;

		public FontWeight FontWeightSelected => Equals(Deck, DeckList.Instance.ActiveDeck) ? FontWeights.Bold : IsSelected ? FontWeights.SemiBold : FontWeights.Regular;

		public bool IsSelected
		{
			get => GetProp(false);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(FontWeightSelected));
			}
		}

		public string? TextUseButton => Deck.Equals(DeckList.Instance.ActiveDeck) ? LocUtil.Get(LocActive, true) : LocUtil.Get(LocUse, true);

		public Visibility HsReplayDataIndicatorVisibility => HsReplayDataManager.Decks.AvailableDecks.Contains(Deck.GetSelectedDeckVersion().ShortId) ? Visibility.Visible : Visibility.Collapsed;

		private FrameworkElement? GetLayout()
		{
			if(_deckPickerItem == typeof(DeckPickerItemLayout1))
				return new DeckPickerItemLayout1();
			if(_deckPickerItem == typeof(DeckPickerItemLayout2))
				return new DeckPickerItemLayout2();
			if(_deckPickerItem == typeof(DeckPickerItemLayoutLegacy))
				return new DeckPickerItemLayoutLegacy();
			if(_deckPickerItem == typeof(DeckPickerItemLayoutMinimal))
				return new DeckPickerItemLayoutMinimal();
			return null;
		}

		public string? DataIndicatorTooltip => LocUtil.Get("DeckCharts_Tooltip_Uploaded");

		public string? WildIndicatorTooltip => LocUtil.Get("DeckPicker_Deck_Wild_Tooltip");

		public string? ArchivedTooltip => LocUtil.Get("DeckPicker_Deck_Archived_Tooltip");

		public string? LegacyNoStatsNo => LocUtil.Get("DeckPicker_Deck_Legacy_NoStats_No");

		public string? LegacyNoStatsStats => LocUtil.Get("DeckPicker_Deck_Legacy_NoStats_Stats");

		public string? DateShownOnDeckTooltip
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
			OnPropertyChanged(nameof(FontWeightActiveDeck));
			OnPropertyChanged(nameof(TextUseButton));
			OnPropertyChanged(nameof(LastPlayed));
			OnPropertyChanged(nameof(HsReplayDataIndicatorVisibility));
			Deck.UpdateWildIndicatorVisibility();
		}

		#region sorting properties

		public string Class => Deck.GetClass;

		public string DeckId => Deck.DeckId.ToString();

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
