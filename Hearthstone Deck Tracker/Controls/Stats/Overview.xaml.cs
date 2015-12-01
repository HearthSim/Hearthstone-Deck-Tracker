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
using Hearthstone_Deck_Tracker.Controls.Stats.Arena;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : UserControl
	{
		private readonly ArenaRuns _arenaRuns = new ArenaRuns();
		private readonly ArenaAdvancedCharts _arenaAdvancedCharts = new ArenaAdvancedCharts();
		private readonly ArenaStatsSummary _arenaStatsSummary = new ArenaStatsSummary();
		private readonly bool _initialized;

		public Overview()
		{
			InitializeComponent();
			ComboBoxTimeframe.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboBoxTimeframe.SelectedItem = Config.Instance.ArenaStatsTimeFrameFilter;
			ComboBoxClass.ItemsSource = Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxClass.SelectedItem = new HeroClassStatsFilterWrapper(Config.Instance.ArenaStatsClassFilter);
			ComboBoxRegion.ItemsSource = Enum.GetValues(typeof(RegionAll));
			ComboBoxRegion.SelectedItem = Config.Instance.ArenaStatsRegionFilter;
			_initialized = true;
		}

		public ArenaStatsSummary ArenaStatsSummary
		{
			get { return _arenaStatsSummary; }
		}

		public ArenaRuns ArenaRuns
		{
			get { return _arenaRuns; }
		}

		public object ArenaAdvancedCharts
		{
			get { return _arenaAdvancedCharts; }
		}

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

		public HeroClassStatsFilter HeroClass { get; private set; }

		public BitmapImage ClassImage
		{
			get { return ImageCache.GetClassIcon(HeroClass.ToString()); }
		}

		public Visibility ImageVisibility
		{
			get { return HeroClass == HeroClassStatsFilter.All ? Visibility.Collapsed : Visibility.Visible; }
		}

		public override bool Equals(object obj)
		{
			var wrapper = obj as HeroClassStatsFilterWrapper;
			return wrapper != null && HeroClass.Equals(wrapper.HeroClass);
		}

		public override int GetHashCode()
		{
			return HeroClass.GetHashCode();
		}
	}
}
