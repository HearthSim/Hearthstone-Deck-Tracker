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
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
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

		private void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = true;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = false;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
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

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

		private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.Localization = (Language)ComboBoxLanguage.SelectedItem;
			Config.Save();
			LocUtil.UpdateCultureInfo();
			UpdateUIAfterChangeLanguage();
			Core.Overlay.UpdateBgsChinaModulePanel();
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
			Core.TrayIcon.MenuItemShow.Text = LocUtil.Get("TrayIcon_MenuItemShow");
			Core.TrayIcon.MenuItemExit.Text = LocUtil.Get("TrayIcon_MenuItemExit");

			if(this.ParentMainWindow() is { } window)
			{
				// My Games Panel
				window.DeckCharts.ReloadUI();

				// Deck Picker
				window.DeckPickerList.ReloadUI();

				//Overlay Panel
				window.Options.OptionsOverlayPlayer.ReloadUI();
				window.Options.OptionsOverlayOpponent.ReloadUI();
			}

			// Reload ComboBoxes
			ComboBoxHelper.Update();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
