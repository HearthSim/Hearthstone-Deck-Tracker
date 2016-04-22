#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hearthstone_Deck_Tracker.Controls.Stats.Arena;
using Hearthstone_Deck_Tracker.Controls.Stats.Constructed;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

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
			ArenaFilters.SetUpdateCallback(UpdateCallBack);
			ConstructedFilters.SetUpdateCallback(UpdateCallBack);
			ConstructedFilters.CheckBoxDecks.Checked += (sender, args) => ConstructedSummary.UpdateContent();
			ConstructedFilters.CheckBoxDecks.Unchecked += (sender, args) => ConstructedSummary.UpdateContent();
			_initialized = true;
		}

		public ArenaStatsSummary ArenaStatsSummary { get; } = new ArenaStatsSummary();

		public ArenaRuns ArenaRuns { get; } = new ArenaRuns();

		public ConstructedGames ConstructedGames { get; } = new ConstructedGames();

		public ConstructedSummary ConstructedSummary { get; } = new ConstructedSummary();

		public ConstructedFilters ConstructedFilters { get; private set; } = new ConstructedFilters();

		public ArenaFilters ArenaFilters { get; private set; } = new ArenaFilters();

		public object ArenaAdvancedCharts => _arenaAdvancedCharts;

		private void UpdateCallBack()
		{
			if(Config.Instance.StatsAutoRefresh)
				UpdateStats();
		}

		public void UpdateStats()
		{
			if(TreeViewItemConstructedGames.IsSelected || TreeViewItemConstructedSummary.IsSelected)
			{
				ConstructedStats.Instance.UpdateConstructedStats();
				return;
			}
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

		private void TreeViewItemConstructedGames_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void TreeViewItemConstructedSummary_OnSelected(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			UpdateStats();
		}

		private void TreeViewStats_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var selected = TreeViewStats.SelectedItem as TreeViewItem;
			if(selected == null)
				return;
			if(selected.Equals(TreeViewItemArenaRuns)
			   || (Helper.GetVisualParent<TreeViewItem>(selected)?.Equals(TreeViewItemArenaRuns) ?? false))
				ContentControlFilter.Content = ArenaFilters;
			else if(selected.Equals(TreeViewItemConstructed)
					|| (Helper.GetVisualParent<TreeViewItem>(selected)?.Equals(TreeViewItemConstructed) ?? false))
				ContentControlFilter.Content = ConstructedFilters;
		}

		private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e) => UpdateStats();

		private void ButtonMore_OnClick(object sender, RoutedEventArgs e)
		{
			ButtonMoreContextMenu.Placement = PlacementMode.Bottom;
			ButtonMoreContextMenu.PlacementTarget = ButtonMore;
			ButtonMoreContextMenu.IsOpen = true;
		}

		private void MenuItemReset_OnClick(object sender, RoutedEventArgs e)
		{
			if(ContentControlFilter.Content is ArenaFilters)
			{
				ArenaFilters.Reset();
				ArenaFilters = new ArenaFilters(UpdateCallBack);
				ContentControlFilter.Content = ArenaFilters;
			}
			else if(ContentControlFilter.Content is ConstructedFilters)
			{
				ConstructedFilters.Reset();
				ConstructedFilters = new ConstructedFilters(UpdateCallBack);
				ContentControlFilter.Content = ConstructedFilters;
			}
			else
				return;
			UpdateStats();
		}
	}
}