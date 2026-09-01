#region

using System;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerMainWindow.xaml
	/// </summary>
	public partial class TrackerMainWindow : UserControl
	{
		private bool _initialized;

		public TrackerMainWindow()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxShowMyGamesPanel.IsChecked = Config.Instance.ShowMyGamesPanel;
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;
			ComboBoxDeckDateType.ItemsSource = Enum.GetValues(typeof(DeckDateType));
			ComboBoxDeckDateType.SelectedItem = Config.Instance.SelectedDateOnDecks;
			ComboBoxDateFormat.ItemsSource = Enum.GetValues(typeof(DateFormat));
			ComboBoxDateFormat.SelectedItem = Config.Instance.SelectedDateFormat;
			DateFormatPanel.Visibility = Config.Instance.ShowDateOnDeck ? Visibility.Visible : Visibility.Collapsed;
			CheckBoxAutoUse.IsChecked = Config.Instance.AutoUseDeck;
			CheckBoxAutoArchiveArenaDecks.IsChecked = Config.Instance.AutoArchiveArenaDecks;
			CheckboxCloseTray.IsChecked = Config.Instance.CloseToTray;
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;

			CheckboxShowNewsBar.IsChecked = null;

			ConfigWrapper.IgnoreNewsIdChanged += () =>
			{
				CheckboxShowNewsBar.IsChecked = ConfigWrapper.IgnoreNewsId == -1;
			};
			Remote.Config.Loaded += data =>
			{
				CheckboxShowNewsBar.IsChecked = Config.Instance.IgnoreNewsId < data?.News?.Id;
			};

			_initialized = true;
		}

		private void CheckboxShowMyGamesPanel_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMyGamesPanel = true;
			this.ParentMainWindow()?.UpdateMyGamesPanelVisibility();
			Config.Save();
		}

		private void CheckboxShowMyGamesPanel_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMyGamesPanel = false;
			this.ParentMainWindow()?.UpdateMyGamesPanelVisibility();
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = true;
			if(this.ParentMainWindow() is {} window)
				window.ManaCurveMyDecks.Visibility = Visibility.Visible;
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = false;
			if(this.ParentMainWindow() is {} window)
				window.ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			Config.Save();
		}

		private void CheckboxDeckPickerCaps_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = true;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckboxDeckPickerCaps_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = false;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void ComboBoxDatesOnDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedDateOnDecks = (DeckDateType)ComboBoxDeckDateType.SelectedItem;
			Config.Instance.ShowDateOnDeck = (Config.Instance.SelectedDateOnDecks != DeckDateType.None) ? true : false;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void ComboBoxDateFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedDateFormat = (DateFormat)ComboBoxDateFormat.SelectedItem;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckboxShowNewsBar_OnClick(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			ConfigWrapper.IgnoreNewsId = ConfigWrapper.IgnoreNewsId == -1
				? Remote.Config.Data?.News?.Id ?? 0 : -1;
		}

		private void CheckboxCloseTray_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CloseToTray = true;
			Config.Save();
			SyncSystemOptions();
		}

		private void CheckboxCloseTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CloseToTray = false;
			Config.Save();
			SyncSystemOptions();
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.MinimizeToTray = true;
			Config.Save();
			SyncSystemOptions();
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.MinimizeToTray = false;
			Config.Save();
			SyncSystemOptions();
		}

		private void CheckBoxAutoUse_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = true;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckBoxAutoUse_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = false;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckBoxAutoArchiveArenaDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoArchiveArenaDecks = true;
			Config.Save();
		}

		private void CheckBoxAutoArchiveArenaDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoArchiveArenaDecks = false;
			Config.Save();
		}

		// the tray options also appear in the system panel
		private void SyncSystemOptions()
		{
			if(Helper.OptionsMain is not { } options)
				return;
			options.OptionsTrackerSystem.CheckboxCloseTray.IsChecked = Config.Instance.CloseToTray;
			options.OptionsTrackerSystem.CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
		}
	}
}
