#region

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
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
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;
			ComboBoxDeckLayout.ItemsSource = Enum.GetValues(typeof(DeckLayout));
			ComboBoxIconSet.ItemsSource = Enum.GetValues(typeof(IconStyle));
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;

			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

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
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if(theme != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
				Config.Instance.ThemeName = theme.Name;
				Application.Current.Resources["GrayTextColorBrush"] = theme.Name == "BaseLight"
					                                                      ? new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor1"])
					                                                      : new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor2"]);
				Helper.OptionsMain.OptionsOverlayDeckWindows.UpdateAdditionalWindowsBackground();
				Config.Save();
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

			Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.");
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
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = true;
			Config.Save();
			Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.");
		}

		private void CheckboxDeckPickerCaps_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerCaps = false;
			Config.Save();
			Helper.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.");
		}
	}
}