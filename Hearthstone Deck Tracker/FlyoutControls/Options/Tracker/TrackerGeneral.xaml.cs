#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
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
		private Visibility _restartLabelVisibility = Visibility.Collapsed;

		public TrackerGeneral()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckBoxAutoUse.IsChecked = Config.Instance.AutoUseDeck;
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxTimerAlert.IsChecked = Config.Instance.TimerAlert;
			CheckboxSpectatorUseNoDeck.IsChecked = Config.Instance.SpectatorUseNoDeck;
			CheckBoxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			TextboxTimerAlert.Text = Config.Instance.TimerAlertSeconds.ToString();
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys.Where(x => x != "English (Great Britain)");
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;
			ComboBoxLastPlayedDateFormat.ItemsSource = Enum.GetValues(typeof(LastPlayedDateFormat));
			CheckBoxShowLastPlayedDate.IsChecked = Config.Instance.ShowLastPlayedDateOnDeck;
			ComboBoxLastPlayedDateFormat.SelectedItem = Config.Instance.LastPlayedDateFormat;

			if(Config.Instance.NonLatinUseDefaultFont == null)
			{
				Config.Instance.NonLatinUseDefaultFont = Helper.IsWindows10();
				Config.Save();
			}
			CheckBoxDefaultFont.IsChecked = Config.Instance.NonLatinUseDefaultFont;


			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;
			_initialized = true;
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = true;
			Config.Save();
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = false;
			Config.Save();
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

		private void TextboxTimerAlert_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void TextboxTimerAlert_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || CheckboxTimerAlert.IsChecked != true)
				return;
			int mTimerAlertValue;
			if(int.TryParse(TextboxTimerAlert.Text, out mTimerAlertValue))
			{
				if(mTimerAlertValue < 0)
				{
					TextboxTimerAlert.Text = "0";
					mTimerAlertValue = 0;
				}

				if(mTimerAlertValue > 90)
				{
					TextboxTimerAlert.Text = "90";
					mTimerAlertValue = 90;
				}

				Config.Instance.TimerAlertSeconds = mTimerAlertValue;
				Config.Save();
			}
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = true;
			Config.Save();
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = false;
			Config.Save();
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = true;
			Config.Save();
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = false;
			Config.Save();
		}

		private void CheckboxTimerAlert_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = true;
			TextboxTimerAlert.IsEnabled = true;
			Config.Save();
		}

		private void CheckboxTimerAlert_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = false;
			TextboxTimerAlert.IsEnabled = false;
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

		private void CheckboxSpectatorUseNoDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SpectatorUseNoDeck = true;
			Config.Save();
		}

		private void CheckboxSpectatorUseNoDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SpectatorUseNoDeck = false;
			Config.Save();
		}

		private void CheckBoxClassCardsFirst_Checked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(true);

		private void CheckBoxClassCardsFirst_Unchecked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(false);

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
			if(!_initialized)
				return;
			Config.Instance.ShowLastPlayedDateOnDeck = true;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckBoxShowLastPlayedDate_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowLastPlayedDateOnDeck = false;
			Config.Save();
			MessageDialogs.ShowRestartDialog();
		}

		private void CheckBoxAutoArchiveArenaDecks_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.AutoArchiveArenaDecks = true;
			Config.Save();
		}

		private void CheckBoxAutoArchiveArenaDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.AutoArchiveArenaDecks = false;
			Config.Save();
		}

		private void ComboBoxLastPlayedDateFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.LastPlayedDateFormat = (LastPlayedDateFormat)ComboBoxLastPlayedDateFormat.SelectedItem;
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

		private void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var language = ComboboxLanguages.SelectedValue.ToString();
			UpdateAlternativeLanguageList(language);

			if(!_initialized)
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			Config.Instance.SelectedLanguage = selectedLanguage;
			Config.Save();
			RestartLabelVisibility = Visibility.Visible;
		}

		private void UpdateAlternativeLanguageList(string primaryLanguage)
		{
			ListBoxAlternativeLanguages.Items.Clear();
			foreach(var pair in Helper.LanguageDict.Where(x => x.Key != "English (Great Britain)"))
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
			RestartLabelVisibility = Visibility.Visible;
		}

		public Visibility RestartLabelVisibility
		{
			get { return _restartLabelVisibility; }
			set
			{
				if(_restartLabelVisibility == value)
					return;
				_restartLabelVisibility = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}