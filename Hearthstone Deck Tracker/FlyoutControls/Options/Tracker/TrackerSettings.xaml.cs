#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using Microsoft.Win32;
using Application = System.Windows.Application;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

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
			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;
			ComboboxKeyPressGameStart.ItemsSource = Helper.MainWindow.EventKeys;
			ComboboxKeyPressGameEnd.ItemsSource = Helper.MainWindow.EventKeys;
			ComboBoxDeckLayout.ItemsSource = Enum.GetValues(typeof(DeckLayout));
			ComboBoxIconSet.ItemsSource = Enum.GetValues(typeof(IconStyle));

			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			CheckboxCheckForBetaUpdates.IsChecked = Config.Instance.CheckForBetaUpdates;
			CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			CheckboxConfigSaveAppData.IsChecked = Config.Instance.SaveConfigInAppData;
			CheckboxDataSaveAppData.IsChecked = Config.Instance.SaveDataInAppData;
			CheckboxAdvancedWindowSearch.IsChecked = Config.Instance.AdvancedWindowSearch;
			CheckboxLogTab.IsChecked = Config.Instance.ShowLogTab;
			CheckBoxShowLoginDialog.IsChecked = Config.Instance.ShowLoginDialog;
			CheckboxStartWithWindows.IsChecked = Config.Instance.StartWithWindows;
            CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;

			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

			if(!Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";
			ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if(!Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";
			ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			var theme = string.IsNullOrEmpty(Config.Instance.ThemeName)
				            ? ThemeManager.DetectAppStyle().Item1 : ThemeManager.AppThemes.First(t => t.Name == Config.Instance.ThemeName);
			var accent = string.IsNullOrEmpty(Config.Instance.AccentName)
				             ? ThemeManager.DetectAppStyle().Item2 : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);
			ComboboxTheme.SelectedItem = theme;
			ComboboxAccent.SelectedItem = accent;

			ComboBoxIconSet.SelectedItem = Config.Instance.ClassIconStyle;
			ComboBoxDeckLayout.SelectedItem = Config.Instance.DeckPickerItemLayout;

			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Helper.MainWindow.Overlay.Update(true);
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

		private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var accent = ComboboxAccent.SelectedItem as Accent;
			if(accent != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.DetectAppStyle().Item1);
				Config.Instance.AccentName = accent.Name;
				SaveConfig(false);
			}
		}

		private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if(theme != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
				Config.Instance.ThemeName = theme.Name;
				//if(ComboboxWindowBackground.SelectedItem.ToString() != "Default")
				Helper.OptionsMain.OptionsOverlayDeckWindows.UpdateAdditionalWindowsBackground();
				SaveConfig(false);
			}
		}

		private void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var language = ComboboxLanguages.SelectedValue.ToString();
			if(!Helper.LanguageDict.ContainsKey(language))
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			if(!File.Exists(string.Format("Files/cardDB.{0}.xml", selectedLanguage)))
				return;

			Config.Instance.SelectedLanguage = selectedLanguage;
			Config.Save();


			Helper.MainWindow.Restart();
		}

		private void ComboboxKeyPressGameStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeyPressOnGameStart = ComboboxKeyPressGameStart.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeyPressOnGameEnd = ComboboxKeyPressGameEnd.SelectedValue.ToString();
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

		private void CheckboxConfigSaveAppData_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			Helper.MainWindow.Restart();
		}

		private void CheckboxConfigSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveConfigInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			Helper.MainWindow.Restart();
		}

		private void CheckboxDataSaveAppData_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = true;
			Config.Save();
			Helper.MainWindow.Restart();
		}

		private void CheckboxDataSaveAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SaveDataInAppData = false;
			Config.Save();
			Helper.MainWindow.Restart();
		}

		private void CheckboxAdvancedWindowSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AdvancedWindowSearch = true;
			Config.Save();
		}

		private void CheckboxAdvancedWindowSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AdvancedWindowSearch = false;
			Config.Save();
		}

		private void CheckboxLogTab_Checked(object sender, RoutedEventArgs e)
		{
			Helper.OptionsMain.TreeViewItemTrackerLogging.Visibility = Visibility.Visible;
			//TabItemLog.Visibility = Visibility.Visible;
			if(!_initialized)
				return;
			Config.Instance.ShowLogTab = true;
			Config.Save();
		}

		private void CheckboxLogTab_Unchecked(object sender, RoutedEventArgs e)
		{
			Helper.OptionsMain.TreeViewItemTrackerLogging.Visibility = Visibility.Collapsed;
			//TabItemLog.Visibility = Visibility.Hidden;
			if(!_initialized)
				return;
			Config.Instance.ShowLogTab = false;
			Config.Save();
		}

		private void ButtonGamePath_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Title = "Select Hearthstone.exe",
				DefaultExt = "Hearthstone.exe",
				Filter = "Hearthstone.exe|Hearthstone.exe"
			};
			var dialogResult = dialog.ShowDialog();

			if(dialogResult == DialogResult.OK)
			{
				Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
				Config.Save();
				Helper.MainWindow.Restart();
			}
		}

		private void SelectSaveDataPath_Click(object sender, RoutedEventArgs e)
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
						Helper.MainWindow.CopyReplayFiles();
						Helper.MainWindow.SetupDeckStatsFile();
						Helper.MainWindow.SetupDeckListFile();
						Helper.MainWindow.SetupDefaultDeckStatsFile();
						Config.Instance.DataDirPath = dialog.SelectedPath;
					}
				}
				Config.Instance.DataDirPath = dialog.SelectedPath;
				Config.Save();
				if(!saveInAppData)
					Helper.MainWindow.Restart();
			}
		}

		private void ButtonOpenAppData_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start(Config.Instance.AppDataPath);
		}

		private void CheckboxStartWithWindows_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if(regKey != null)
				regKey.SetValue("Hearthstone Deck Tracker", Application.ResourceAssembly.Location);
			Config.Instance.StartWithWindows = true;
			Config.Save();
		}

		private void CheckboxStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if(regKey != null)
				regKey.DeleteValue("Hearthstone Deck Tracker", false);
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

		private void ComboboxIconSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ClassIconStyle = (IconStyle)ComboBoxIconSet.SelectedItem;
			Config.Save();
			Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for the new iconset to be loaded.");
		}

		private void ComboboxDeckLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerItemLayout = (DeckLayout)ComboBoxDeckLayout.SelectedItem;
			Config.Save();
			Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for the new layout to be loaded.");
		}

		private void ButtonRestart_OnClick(object sender, RoutedEventArgs e)
		{
			Helper.MainWindow.Restart();
		}

        private void CheckboxDeckPickerCaps_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            Config.Instance.DeckPickerCaps = true;
            Config.Save();
            Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.");
        }

        private void CheckboxDeckPickerCaps_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            Config.Instance.DeckPickerCaps = false;
            Config.Save();
            Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.");
        }
	}
}