#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class OptionsMain
	{
		public OverlayDeckWindows OptionsOverlayDeckWindows;
		public OverlayGeneral OptionsOverlayGeneral;
		public OverlayInteractivity OptionsOverlayInteractivity;
		public OverlayOpponent OptionsOverlayOpponent;
		public OverlayPlayer OptionsOverlayPlayer;
		public TrackerExporting OptionsTrackerExporting;
		public TrackerGeneral OptionsTrackerGeneral;
		public TrackerImporting OptionsTrackerImporting;
		public TrackerLogging OptionsTrackerLogging;
		public TrackerPlugins OptionsTrackerPlugins;
		public TrackerSettings OptionsTrackerSettings;
		public TrackerStats OptionsTrackerStats;
		public TrackerAppearance OptionsTrackerAppearance;
		public TrackerBackups OptionsTrackerBackups;
		public TrackerHotKeys OptionsTrackerHotKeys;

		public OptionsMain()
		{
			InitializeComponent();
			Helper.OptionsMain = this;
			OptionsOverlayGeneral = new OverlayGeneral();
			OptionsOverlayDeckWindows = new OverlayDeckWindows();
			OptionsOverlayOpponent = new OverlayOpponent();
			OptionsOverlayPlayer = new OverlayPlayer();
			OptionsOverlayInteractivity = new OverlayInteractivity();
			OptionsTrackerExporting = new TrackerExporting();
			OptionsTrackerImporting = new TrackerImporting();
			OptionsTrackerLogging = new TrackerLogging();
			OptionsTrackerStats = new TrackerStats();
			OptionsTrackerExporting = new TrackerExporting();
			OptionsTrackerSettings = new TrackerSettings();
			OptionsTrackerGeneral = new TrackerGeneral();
			OptionsTrackerPlugins = new TrackerPlugins();
			OptionsTrackerAppearance = new TrackerAppearance();
			OptionsTrackerBackups = new TrackerBackups();
			OptionsTrackerHotKeys = new TrackerHotKeys();
			try
			{
				foreach(var treeItem in TreeViewOptions.Items.Cast<TreeViewItem>())
					treeItem.ExpandSubtree();
				//select overlay - general
				TreeViewOptions.Items.Cast<TreeViewItem>().First().Items.Cast<TreeViewItem>().First().IsSelected = true;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "Options");
			}
		}

		public void Load(GameV2 game)
		{
			OptionsOverlayGeneral.Load(game);
			OptionsOverlayDeckWindows.Load(game);
			OptionsOverlayPlayer.Load(game);
			OptionsOverlayOpponent.Load(game);
			OptionsOverlayInteractivity.Load();
			OptionsTrackerSettings.Load();
			OptionsTrackerExporting.Load();
			OptionsTrackerImporting.Load(game);
			OptionsTrackerLogging.Load(game);
			OptionsTrackerStats.Load();
			OptionsTrackerLogging.Load(game);
			OptionsTrackerGeneral.Load();
			OptionsTrackerAppearance.Load();
			OptionsTrackerBackups.Load();
			//OptionsTrackerPlugins.Load(); - load in main after loading plugins
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayGeneral;
		}

		private void TreeViewItemOverlayDeckWindows_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayDeckWindows;
		}

		private void TreeViewItemOverlayOpponent_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayOpponent;
		}

		private void TreeViewItemOverlayPlayer_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayPlayer;
		}

		private void TreeViewItemTrackerGeneral_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerGeneral;
		}

		private void TreeViewItemTrackerStats_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerStats;
		}

		private void TreeViewItemTrackerExporting_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerExporting;
		}

		private void TreeViewItemTrackerImporting_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerImporting;
		}

		private void TreeViewItemTrackerLogging_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerLogging;
		}

		private void TreeViewItemOverlayInteractivity_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayInteractivity;
		}

		private void TreeViewItemTrackerSettings_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerSettings;
		}

		private void TreeViewItemTrackerPlugins_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerPlugins;
		}

		private void TreeViewItemTrackerAppearance_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerAppearance;
		}

		private void TreeViewItemTrackerBackups_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerBackups;
		}

		private void TreeViewItemTrackerHotKeys_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsTrackerHotKeys;
		}
	}
}