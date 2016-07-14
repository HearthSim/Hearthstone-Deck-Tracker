#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using Microsoft.Win32;
using Application = System.Windows.Application;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for OtherTracker.xaml
	/// </summary>
	public partial class TrackerSettings
	{
		private bool _initialized;

		public TrackerSettings()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			CheckboxCheckForBetaUpdates.IsChecked = Config.Instance.CheckForBetaUpdates;
			CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			CheckboxStartHearthstoneWithHDT.IsChecked = Config.Instance.StartHearthstoneWithHDT;
			CheckboxConfigSaveAppData.IsChecked = Config.Instance.SaveConfigInAppData;
			CheckboxDataSaveAppData.IsChecked = Config.Instance.SaveDataInAppData;
			CheckboxAdvancedWindowSearch.IsChecked = Config.Instance.UseAnyUnityWindow;
			CheckboxLogTab.IsChecked = Config.Instance.ShowLogTab;
			CheckBoxShowLoginDialog.IsChecked = Config.Instance.ShowLoginDialog;
			CheckBoxShowSplashScreen.IsChecked = Config.Instance.ShowSplashScreen;
			CheckboxStartWithWindows.IsChecked = Config.Instance.StartWithWindows;
			CheckBoxAnalytics.IsChecked = Config.Instance.GoogleAnalytics;
			CheckboxAlternativeScreenCapture.IsChecked = Config.Instance.AlternativeScreenCapture;

			_initialized = true;
		}

		private void TrackerSettings_Loaded(object sender, RoutedEventArgs e)
		{
			CheckboxShowNewsBar.IsChecked = Core.MainWindow.StatusBarNews.Visibility != Visibility.Collapsed;
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
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
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
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.MainWindow.Restart();
		}

		private async void CheckboxConfigSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.MainWindow.Restart();
		}

		private async void CheckboxDataSaveAppData_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = true;
			Config.Save();
			await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.MainWindow.Restart();
		}

		private async void CheckboxDataSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = false;
			Config.Save();
			await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
			Core.MainWindow.Restart();
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

		private void CheckboxLogTab_Checked(object sender, RoutedEventArgs e)
		{
			Helper.OptionsMain.TreeViewItemTrackerLogging.Visibility = Visibility.Visible;
			if(!_initialized)
				return;
			Config.Instance.ShowLogTab = true;
			Config.Save();
		}

		private void CheckboxLogTab_Unchecked(object sender, RoutedEventArgs e)
		{
			Helper.OptionsMain.TreeViewItemTrackerLogging.Visibility = Visibility.Collapsed;
			if(!_initialized)
				return;
			Config.Instance.ShowLogTab = false;
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
				Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
			}
		}

		private async void SelectSaveDataPath_Click(object sender, RoutedEventArgs e)
		{
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
					await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
					Core.MainWindow.Restart();
				}
			}
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
			var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			regKey?.SetValue("Hearthstone Deck Tracker", Application.ResourceAssembly.Location);
			Config.Instance.StartWithWindows = true;
			Config.Save();
		}

		private void CheckboxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			regKey?.DeleteValue("Hearthstone Deck Tracker", false);
			Config.Instance.StartWithWindows = false;
			Config.Save();
		}

		private void CheckboxCheckForBetaUpdates_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CheckForBetaUpdates = true;
			Config.Save();
		}

		private void CheckboxCheckForBetaUpdates_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CheckForBetaUpdates = false;
			Config.Save();
		}

		private void CheckboxShowLoginDialog_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowLoginDialog = true;
			Config.Save();
		}

		private void CheckboxShowLoginDialog_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowLoginDialog = false;
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

		private void ButtonRestart_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.Restart();
		}

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

		private void CheckboxShowNewsBar_OnClick(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Utility.NewsUpdater.ToggleNewsVisibility();
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
					await Core.MainWindow.ShowMessage("Invalid argument", "Selected directory not in Hearthstone directory!");
					return;
				}

				//Check if same path selected (no restart required)
				if (Config.Instance.HearthstoneLogsDirectoryName.Equals(dialog.SelectedPath))
					return;

				Config.Instance.HearthstoneLogsDirectoryName = dialog.SelectedPath.Remove(0, Config.Instance.HearthstoneDirectory.Length + 1);
				Config.Save();

				await Core.MainWindow.ShowMessage("Restart required.", "Click ok to restart HDT");
				Core.MainWindow.Restart();
			}
		}
	}
}