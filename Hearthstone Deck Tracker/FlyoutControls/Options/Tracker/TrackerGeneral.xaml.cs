#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerGeneral.xaml
	/// </summary>
	public partial class TrackerGeneral : INotifyPropertyChanged
	{
		private bool _initialized;

		public TrackerGeneral()
		{
			InitializeComponent();
		}

		public void Load()
		{
			ComboBoxLanguage.ItemsSource = Enum.GetValues(typeof(Language));
			ComboBoxLanguage.SelectedItem = Config.Instance.Localization;

			CheckBoxAutoUse.IsChecked = Config.Instance.AutoUseDeck;
			CheckBoxAutoDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckBoxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;
			ComboBoxDeckDateType.ItemsSource = Enum.GetValues(typeof(DeckDateType));
			ComboBoxDeckDateType.SelectedItem = Config.Instance.SelectedDateOnDecks;
			ComboBoxDateFormat.ItemsSource = Enum.GetValues(typeof(DateFormat));
			ComboBoxDateFormat.SelectedItem = Config.Instance.SelectedDateFormat;
			DateFormatPanel.Visibility = Config.Instance.ShowDateOnDeck ? Visibility.Visible : Visibility.Collapsed;
			CheckboxShowMyGamesPanel.IsChecked = Config.Instance.ShowMyGamesPanel;
			CheckBoxAutoArchiveArenaDecks.IsChecked = Config.Instance.AutoArchiveArenaDecks;

			_initialized = true;
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = true;
			Core.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Visible;
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = false;
			Core.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			Config.Save();
		}

		private void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = true;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = false;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckboxFullTextSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = true;
			Config.Save();
		}

		private void CheckboxFullTextSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = false;
			Config.Save();
		}

		private void CheckBoxAutoUse_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = true;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckBoxAutoUse_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = false;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckBoxClassCardsFirst_Checked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(true);

		private void CheckBoxClassCardsFirst_Unchecked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(false);

		private void ComboBoxDatesOnDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedDateOnDecks = (DeckDateType)ComboBoxDeckDateType.SelectedItem;
			Config.Instance.ShowDateOnDeck = (Config.Instance.SelectedDateOnDecks != DeckDateType.None) ? true : false;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void ComboBoxDateFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedDateFormat = (DateFormat)ComboBoxDateFormat.SelectedItem;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
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

		private void CheckboxDeckPickerCaps_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = true;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckboxDeckPickerCaps_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = false;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void CheckBoxAutoDeckDetecion_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoDeckDetection = true;
			Config.Save();
			Core.MainWindow.AutoDeckDetection(true);
		}

		private void CheckBoxAutoDeckDetection_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoDeckDetection = false;
			Config.Save();
			Core.MainWindow.AutoDeckDetection(false);
		}

		private void CheckboxShowMyGamesPanel_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMyGamesPanel = true;
			Core.MainWindow.UpdateMyGamesPanelVisibility();
			Config.Save();
		}

		private void CheckboxShowMyGamesPanel_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMyGamesPanel = false;
			Core.MainWindow.UpdateMyGamesPanelVisibility();
			Config.Save();
		}

		private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.Localization = (Language)ComboBoxLanguage.SelectedItem;
			Config.Save();
			LocUtil.UpdateCultureInfo();
			UpdateUIAfterChangeLanguage();
			if(Config.Instance.LastSeenHearthstoneLang == null)
				Helper.UpdateCardLanguage();
		}

		private void UpdateUIAfterChangeLanguage()
		{
			// Options
			if(Helper.OptionsMain != null)
				Helper.OptionsMain.ContentHeader = LocUtil.Get("Options_Tracker_General_Header");

			// TrayIcon
			Core.TrayIcon.MenuItemStartHearthstone.Text = LocUtil.Get("TrayIcon_MenuItemStartHearthstone");
			Core.TrayIcon.MenuItemUseNoDeck.Text = LocUtil.Get("TrayIcon_MenuItemUseNoDeck");
			Core.TrayIcon.MenuItemAutoSelect.Text = LocUtil.Get("TrayIcon_MenuItemAutoSelect");
			Core.TrayIcon.MenuItemClassCardsFirst.Text = LocUtil.Get("TrayIcon_MenuItemClassCardsFirst");
			Core.TrayIcon.MenuItemShow.Text = LocUtil.Get("TrayIcon_MenuItemShow");
			Core.TrayIcon.MenuItemExit.Text = LocUtil.Get("TrayIcon_MenuItemExit");

			// My Games Panel
			Core.MainWindow.DeckCharts.ReloadUI();

			// Deck Picker
			Core.MainWindow.DeckPickerList.ReloadUI();

			//Overlay Panel
			Core.MainWindow.Options.OptionsOverlayPlayer.ReloadUI();
			Core.MainWindow.Options.OptionsOverlayOpponent.ReloadUI();

			// Reload ComboBoxes
			ComboBoxHelper.Update();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
