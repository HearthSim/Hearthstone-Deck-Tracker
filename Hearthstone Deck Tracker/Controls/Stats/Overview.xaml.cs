#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Controls.Stats.Arena;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : UserControl
	{
		private readonly ArenaAdvancedCharts _arenaAdvancedCharts = new ArenaAdvancedCharts();
		private readonly bool _initialized;

		public Overview()
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

		public ArenaStatsSummary ArenaStatsSummary { get; } = new ArenaStatsSummary();

		public ArenaRuns ArenaRuns { get; } = new ArenaRuns();

		public object ArenaAdvancedCharts => _arenaAdvancedCharts;

		private void ComboBoxTimeframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsTimeFrameFilter = (DisplayedTimeFrame)ComboBoxTimeframe.SelectedItem;
			Config.Save();
			UpdateStats();
		}

		private void ComboBoxClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxClass.SelectedItem).HeroClass;
			Config.Save();
			UpdateStats();
		}

		private void DatePickerCustomTimeFrame_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void ComboBoxRegion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaStatsRegionFilter = (RegionAll)ComboBoxRegion.SelectedItem;
			Config.Save();
			UpdateStats();
		}

		private void CheckBoxArchived_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void CheckBoxArchived_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		public void UpdateStats()
		{
			ArenaStats.Instance.UpdateArenaStats();
			if(TreeViewItemArenaRunsSummary.IsSelected || TreeViewItemArenaRuns.IsSelected)
			{
				ArenaStats.Instance.UpdateArenaStatsHighlights();
				ArenaStats.Instance.UpdateArenaRewards();
			}
		}

		private void TreeViewItemArenaRunsSummary_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void TreeViewItemArenaRunsOverview_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void TreeViewItemArenaRunsAdvanced_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void TreeViewItemArenaRuns_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}
	}

	public class HeroClassStatsFilterWrapper
	{
		public HeroClassStatsFilterWrapper(HeroClassStatsFilter heroClass)
		{
			HeroClass = heroClass;
		}

		public HeroClassStatsFilter HeroClass { get; }

		public BitmapImage ClassImage => ImageCache.GetClassIcon(HeroClass.ToString());

		public Visibility ImageVisibility => HeroClass == HeroClassStatsFilter.All ? Visibility.Collapsed : Visibility.Visible;

		public override bool Equals(object obj)
		{
			var wrapper = obj as HeroClassStatsFilterWrapper;
			return wrapper != null && HeroClass.Equals(wrapper.HeroClass);
		}

		public override int GetHashCode() => HeroClass.GetHashCode();
	}
}