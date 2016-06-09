#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherStats.xaml
	/// </summary>
	public partial class TrackerStats
	{
		private bool _initialized;

		public TrackerStats()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxRecordArena.IsChecked = Config.Instance.RecordArena;
			CheckboxRecordBrawl.IsChecked = Config.Instance.RecordBrawl;
			CheckboxRecordCasual.IsChecked = Config.Instance.RecordCasual;
			CheckboxRecordFriendly.IsChecked = Config.Instance.RecordFriendly;
			CheckboxRecordOther.IsChecked = Config.Instance.RecordOther;
			CheckboxRecordPractice.IsChecked = Config.Instance.RecordPractice;
			CheckboxRecordRanked.IsChecked = Config.Instance.RecordRanked;
			CheckboxDiscardGame.IsChecked = Config.Instance.DiscardGameIfIncorrectDeck;
			CheckboxAskBeforeDiscarding.IsChecked = Config.Instance.AskBeforeDiscardingGame;
			CheckboxAskBeforeDiscarding.IsEnabled = Config.Instance.DiscardGameIfIncorrectDeck;
			CheckboxRecordSpectator.IsChecked = Config.Instance.RecordSpectator;
			CheckboxDiscardZeroTurnGame.IsChecked = Config.Instance.DiscardZeroTurnGame;
			CheckboxDeleteDeckKeepStats.IsChecked = Config.Instance.KeepStatsWhenDeletingDeck;
			CheckboxStatsInWindow.IsChecked = Config.Instance.StatsInWindow;
			CheckboxReplays.IsChecked = Config.Instance.RecordReplays;
			ComboboxDisplayedStats.ItemsSource = Enum.GetValues(typeof(DisplayedStats));
			ComboboxDisplayedMode.ItemsSource = Enum.GetValues(typeof(GameMode));
			ComboboxDisplayedTimeFrame.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboboxDisplayedStats.SelectedItem = Config.Instance.DisplayedStats;
			ComboboxDisplayedMode.SelectedItem = Config.Instance.DisplayedMode;
			ComboboxDisplayedTimeFrame.SelectedItem = Config.Instance.DisplayedTimeFrame;
			PanelCustomTimeFrame.Visibility = Config.Instance.DisplayedTimeFrame == DisplayedTimeFrame.Custom
				                                  ? Visibility.Visible : Visibility.Collapsed;
			DatePickerCustomTimeFrame.SelectedDate = Config.Instance.CustomDisplayedTimeFrame;
			_initialized = true;
		}

		private void CheckboxRecordRanked_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordRanked = true;
			Config.Save();
		}

		private void CheckboxRecordRanked_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordRanked = false;
			Config.Save();
		}

		private void CheckboxRecordArena_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordArena = true;
			Config.Save();
		}

		private void CheckboxRecordArena_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordArena = false;
			Config.Save();
		}

		private void CheckboxRecordBrawl_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordBrawl = true;
			Config.Save();
		}

		private void CheckboxRecordBrawl_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordBrawl = false;
			Config.Save();
		}

		private void CheckboxRecordCasual_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordCasual = true;
			Config.Save();
		}

		private void CheckboxRecordCasual_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordCasual = false;
			Config.Save();
		}

		private void CheckboxRecordFriendly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordFriendly = true;
			Config.Save();
		}

		private void CheckboxRecordFriendly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordFriendly = false;
			Config.Save();
		}

		private void CheckboxRecordPractice_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordPractice = true;
			Config.Save();
		}

		private void CheckboxRecordPractice_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordPractice = false;
			Config.Save();
		}

		private void CheckboxRecordOther_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordOther = true;
			Config.Save();
		}

		private void CheckboxRecordOther_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordOther = false;
			Config.Save();
		}

		private void CheckboxDiscardGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardGameIfIncorrectDeck = true;
			CheckboxAskBeforeDiscarding.IsEnabled = true;
			Config.Save();
		}

		private void CheckboxDiscardGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardGameIfIncorrectDeck = false;
			CheckboxAskBeforeDiscarding.IsEnabled = false;
			Config.Save();
		}

		private void CheckboxDiscardZeroTurnGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardZeroTurnGame = true;
			Config.Save();
		}

		private void CheckboxDiscardZeroTurnGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DiscardZeroTurnGame = false;
			Config.Save();
		}

		private void CheckboxRecordSpectator_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordSpectator = true;
			Config.Save();
		}

		private void CheckboxRecordSpectator_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordSpectator = false;
			Config.Save();
		}

		private void CheckboxRecordReplays_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordReplays = true;
			Config.Save();
		}

		private void CheckboxRecordReplays_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RecordReplays = false;
			Config.Save();
		}

		private void CheckboxDeleteDeckKeepStats_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepStatsWhenDeletingDeck = true;
			Config.Save();
		}

		private void CheckboxDeleteDeckKeepStats_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepStatsWhenDeletingDeck = false;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsInWindow = true;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsInWindow = false;
			Config.Save();
		}

		private void ComboboxDisplayedStats_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DisplayedStats = (DisplayedStats)ComboboxDisplayedStats.SelectedItem;
			Config.Save();
			foreach(var deck in DeckList.Instance.Decks)
				deck.StatsUpdated();
			Core.Overlay.Update(true);
		}

		private void ComboboxGameMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DisplayedMode = (GameMode)ComboboxDisplayedMode.SelectedItem;
			Config.Save();
			foreach(var deck in DeckList.Instance.Decks)
				deck.StatsUpdated();
			Core.Overlay.Update(true);
		}

		private void ComboboxDisplayedTimeFrame_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DisplayedTimeFrame = (DisplayedTimeFrame)ComboboxDisplayedTimeFrame.SelectedItem;
			Config.Save();
			foreach(var deck in DeckList.Instance.Decks)
				deck.StatsUpdated();
			Core.Overlay.Update(true);
			PanelCustomTimeFrame.Visibility = Config.Instance.DisplayedTimeFrame == DisplayedTimeFrame.Custom
				                                  ? Visibility.Visible : Visibility.Collapsed;
		}

		private void DatePickerCustomTimeFrame_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CustomDisplayedTimeFrame = DatePickerCustomTimeFrame.SelectedDate;
			Config.Save();
			foreach(var deck in DeckList.Instance.Decks)
				deck.StatsUpdated();
			Core.Overlay.Update(true);
		}

		private void CheckboxAskBeforeDiscarding_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AskBeforeDiscardingGame = true;
			Config.Save();
		}

		private void CheckboxAskBeforeDiscarding_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AskBeforeDiscardingGame = false;
			Config.Save();
		}
	}
}