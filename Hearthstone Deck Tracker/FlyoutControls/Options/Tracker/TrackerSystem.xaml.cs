#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerSystem.xaml
	/// </summary>
	public partial class TrackerSystem
	{
		private bool _initialized;

		public TrackerSystem()
		{
			InitializeComponent();
#if(SQUIRREL)
			CheckboxConfigSaveAppData.Visibility = Visibility.Collapsed;
			CheckboxDataSaveAppData.Visibility = Visibility.Collapsed;
			SelectSaveDataPath.Visibility = Visibility.Collapsed;
#else
			CheckboxCheckForUpdates.Visibility = Visibility.Collapsed;
#endif
		}

		public void Load()
		{
			ComboBoxLanguage.ItemsSource = Enum.GetValues(typeof(Language));
			ComboBoxLanguage.SelectedItem = Config.Instance.Localization;

			CheckboxCloseTray.IsChecked = Config.Instance.CloseToTray;
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			CheckboxStartHearthstoneWithHDT.IsChecked = Config.Instance.StartHearthstoneWithHDT;
			CheckboxAdvancedWindowSearch.IsChecked = Config.Instance.UseAnyUnityWindow;
			CheckBoxShowSplashScreen.IsChecked = Config.Instance.ShowSplashScreen;
			CheckboxStartWithWindows.IsChecked = Config.Instance.StartWithWindows;
			CheckBoxAnalytics.IsChecked = Config.Instance.GoogleAnalytics;

			CheckboxAlternativeScreenCapture.IsChecked = Config.Instance.AlternativeScreenCapture;
			CheckboxHardwareAcceleration.IsChecked = Config.Instance.UseHardwareAcceleration;
#if(!SQUIRREL)
			CheckboxConfigSaveAppData.IsChecked = Config.Instance.SaveConfigInAppData;
			CheckboxDataSaveAppData.IsChecked = Config.Instance.SaveDataInAppData;
#endif

			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.MinimizeToTray = true;
			SaveConfig(false);
			SyncMainWindowOptions();
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
			SyncMainWindowOptions();
		}

		private void CheckboxCloseTray_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.CloseToTray = true;
			SaveConfig(false);
			SyncMainWindowOptions();
		}

		private void CheckboxCloseTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.CloseToTray = false;
			SaveConfig(false);
			SyncMainWindowOptions();
		}

		// the tray options also appear in the main window panel
		private void SyncMainWindowOptions()
		{
			if(Helper.OptionsMain is not { } options)
				return;
			options.OptionsTrackerMainWindow.CheckboxCloseTray.IsChecked = Config.Instance.CloseToTray;
			options.OptionsTrackerMainWindow.CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
		}

		private void CheckboxStartMinimized_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StartMinimized = true;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StartMinimized = false;
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CheckForUpdates = true;
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CheckForUpdates = false;
			SaveConfig(false);
		}

		private void CheckboxCloseWithHearthstone_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CloseWithHearthstone = true;
			Config.Save();
		}

		private void CheckboxCloseWithHearthstone_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CloseWithHearthstone = false;
			Config.Save();
		}

		private void CheckboxStartHearthstoneWithHDT_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.StartHearthstoneWithHDT = true;
			Config.Save();
		}

		private void CheckboxStartHearthstoneWithHDT_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.StartHearthstoneWithHDT = false;
			Config.Save();
		}

		private async void CheckboxConfigSaveAppData_Checked(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			if(this.ParentMainWindow() is { } window)
				await window.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.RestartApplication();
#endif
		}

		private async void CheckboxConfigSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			if(this.ParentMainWindow() is { } window)
				await window.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.RestartApplication();
#endif
		}

		private async void CheckboxDataSaveAppData_Checked(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = true;
			Config.Save();
			if(this.ParentMainWindow() is { } window)
				await window.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.RestartApplication();
#endif
		}

		private async void CheckboxDataSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = false;
			Config.Save();
			if(this.ParentMainWindow() is { } window)
				await window.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.RestartApplication();
