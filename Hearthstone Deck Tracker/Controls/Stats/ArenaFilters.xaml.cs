using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for ArenaFilters.xaml
	/// </summary>
	public partial class ArenaFilters : UserControl
	{
		private readonly bool _initialized;
		private Action _updateCallback;

		internal void SetUpdateCallback(Action action)
		{
			if(_updateCallback == null)
				_updateCallback = action;
		}

		public ArenaFilters()
		{
			InitializeComponent();
			ComboBoxTimeframe.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboBoxTimeframe.SelectedItem = Config.Instance.ArenaStatsTimeFrameFilter;
			ComboBoxClass.ItemsSource =
				Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ArenaStatsClassFilter);
			ComboBoxRegion.ItemsSource = Enum.GetValues(typeof(RegionAll));
			ComboBoxRegion.SelectedItem = Config.Instance.ArenaStatsRegionFilter;
			_initialized = true;
		}

		private void ComboBoxTimeframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsTimeFrameFilter = (DisplayedTimeFrame)ComboBoxTimeframe.SelectedItem;
			Config.Save();
			_updateCallback?.Invoke();
		}

		private void ComboBoxClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxClass.SelectedItem).HeroClass;
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
			Config.Instance.ArenaStatsRegionFilter = (RegionAll)ComboBoxRegion.SelectedItem;
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
	}
}
