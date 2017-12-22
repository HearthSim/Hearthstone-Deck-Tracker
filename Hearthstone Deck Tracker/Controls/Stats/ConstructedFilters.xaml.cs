#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for ConstructedFilters.xaml
	/// </summary>
	public partial class ConstructedFilters : INotifyPropertyChanged
	{
		private readonly bool _initialized;
		private Action _updateCallback;

		public ConstructedFilters(Action updateCallback = null)
		{
			InitializeComponent();
			ComboBoxTimeframe.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboBoxTimeframe.SelectedItem = Config.Instance.ConstructedStatsTimeFrameFilter;
			ComboBoxClass.ItemsSource =
				Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ConstructedStatsClassFilter);
			ComboBoxRegion.ItemsSource = Enum.GetValues(typeof(RegionAll));
			ComboBoxRegion.SelectedItem = Config.Instance.ConstructedStatsRegionFilter;
			ComboBoxMode.ItemsSource = new[]
			{
				GameMode.All,
				GameMode.Ranked,
				GameMode.Casual,
				GameMode.Brawl,
				GameMode.Friendly,
				GameMode.Practice,
				GameMode.Spectator
			};
			ComboBoxMode.SelectedItem = Config.Instance.ConstructedStatsModeFilter;
			ComboBoxFormat.ItemsSource = Enum.GetValues(typeof(Format));
			ComboBoxFormat.SelectedItem = Config.Instance.ConstructedStatsFormatFilter;
			ComboBoxCoin.ItemsSource = Enum.GetValues(typeof(AllYesNo));
			ComboBoxCoin.SelectedItem = Config.Instance.ConstructedStatsCoinFilter;
			ComboBoxResult.ItemsSource = Enum.GetValues(typeof(GameResultAll));
			ComboBoxResult.SelectedItem = Config.Instance.ConstructedStatsResultFilter;
			ComboBoxOpponentClass.ItemsSource =
				Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxOpponentClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ConstructedStatsOpponentClassFilter);
			TextBoxOpponentName.Text = Config.Instance.ConstructedStatsOpponentNameFilter;
			TextBoxNote.Text = Config.Instance.ConstructedStatsNoteFilter;
			TextBoxTurnsMin.Text = Config.Instance.ConstructedStatsTurnsFilterMin.ToString();
			TextBoxTurnsMax.Text = Config.Instance.ConstructedStatsTurnsFilterMax.ToString();
			if(updateCallback != null)
				SetUpdateCallback(updateCallback);
			_initialized = true;
		}

		public Visibility RankFilterVisibility
			=> Config.Instance.ConstructedStatsModeFilter == GameMode.Ranked ? Visibility.Visible : Visibility.Collapsed;

		public Visibility FormatFilterVisibility
			=> Config.Instance.ConstructedStatsModeFilter == GameMode.Ranked || Config.Instance.ConstructedStatsModeFilter == GameMode.Casual
					? Visibility.Visible : Visibility.Collapsed;

		public bool ActiveDeckOnlyIsEnabled => !DeckList.Instance.ActiveDeck?.IsArenaDeck ?? false;

		public string ActiveDeckOnlyToolTip
			=>DeckList.Instance.ActiveDeck == null
					? "No active deck" : (DeckList.Instance.ActiveDeck.IsArenaDeck ? "Active deck is an arena deck" : "Deck: " + DeckList.Instance.ActiveDeck.Name);

		public event PropertyChangedEventHandler PropertyChanged;

		internal void SetUpdateCallback(Action callback)
		{
			if(_updateCallback == null)
				_updateCallback = callback;
		}

		internal void UpdateActiveDeckOnlyCheckBox()
		{
			if(Config.Instance.ConstructedStatsActiveDeckOnly && (DeckList.Instance.ActiveDeck?.IsArenaDeck ?? true))
			{
				Config.Instance.ConstructedStatsActiveDeckOnly = false;
				Config.Save();
				CheckBoxDecks.GetBindingExpression(ToggleButton.IsCheckedProperty)?.UpdateTarget();
			}
			OnPropertyChanged(nameof(ActiveDeckOnlyIsEnabled));
			OnPropertyChanged(nameof(ActiveDeckOnlyToolTip));
		}

		private void ComboBoxTimeframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsTimeFrameFilter = (DisplayedTimeFrame)ComboBoxTimeframe.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxClass.SelectedItem).HeroClass;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void DatePickerCustomTimeFrame_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
		}

		private void ComboBoxRegion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsRegionFilter = (RegionAll)ComboBoxRegion.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsModeFilter = (GameMode)ComboBoxMode.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
			OnPropertyChanged(nameof(RankFilterVisibility));
			OnPropertyChanged(nameof(FormatFilterVisibility));
		}

		private void ComboBoxCoin_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsCoinFilter = (AllYesNo)ComboBoxCoin.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void CheckBox_UpdateStats(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ComboBoxFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsFormatFilter = (Format)ComboBoxFormat.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBox_OnPreviewTextInput_DigitsOnly(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void ComboBoxResult_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsResultFilter = (GameResultAll)ComboBoxResult.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxOpponentClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsOpponentClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxOpponentClass.SelectedItem).HeroClass;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxOpponentName_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxOpponentName.Text == Config.Instance.ConstructedStatsOpponentNameFilter)
				return;
			Config.Instance.ConstructedStatsOpponentNameFilter = TextBoxOpponentName.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxNote_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxNote.Text == Config.Instance.ConstructedStatsNoteFilter)
				return;
			Config.Instance.ConstructedStatsNoteFilter = TextBoxNote.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private async void TextBoxRankMin_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxRankMin.Text == Config.Instance.ConstructedStatsRankFilterMin)
				return;
			await Task.Delay(100);
			if(Validation.GetHasError(TextBoxRankMin))
				return;
			_updateCallback?.Invoke();
		}

		private async void TextBoxRankMax_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxRankMax.Text == Config.Instance.ConstructedStatsRankFilterMax)
				return;
			await Task.Delay(100);
			if(Validation.GetHasError(TextBoxRankMax))
				return;
			_updateCallback?.Invoke();
		}

		private void TextBoxTurnsMin_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			int value;
			if(!int.TryParse(TextBoxTurnsMin.Text, out value))
				return;
			if(value == Config.Instance.ConstructedStatsTurnsFilterMin)
				return;
			Config.Instance.ConstructedStatsTurnsFilterMin = value;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxTurnsMax_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			int value;
			if(!int.TryParse(TextBoxTurnsMax.Text, out value))
				return;
			if(value == Config.Instance.ConstructedStatsTurnsFilterMax)
				return;
			Config.Instance.ConstructedStatsTurnsFilterMax = value;
			Config.Save();
			_updateCallback?.Invoke();
		}

		public void Reset()
		{
			new List<string>
			{
				nameof(Config.Instance.ConstructedStatsTimeFrameFilter),
				nameof(Config.Instance.ConstructedStatsTimeFrameCustomStart),
				nameof(Config.Instance.ConstructedStatsTimeFrameCustomEnd),
				nameof(Config.Instance.ConstructedStatsClassFilter),
				nameof(Config.Instance.ConstructedStatsRegionFilter),
				nameof(Config.Instance.ConstructedStatsIncludeArchived),
				nameof(Config.Instance.ConstructedStatsModeFilter),
				nameof(Config.Instance.ConstructedStatsRankFilterMin),
				nameof(Config.Instance.ConstructedStatsRankFilterMax),
				nameof(Config.Instance.ConstructedStatsFormatFilter),
				nameof(Config.Instance.ConstructedStatsTurnsFilterMin),
				nameof(Config.Instance.ConstructedStatsTurnsFilterMax),
				nameof(Config.Instance.ConstructedStatsCoinFilter),
				nameof(Config.Instance.ConstructedStatsResultFilter),
				nameof(Config.Instance.ConstructedStatsOpponentClassFilter),
				nameof(Config.Instance.ConstructedStatsOpponentNameFilter),
				nameof(Config.Instance.ConstructedStatsNoteFilter),
				nameof(Config.Instance.ConstructedStatsApplyTagFilters)
			}.ForEach(Config.Instance.Reset);
			Config.Save();
		}

		private void TextBox_OnEnter(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
				(sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		private async void TextBoxCustomSeasonMin_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxCustomSeasonMin.Text == Config.Instance.ConstructedStatsCustomSeasonMin.ToString())
				return;
			await Task.Delay(100);
			if(Validation.GetHasError(TextBoxCustomSeasonMin))
				return;
			_updateCallback?.Invoke();
		}

		private async void TextBoxCustomSeasonMax_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxCustomSeasonMax.Text == Config.Instance.ConstructedStatsCustomSeasonMax.ToString())
				return;
			await Task.Delay(100);
			if(Validation.GetHasError(TextBoxCustomSeasonMax))
				return;
			_updateCallback?.Invoke();
		}

		private void CheckBoxDecks_OnCheckedChanged(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
			Core.StatsOverview.ConstructedGames.UpdateAddGameButton();
		}
	}
}
