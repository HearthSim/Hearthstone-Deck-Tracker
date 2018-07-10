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
			CheckBoxAutoDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckBoxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys.Where(x => x != "English (Great Britain)");
			CheckboxDeckPickerCaps.IsChecked = Config.Instance.DeckPickerCaps;
			ComboBoxDeckDateType.ItemsSource = Enum.GetValues(typeof(DeckDateType));
			ComboBoxDeckDateType.SelectedItem = Config.Instance.SelectedDateOnDecks;
			ComboBoxDateFormat.ItemsSource = Enum.GetValues(typeof(DateFormat));
			ComboBoxDateFormat.SelectedItem = Config.Instance.SelectedDateFormat;
			DateFormatPanel.Visibility = Config.Instance.ShowDateOnDeck ? Visibility.Visible : Visibility.Collapsed;
			CheckboxShowMyGamesPanel.IsChecked = Config.Instance.ShowMyGamesPanel;
			CheckBoxAutoArchiveArenaDecks.IsChecked = Config.Instance.AutoArchiveArenaDecks;

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
	}
}
