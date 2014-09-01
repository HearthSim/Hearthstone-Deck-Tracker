﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using SystemColors = System.Windows.SystemColors;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options
	{
		private bool _initialized;

		public Options()
		{
			InitializeComponent();
		}

		public void MainWindowInitialized()
		{
			_initialized = true;
		}

   		private void CheckboxPredict_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.PredictAllowed = true;
			SaveConfig(true);
		}

   		private void CheckboxPredict_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
            Config.Instance.PredictAllowed = false;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightCardsInHand = true;
			Game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightCardsInHand = false;
			Game.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInMenu = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInMenu = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmost = true;
			Helper.MainWindow.PlayerWindow.Topmost = true;
			Helper.MainWindow.OpponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmost = false;
			Helper.MainWindow.PlayerWindow.Topmost = false;
			Helper.MainWindow.OpponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			Helper.MainWindow.PlayerWindow.Topmost = false;
			Helper.MainWindow.OpponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if(Config.Instance.WindowsTopmost)
			{
				Helper.MainWindow.PlayerWindow.Topmost = true;
				Helper.MainWindow.OpponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmost = true;
			Helper.MainWindow.TimerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmost = false;
			Helper.MainWindow.TimerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.TimerWindow.Show();
			Helper.MainWindow.TimerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.TimerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			Helper.MainWindow.TimerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if(Config.Instance.TimerWindowTopmost)
				Helper.MainWindow.TimerWindow.Topmost = true;
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Helper.MainWindow.Overlay.Update(true);
		}


		private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayOpacity = SliderOverlayOpacity.Value;
			SaveConfig(true);
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.KeepDecksVisible = true;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.KeepDecksVisible = false;
			SaveConfig(true);
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.MinimizeToTray = true;
			SaveConfig(false);
		}

		private void CheckboxTagOnImport_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TagDecksOnImport = true;
			SaveConfig(false);
		}

		private void CheckboxTagOnImport_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TagDecksOnImport = false;
			SaveConfig(false);
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseSameScaling = true;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseSameScaling = false;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = true;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = false;
			SaveConfig(false);
		}

		private void SliderOverlayPlayerScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			var scaling = SliderOverlayPlayerScaling.Value;
			Config.Instance.OverlayPlayerScaling = scaling;
			SaveConfig(false);
			Helper.MainWindow.Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && SliderOverlayOpponentScaling.Value != scaling)
				SliderOverlayOpponentScaling.Value = scaling;
		}

		private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			var scaling = SliderOverlayOpponentScaling.Value;
			Config.Instance.OverlayOpponentScaling = scaling;
			SaveConfig(false);
			Helper.MainWindow.Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
				SliderOverlayPlayerScaling.Value = scaling;
		}

		private void CheckboxHideTimers_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideTimers = true;
			SaveConfig(true);
		}

		private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideTimers = false;
			SaveConfig(true);
		}

		private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
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
			if(!_initialized) return;
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if(theme != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
				Config.Instance.ThemeName = theme.Name;
				//if(ComboboxWindowBackground.SelectedItem.ToString() != "Default")
				UpdateAdditionalWindowsBackground();
				SaveConfig(false);
			}
		}

		private void ComboboxWindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
			Config.Instance.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
			UpdateAdditionalWindowsBackground();
		}

		internal void UpdateAdditionalWindowsBackground(Brush brush = null)
		{
			var background = brush;

			switch(ComboboxWindowBackground.SelectedItem.ToString())
			{
				case "Theme":
					background = Background;
					break;
				case "Light":
					background = SystemColors.ControlLightBrush;
					break;
				case "Dark":
					background = SystemColors.ControlDarkDarkBrush;
					break;
			}
			if(background == null)
			{
				var hexBackground = BackgroundFromHex();
				if(hexBackground != null)
				{
					Helper.MainWindow.PlayerWindow.Background = hexBackground;
					Helper.MainWindow.OpponentWindow.Background = hexBackground;
					Helper.MainWindow.TimerWindow.Background = hexBackground;
				}
			}
			else
			{
				Helper.MainWindow.PlayerWindow.Background = background;
				Helper.MainWindow.OpponentWindow.Background = background;
				Helper.MainWindow.TimerWindow.Background = background;
			}
		}

		private SolidColorBrush BackgroundFromHex()
		{
			SolidColorBrush brush = null;
			var hex = TextboxCustomBackground.Text;
			if(hex.StartsWith("#")) hex = hex.Remove(0, 1);
			if(!string.IsNullOrEmpty(hex) && hex.Length == 6 && Helper.IsHex(hex))
			{
				var color = ColorTranslator.FromHtml("#" + hex);
				brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
			}
			return brush;
		}

		private void TextboxCustomBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || ComboboxWindowBackground.SelectedItem.ToString() != "Custom") return;
			var background = BackgroundFromHex();
			if(background != null)
			{
				UpdateAdditionalWindowsBackground(background);
				Config.Instance.WindowsBackgroundHex = TextboxCustomBackground.Text;
				SaveConfig(false);
			}
		}

		private async void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			var language = ComboboxLanguages.SelectedValue.ToString();
			if(!Helper.LanguageDict.ContainsKey(language))
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			if(!File.Exists(string.Format("Files/cardsDB.{0}.json", selectedLanguage)))
				return;

			Config.Instance.SelectedLanguage = selectedLanguage;
			Config.Save();


			await Helper.MainWindow.Restart();
		}

		private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = true;
			SaveConfig(false);
		}

		private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = false;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = true;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = false;
			SaveConfig(false);
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

		private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = false;
			SaveConfig(true);
		}

		private async void CheckboxAppData_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			await Helper.MainWindow.Restart();
		}

		private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			await Helper.MainWindow.Restart();
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveMyDecks = true;
			Helper.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveMyDecks = false;
			Helper.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized) return;
			Config.Instance.TrackerCardToolTips = true;
			SaveConfig(false);
			await Helper.MainWindow.Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized) return;
			Config.Instance.TrackerCardToolTips = false;
			SaveConfig(false);
			await Helper.MainWindow.Restart();
		}

		private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowCardToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowCardToolTips = false;
			SaveConfig(false);
		}

		private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayCardToolTips = true;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = true;
			CheckboxOverlaySecretToolTipsOnly.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayCardToolTips = false;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = false;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = false;
			CheckboxOverlaySecretToolTipsOnly.IsEnabled = false;
			CheckboxOverlaySecretToolTipsOnly.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxDeckSortingClassFirst_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CardSortingClassFirst = true;
			SaveConfig(false);
			Helper.SortCardCollection(Helper.MainWindow.ListViewDeck.ItemsSource, true);
			//Helper.SortCardCollection(ListViewNewDeck.Items, true);
		}

		private void CheckboxDeckSortingClassFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CardSortingClassFirst = false;
			SaveConfig(false);
			Helper.SortCardCollection(Helper.MainWindow.ListViewDeck.ItemsSource, false);
			//Helper.SortCardCollection(ListViewNewDeck.Items, false);
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.BringHsToForeground = true;
			SaveConfig(false);
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.BringHsToForeground = false;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = false;
			SaveConfig(false);
		}

		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			Helper.MainWindow.Overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if(!Game.IsInMenu)
				Helper.MainWindow.Overlay.ShowSecrets(Game.PlayingAgainst);
		}

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightDiscarded = true;
			Game.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightDiscarded = false;
			Game.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private async void BtnUnlockOverlay_Click(object sender, RoutedEventArgs e)
		{
			if(User32.GetHearthstoneWindow() == IntPtr.Zero) return;
			BtnUnlockOverlay.Content = await Helper.MainWindow.Overlay.UnlockUI() ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result =
				await
				Helper.MainWindow.ShowMessageAsync("Resetting overlay to default",
				                                   "Positions of: Player Deck, Opponent deck, Timers and Secrets will be reset to default. Are you sure?",
				                                   MessageDialogStyle.AffirmativeAndNegative);
			if(result != MessageDialogResult.Affirmative)
				return;

			if((string)BtnUnlockOverlay.Content == "Lock")
			{
				await Helper.MainWindow.Overlay.UnlockUI();
				BtnUnlockOverlay.Content = "Unlock";
			}

			Config.Instance.PlayerDeckTop = Config.Defaults.PlayerDeckTop;
			Config.Instance.PlayerDeckLeft = Config.Defaults.PlayerDeckLeft;
			Config.Instance.PlayerDeckHeight = Config.Defaults.PlayerDeckHeight;

			Config.Instance.OpponentDeckTop = Config.Defaults.OpponentDeckTop;
			Config.Instance.OpponentDeckLeft = Config.Defaults.OpponentDeckLeft;
			Config.Instance.OpponentDeckHeight = Config.Defaults.OpponentDeckHeight;

			Config.Instance.TimersHorizontalPosition = Config.Defaults.TimersHorizontalPosition;
			Config.Instance.TimersHorizontalSpacing = Config.Defaults.TimersHorizontalSpacing;

			Config.Instance.TimersHorizontalSpacing = Config.Defaults.TimersHorizontalSpacing;
			Config.Instance.TimersVerticalSpacing = Config.Defaults.TimersVerticalSpacing;

			Config.Instance.SecretsTop = Config.Defaults.SecretsTop;
			Config.Instance.SecretsLeft = Config.Defaults.SecretsLeft;
			Config.Instance.SecretsHeight = Config.Defaults.SecretsHeight;

			SaveConfig(true);
		}

		private void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			Game.Reset();
			if(Helper.MainWindow.DeckPickerList.SelectedDeck != null)
				Game.SetPremadeDeck((Deck)Helper.MainWindow.DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			Game.Reset();
			if(Helper.MainWindow.DeckPickerList.SelectedDeck != null)
				Game.SetPremadeDeck((Deck)Helper.MainWindow.DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxHighlightLastDrawn_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightLastDrawn = true;
			SaveConfig(false);
		}

		private void CheckboxHighlightLastDrawn_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightLastDrawn = false;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StartMinimized = true;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StartMinimized = false;
			SaveConfig(false);
		}

		private void CheckboxShowPlayerGet_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ShowPlayerGet = true;
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ShowPlayerGet = false;
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = true;
			SaveConfig(false);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = false;
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExtraFeatures = true;
			Helper.MainWindow.Overlay.HookMouse();
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExtraFeatures = false;
			Helper.MainWindow.Overlay.UnHookMouse();
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CheckForUpdates = true;
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CheckForUpdates = false;
			SaveConfig(false);
		}

		private void CheckboxRecordRanked_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordRanked = true;
			SaveConfig(false);
		}

		private void CheckboxRecordRanked_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordRanked = false;
			SaveConfig(false);
		}

		private void CheckboxRecordArena_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordArena = true;
			SaveConfig(false);
		}

		private void CheckboxRecordArena_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordArena = false;
			SaveConfig(false);
		}

		private void CheckboxRecordCasual_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordCasual = true;
			SaveConfig(false);
		}

		private void CheckboxRecordCasual_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordCasual = false;
			SaveConfig(false);
		}

		private void CheckboxRecordFriendly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordFriendly = true;
			SaveConfig(false);
		}

		private void CheckboxRecordFriendly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordFriendly = false;
			SaveConfig(false);
		}

		private void CheckboxRecordPractice_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordPractice = true;
			SaveConfig(false);
		}

		private void CheckboxRecordPractice_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordPractice = false;
			SaveConfig(false);
		}

		private void CheckboxRecordOther_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordOther = true;
			SaveConfig(false);
		}

		private void CheckboxRecordOther_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordOther = false;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseFullTextSearch = true;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseFullTextSearch = false;
			SaveConfig(false);
		}

		private void CheckboxDiscardGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.DiscardGameIfIncorrectDeck = true;
			SaveConfig(false);
		}

		private void CheckboxDiscardGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.DiscardGameIfIncorrectDeck = false;
			SaveConfig(false);
		}

		private void ComboboxExportSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var selected = ComboboxExportSpeed.SelectedValue.ToString();

			switch(selected)
			{
				case "Very Fast (20ms)":
					Config.Instance.DeckExportDelay = 20;
					break;
				case "Fast (40ms)":
					Config.Instance.DeckExportDelay = 40;
					break;
				case "Normal (60ms)":
					Config.Instance.DeckExportDelay = 60;
					break;
				case "Slow (100ms)":
					Config.Instance.DeckExportDelay = 100;
					break;
				case "Very Slow (150ms)":
					Config.Instance.DeckExportDelay = 150;
					break;
				default:
					return;
			}
			SaveConfig(false);
		}

		private void CheckboxExportPasteClipboard_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExportPasteClipboard = true;
			SaveConfig(false);
		}

		private void CheckboxExportPasteClipboard_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExportPasteClipboard = false;
			SaveConfig(false);
		}

		private void CheckboxGoldenFeugen_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenFeugen = true;
			SaveConfig(false);
		}

		private void CheckboxGoldenFeugen_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenFeugen = false;
			SaveConfig(false);
		}

		private void CheckboxGoldenStalagg_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenStalagg = true;
			SaveConfig(false);
		}

		private void CheckboxGoldenStalagg_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenStalagg = false;
			SaveConfig(false);
		}

		private void CheckboxCloseWithHearthstone_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CloseWithHearthstone = true;
			Config.Save();
		}

		private void CheckboxCloseWithHearthstone_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CloseWithHearthstone = false;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StatsInWindow = true;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StatsInWindow = false;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.PlayerWindow.Show();
			Helper.MainWindow.PlayerWindow.Activate();
			Helper.MainWindow.PlayerWindow.SetCardCount(Game.PlayerHandCount, 30 - Game.PlayerDrawn.Where(c => !c.IsStolen).Sum(card => card.Count));
			Config.Instance.PlayerWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.PlayerWindow.Hide();
			Config.Instance.PlayerWindowOnStart = false;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.OpponentWindow.Show();
			Helper.MainWindow.OpponentWindow.Activate();
			Helper.MainWindow.OpponentWindow.SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount, Game.OpponentHasCoin);
			Config.Instance.OpponentWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Helper.MainWindow.OpponentWindow.Hide();
			Config.Instance.OpponentWindowOnStart = false;
			Config.Save();
		}

		private void CheckboxOverlaySecretToolTipsOnly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlaySecretToolTipsOnly = true;
			Config.Save();
		}

		private void CheckboxOverlaySecretToolTipsOnly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlaySecretToolTipsOnly = false;
			Config.Save();
		}
	}
}