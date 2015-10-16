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
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Stats
{
	/// <summary>
	/// Interaction logic for Overview.xaml
	/// </summary>
	public partial class Overview : UserControl
	{
		private readonly ArenaStats _arenaOverview = new ArenaStats();
		private readonly bool _initialized;

		public Overview()
		{
			InitializeComponent();
			ComboBoxTimeframe.ItemsSource = Enum.GetValues(typeof(DisplayedTimeFrame));
			ComboBoxTimeframe.SelectedItem = Config.Instance.ArenaStatsTimeFrameFilter;
			ComboBoxClass.ItemsSource = Enum.GetValues(typeof(HeroClassStatsFilter)).Cast<HeroClassStatsFilter>().Select(x => new HeroClassStatsFilterWrapper(x));
			ComboBoxClass.SelectedItem = Config.Instance.ArenaStatsClassFilter;
			_initialized = true;
		}

		public ArenaStats ArenaOverview
		{
			get { return _arenaOverview; }
		}

		private void ComboBoxTimeframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TreeViewItemArenaRuns.IsSelected)
			{
				Config.Instance.ArenaStatsTimeFrameFilter = (DisplayedTimeFrame)ComboBoxTimeframe.SelectedItem;
				Config.Save();
				CompiledStats.Instance.UpdateArenaStats();
			}
		}

		private void ComboBoxClass_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TreeViewItemArenaRuns.IsSelected)
			{
				Config.Instance.ArenaStatsClassFilter = ((HeroClassStatsFilterWrapper)ComboBoxClass.SelectedItem).HeroClass;
				Config.Save();
				CompiledStats.Instance.UpdateArenaStats();
			}
		}

		private void DatePickerCustomTimeFrame_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			if(TreeViewItemArenaRuns.IsSelected)
				CompiledStats.Instance.UpdateArenaStats();
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
    }
}
