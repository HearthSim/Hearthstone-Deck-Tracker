using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for ConstructedFilters.xaml
	/// </summary>
	public partial class ConstructedFilters : INotifyPropertyChanged
	{
		private readonly bool _initialized;
		private Action _updateCallback;

		internal void SetUpdateCallback(Action action)
		{
			if(_updateCallback == null)
				_updateCallback = action;
		}

		public ConstructedFilters()
		{
			InitializeComponent();
			ComboBoxTimeframe.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboBoxTimeframe.SelectedItem = Config.Instance.ConstructedStatsTimeFrameFilter;
			ComboBoxClass.ItemsSource =
				Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ConstructedStatsClassFilter);
			ComboBoxRegion.ItemsSource = Enum.GetValues(typeof(RegionAll));
			ComboBoxRegion.SelectedItem = Config.Instance.ConstructedStatsRegionFilter;
			ComboBoxMode.ItemsSource = new[]
			{
				GameMode.All,
				GameMode.Ranked,
				GameMode.Casual,
				GameMode.Brawl,
				GameMode.Friendly,
				GameMode.Practice,
				GameMode.Spectator
			};
			ComboBoxMode.SelectedItem = Config.Instance.ConstructedStatsModeFilter;
			ComboBoxFormat.ItemsSource = Enum.GetValues(typeof(Format));
			ComboBoxFormat.SelectedItem = Config.Instance.ConstructedStatsFormatFilter;
			ComboBoxCoin.ItemsSource = Enum.GetValues(typeof(AllYesNo));
			ComboBoxCoin.SelectedItem = Config.Instance.ConstructedStatsCoinFilter;
			ComboBoxResult.ItemsSource = Enum.GetValues(typeof(GameResult));
			ComboBoxResult.SelectedItem = Config.Instance.ConstructedStatsResultFilter;
			ComboBoxOpponentClass.ItemsSource =
				Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxOpponentClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ConstructedStatsOpponentClassFilter);
			TextBoxOpponentName.Text = Config.Instance.ConstructedStatsOpponentNameFilter;
			TextBoxNote.Text = Config.Instance.ConstructedStatsNoteFilter;
			TextBoxTurnsMin.Text = Config.Instance.ConstructedStatsTurnsFilterMin.ToString();
			TextBoxTurnsMax.Text = Config.Instance.ConstructedStatsTurnsFilterMax.ToString();

			_initialized = true;
		}

		private void ComboBoxTimeframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsTimeFrameFilter = (DisplayedTimeFrame)ComboBoxTimeframe.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxClass.SelectedItem).HeroClass;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void DatePickerCustomTimeFrame_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
		}

		private void ComboBoxRegion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsRegionFilter = (RegionAll)ComboBoxRegion.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsModeFilter = (GameMode)ComboBoxMode.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
			OnPropertyChanged(nameof(RankFilterVisibility));
			OnPropertyChanged(nameof(FormatFilterVisibility));
		}

		private void ComboBoxCoin_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsCoinFilter = (AllYesNo)ComboBoxCoin.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}
		private void CheckBoxArchived_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
		}

		private void CheckBoxArchived_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			_updateCallback?.Invoke();
		}

		public Visibility RankFilterVisibility
			=> Config.Instance.ConstructedStatsModeFilter == GameMode.Ranked ? Visibility.Visible : Visibility.Collapsed;

		public Visibility FormatFilterVisibility
			=> Config.Instance.ConstructedStatsModeFilter == GameMode.Ranked || Config.Instance.ConstructedStatsModeFilter == GameMode.Casual
					? Visibility.Visible : Visibility.Collapsed;

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ComboBoxFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsFormatFilter = (Format)ComboBoxFormat.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxTurn_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void ComboBoxResult_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsResultFilter = (GameResultAll)ComboBoxResult.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxOpponentClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ConstructedStatsOpponentClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxOpponentClass.SelectedItem).HeroClass;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxOpponentName_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxOpponentName.Text == Config.Instance.ConstructedStatsOpponentNameFilter)
				return;
			Config.Instance.ConstructedStatsOpponentNameFilter = TextBoxOpponentName.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxNote_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxNote.Text == Config.Instance.ConstructedStatsNoteFilter)
				return;
			Config.Instance.ConstructedStatsNoteFilter = TextBoxNote.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxRankMin_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxRankMin.Text == Config.Instance.ConstructedStatsRankFilterMin)
				return;
			Config.Instance.ConstructedStatsRankFilterMin = TextBoxRankMin.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxRankMax_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TextBoxRankMax.Text == Config.Instance.ConstructedStatsRankFilterMax)
				return;
			Config.Instance.ConstructedStatsRankFilterMax = TextBoxRankMax.Text;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxTurnsMin_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			int value;
			if(!int.TryParse(TextBoxTurnsMin.Text, out value))
				return;
			if(value == Config.Instance.ConstructedStatsTurnsFilterMin)
				return;
			Config.Instance.ConstructedStatsTurnsFilterMin = value;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void TextBoxTurnsMax_OnLostFocus(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			int value;
			if(!int.TryParse(TextBoxTurnsMax.Text, out value))
				return;
			if(value == Config.Instance.ConstructedStatsTurnsFilterMax)
				return;
			Config.Instance.ConstructedStatsTurnsFilterMax = value;
			Config.Save();
			_updateCallback?.Invoke();
		}
	}
}
