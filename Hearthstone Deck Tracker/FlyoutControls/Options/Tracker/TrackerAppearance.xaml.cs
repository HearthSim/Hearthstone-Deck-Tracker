#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerAppearance.xaml
	/// </summary>
	public partial class TrackerAppearance : UserControl
	{
		private bool _initialized;

		public TrackerAppearance()
		{
			InitializeComponent();
		}

		public void Load()
		{
			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = Enum.GetValues(typeof(MetroTheme));
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;
			ComboBoxDeckLayout.ItemsSource = Enum.GetValues(typeof(DeckLayout));
			ComboBoxIconSet.ItemsSource = Enum.GetValues(typeof(IconStyle));
			ComboBoxClassColors.ItemsSource = Enum.GetValues(typeof(ClassColorScheme));
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;
			CheckboxUseAnimations.IsChecked = Config.Instance.UseAnimations;
			ComboBoxLastPlayedDateFormat.ItemsSource = Enum.GetValues(typeof(LastPlayedDateFormat));

			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

			ComboboxTheme.SelectedItem = Config.Instance.ThemeName;
			ComboboxAccent.SelectedItem = Helper.GetAppAccent();

			ComboBoxIconSet.SelectedItem = Config.Instance.ClassIconStyle;
			ComboBoxDeckLayout.SelectedItem = Config.Instance.DeckPickerItemLayout;
			ComboBoxClassColors.SelectedItem = Config.Instance.ClassColorScheme;
			CheckBoxArenaStatsTextColoring.IsChecked = Config.Instance.ArenaStatsTextColoring;
			CheckBoxShowLastPlayedDate.IsChecked = Config.Instance.ShowLastPlayedDateOnDeck;
			ComboBoxLastPlayedDateFormat.SelectedItem = Config.Instance.LastPlayedDateFormat;

			if(Config.Instance.NonLatinUseDefaultFont == null)
			{
				Config.Instance.NonLatinUseDefaultFont = Helper.IsWindows10();
				Config.Save();
			}
			CheckBoxDefaultFont.IsChecked = Config.Instance.NonLatinUseDefaultFont;

			_initialized = true;
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
				Config.Save();
			}
		}

		private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ThemeName = (MetroTheme)ComboboxTheme.SelectedItem;
			Config.Save();
			Helper.UpdateAppTheme();
			Helper.OptionsMain.OptionsOverlayDeckWindows.UpdateAdditionalWindowsBackground();
		}

		private void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var language = ComboboxLanguages.SelectedValue.ToString();
			UpdateAlternativeLanguageList(language);

			if(!_initialized)
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			Config.Instance.SelectedLanguage = selectedLanguage;
			Config.Save();
		}

		private void UpdateAlternativeLanguageList(string primaryLanguage)
		{
			ListBoxAlternativeLanguages.Items.Clear();
			foreach(var pair in Helper.LanguageDict)
			{
				var box = new CheckBox();
				box.Content = pair.Key;
				if(pair.Key == primaryLanguage)
					box.IsEnabled = false;
				else
				{
					box.IsChecked = Config.Instance.AlternativeLanguages.Contains(pair.Value);
					box.Unchecked += CheckboxAlternativeLanguageToggled;
					box.Checked += CheckboxAlternativeLanguageToggled;
				}
				ListBoxAlternativeLanguages.Items.Add(box);
			}
		}

		private void CheckboxAlternativeLanguageToggled(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;

			var languages = new List<string>();
			foreach(CheckBox box in ListBoxAlternativeLanguages.Items)
			{
				var language = (string)box.Content;
				if(box.IsChecked == true)
					languages.Add(Helper.LanguageDict[language]);
			}
			Config.Instance.AlternativeLanguages = languages;
			Config.Save();
		}

		private void ComboboxIconSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ClassIconStyle = (IconStyle)ComboBoxIconSet.SelectedItem;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for the new iconset to be loaded.").Forget();
		}

		private void ComboboxDeckLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerItemLayout = (DeckLayout)ComboBoxDeckLayout.SelectedItem;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for the new layout to be loaded.").Forget();
		}

		private void ButtonRestart_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.Restart();
		}

		private void CheckboxDeckPickerCaps_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = true;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckboxDeckPickerCaps_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = false;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckboxUseAnimations_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseAnimations = false;
			Core.MainWindow.UpdateFlyoutAnimationsEnabled();
			Config.Save();
		}

		private void CheckboxUseAnimations_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseAnimations = true;
			Core.MainWindow.UpdateFlyoutAnimationsEnabled();
			Config.Save();
		}

		private void ComboBoxClassColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ClassColorScheme = (ClassColorScheme)ComboBoxClassColors.SelectedItem;
			Config.Save();
		}

		private void CheckBoxArenaStatsTextColoring_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsTextColoring = true;
			Config.Save();
			ArenaStats.Instance.UpdateArenaStatsHighlights();
		}

		private void CheckBoxArenaStatsTextColoring_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsTextColoring = false;
			Config.Save();
			ArenaStats.Instance.UpdateArenaStatsHighlights();
		}

		private void CheckBoxDefaultFont_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NonLatinUseDefaultFont = true;
			Config.Save();
		}

		private void CheckBoxDefaultFont_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NonLatinUseDefaultFont = false;
			Config.Save();
		}

		private void CheckBoxShowLastPlayedDate_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowLastPlayedDateOnDeck = true;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckBoxShowLastPlayedDate_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowLastPlayedDateOnDeck = false;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void ComboBoxLastPlayedDateFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.LastPlayedDateFormat = (LastPlayedDateFormat)ComboBoxLastPlayedDateFormat.SelectedItem;
			Config.Save();
		}
	}
}