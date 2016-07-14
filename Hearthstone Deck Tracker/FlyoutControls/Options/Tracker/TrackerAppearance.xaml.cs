#region

using System;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
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
			ComboBoxDeckLayout.ItemsSource = Enum.GetValues(typeof(DeckLayout));
			ComboBoxIconSet.ItemsSource = new[] {IconStyle.Round, IconStyle.Square};
			ComboBoxClassColors.ItemsSource = Enum.GetValues(typeof(ClassColorScheme));
			CheckboxUseAnimations.IsChecked = Config.Instance.UseAnimations;
			ComboBoxCardTheme.ItemsSource = Utility.Themes.ThemeManager.Themes;

			ComboboxTheme.SelectedItem = Config.Instance.AppTheme;
			ComboboxAccent.SelectedItem = Helper.GetAppAccent();

			ComboBoxIconSet.SelectedItem = Config.Instance.ClassIconStyle;
			ComboBoxDeckLayout.SelectedItem = Config.Instance.DeckPickerItemLayout;
			ComboBoxClassColors.SelectedItem = Config.Instance.ClassColorScheme;
			CheckBoxArenaStatsTextColoring.IsChecked = Config.Instance.ArenaStatsTextColoring;
			ComboBoxCardTheme.SelectedItem = Utility.Themes.ThemeManager.FindTheme(Config.Instance.CardBarTheme);
			CheckboxCardFrameRarity.IsChecked = Config.Instance.RarityCardFrames;
			CheckboxCardGemRarity.IsChecked = Config.Instance.RarityCardGems;
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
			Config.Instance.AppTheme = (MetroTheme)ComboboxTheme.SelectedItem;
			Config.Save();
			Helper.UpdateAppTheme();
			Helper.OptionsMain.OptionsOverlayDeckWindows.UpdateAdditionalWindowsBackground();
		}

		private void ComboboxIconSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ClassIconStyle = (IconStyle)ComboBoxIconSet.SelectedItem;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void ComboBoxCardTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CardBarTheme = ComboBoxCardTheme.SelectedItem.ToString().ToLowerInvariant();
			Config.Save();
			Utility.Themes.ThemeManager.SetTheme(Config.Instance.CardBarTheme);
		}

		private void ComboboxDeckLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.DeckPickerItemLayout = (DeckLayout)ComboBoxDeckLayout.SelectedItem;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
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

		private void CheckboxCardFrameRarity_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardFrames = true;
			Config.Save();
			Utility.Themes.ThemeManager.UpdateCards();
		}

		private void CheckboxCardFrameRarity_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardFrames = false;
			Config.Save();
			Utility.Themes.ThemeManager.UpdateCards();
		}

		private void CheckboxCardGemRarity_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardGems = true;
			Config.Save();
			Utility.Themes.ThemeManager.UpdateCards();
		}

		private void CheckboxCardGemRarity_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardGems = false;
			Config.Save();
			Utility.Themes.ThemeManager.UpdateCards();
		}
	}
}