#endif
		}

		private void CheckboxAdvancedWindowSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseAnyUnityWindow = true;
			Config.Save();
		}

		private void CheckboxAdvancedWindowSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseAnyUnityWindow = false;
			Config.Save();
		}

		private void ButtonGamePath_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog {Description = "Select your Hearthstone Directory", ShowNewFolderButton = false};
			var dialogResult = dialog.ShowDialog();

			if(dialogResult == DialogResult.OK)
			{
				Config.Instance.HearthstoneDirectory = dialog.SelectedPath;
				Config.Save();
				if(this.ParentMainWindow() is { } window)
					window.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
			}
		}

		private async void SelectSaveDataPath_Click(object sender, RoutedEventArgs e)
		{
#if(!SQUIRREL)
			var dialog = new FolderBrowserDialog();
			var dialogResult = dialog.ShowDialog();

			if(dialogResult == DialogResult.OK)
			{
				var saveInAppData = Config.Instance.SaveDataInAppData.HasValue && Config.Instance.SaveDataInAppData.Value;
				if(!saveInAppData)
				{
					foreach(var value in new List<bool> {true, false})
					{
						Config.Instance.SaveDataInAppData = value;
						Helper.CopyReplayFiles();
						DeckStatsList.SetupDeckStatsFile();
						DeckList.SetupDeckListFile();
						DefaultDeckStats.SetupDefaultDeckStatsFile();
						Config.Instance.DataDirPath = dialog.SelectedPath;
					}
				}
				Config.Instance.DataDirPath = dialog.SelectedPath;
				Config.Save();
				if(!saveInAppData)
				{
					if(this.ParentMainWindow() is { } window)
						await window.ShowMessage("Restart required.", "Click ok to restart HDT");
					Core.RestartApplication();
				}
			}
#endif
		}

		private void ButtonOpenAppData_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start(Config.AppDataPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				ErrorManager.AddError("Could not open AppData folder.", "Manually navigate to '%AppData%/HearthstoneDeckTracker'.");
			}
		}

		private void CheckboxStartWithWindows_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			RegistryHelper.SetRunKey();
			Config.Instance.StartWithWindows = true;
			Config.Save();
		}

		private void CheckboxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			RegistryHelper.DeleteRunKey();
			Config.Instance.StartWithWindows = false;
			Config.Save();
		}

		private void CheckboxShowSplashScreen_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSplashScreen = true;
			Config.Save();
		}

		private void CheckboxShowSplashScreen_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSplashScreen = false;
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
			Core.Overlay.RefreshCenteredBgsPanels();
			if(Config.Instance.LastSeenHearthstoneLang == null)
				Helper.UpdateCardLanguage();
		}

		private void UpdateUIAfterChangeLanguage()
		{
			// Options
			if(Helper.OptionsMain != null)
				Helper.OptionsMain.ContentHeader = LocUtil.Get("Options_Tracker_System_Header");

			// TrayIcon
			Core.TrayIcon.MenuItemStartHearthstone.Text = LocUtil.Get("TrayIcon_MenuItemStartHearthstone");
			Core.TrayIcon.MenuItemUseNoDeck.Text = LocUtil.Get("TrayIcon_MenuItemUseNoDeck");
			Core.TrayIcon.MenuItemShow.Text = LocUtil.Get("TrayIcon_MenuItemShow");
			Core.TrayIcon.MenuItemSettings.Text = LocUtil.Get("TrayIcon_MenuItemSettings");
			Core.TrayIcon.MenuItemQuit.Text = LocUtil.Get("TrayIcon_MenuItemQuit");

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

		private void CheckBoxAnalytics_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GoogleAnalytics = true;
			Config.Save();
		}

		private void CheckBoxAnalytics_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GoogleAnalytics = false;
			Config.Save();
		}

		private void CheckboxAlternativeScreenCapture_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlternativeScreenCapture = true;
			Config.Save();
		}

		private void CheckboxAlternativeScreenCapture_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlternativeScreenCapture = false;
			Config.Save();
		}

		private void CheckboxHardwareAcceleration_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseHardwareAcceleration = true;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private void CheckboxHardwareAcceleration_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseHardwareAcceleration = false;
			Config.Save();
			this.ParentMainWindow()?.ShowRestartDialog();
		}

		private async void ButtonHearthstoneLogsDirectory_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog();
			dialog.SelectedPath = Config.Instance.HearthstoneDirectory;
			var dialogResult = dialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
			{
				//Logs directory needs to be a child directory in Hearthstone directory
				if (!dialog.SelectedPath.StartsWith(Config.Instance.HearthstoneDirectory + @"\"))
				{
					if(this.ParentMainWindow() is { } window)
						await window.ShowMessage("Invalid argument", "Selected directory not in Hearthstone directory!");
					return;
				}

				//Check if same path selected (no restart required)
				if (Config.Instance.HearthstoneLogsDirectoryName.Equals(dialog.SelectedPath))
					return;

				Config.Instance.HearthstoneLogsDirectoryName = dialog.SelectedPath.Remove(0, Config.Instance.HearthstoneDirectory.Length + 1);
				Config.Save();

				if(this.ParentMainWindow() is { } window2)
					await window2.ShowMessage("Restart required.", "Click ok to restart HDT");
				Core.RestartApplication();
			}
		}

		private void ButtonDebugWindow_Click(object sender, RoutedEventArgs e)
		{
			new DebugWindow(Core.Game).Show();
		}
	}
}
