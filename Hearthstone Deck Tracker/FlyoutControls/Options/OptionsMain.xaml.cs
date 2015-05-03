#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker;

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

		public void Load()
		{
			OptionsOverlayGeneral.Load();
			OptionsOverlayDeckWindows.Load();
			OptionsOverlayPlayer.Load();
			OptionsOverlayOpponent.Load();
			OptionsOverlayInteractivity.Load();
			OptionsTrackerSettings.Load();
			OptionsTrackerExporting.Load();
			OptionsTrackerImporting.Load();
			OptionsTrackerLogging.Load();
			OptionsTrackerStats.Load();
			OptionsTrackerLogging.Load();
			OptionsTrackerGeneral.Load();
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
	}
}