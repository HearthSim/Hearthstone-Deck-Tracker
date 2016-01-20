﻿#region

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
		public readonly OverlayDeckWindows OptionsOverlayDeckWindows = new OverlayDeckWindows();
		public readonly OverlayGeneral OptionsOverlayGeneral = new OverlayGeneral();
		public readonly OverlayInteractivity OptionsOverlayInteractivity = new OverlayInteractivity();
		public readonly OverlayOpponent OptionsOverlayOpponent = new OverlayOpponent();
		public readonly OverlayPlayer OptionsOverlayPlayer = new OverlayPlayer();
		public readonly TrackerAppearance OptionsTrackerAppearance = new TrackerAppearance();
		public readonly TrackerBackups OptionsTrackerBackups = new TrackerBackups();
		public readonly TrackerExporting OptionsTrackerExporting = new TrackerExporting();
		public readonly TrackerGeneral OptionsTrackerGeneral = new TrackerGeneral();
		public readonly TrackerHotKeys OptionsTrackerHotKeys = new TrackerHotKeys();
		public readonly TrackerImporting OptionsTrackerImporting = new TrackerImporting();
		public readonly TrackerLogging OptionsTrackerLogging = new TrackerLogging();
		public readonly TrackerNotifications OptionsTrackerNotifications = new TrackerNotifications();
		public readonly TrackerPlugins OptionsTrackerPlugins = new TrackerPlugins();
		public readonly TrackerSettings OptionsTrackerSettings = new TrackerSettings();
		public readonly TrackerStats OptionsTrackerStats = new TrackerStats();

		public OptionsMain()
		{
			InitializeComponent();
			Helper.OptionsMain = this;
			try
			{
				foreach(var treeItem in TreeViewOptions.Items.Cast<TreeViewItem>())
					treeItem.ExpandSubtree();
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
			OptionsTrackerNotifications.Load();
			//OptionsTrackerPlugins.Load(); - load in main after loading plugins
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsOverlayGeneral;
		private void TreeViewItemOverlayDeckWindows_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsOverlayDeckWindows;
		private void TreeViewItemOverlayOpponent_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsOverlayOpponent;
		private void TreeViewItemOverlayPlayer_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsOverlayPlayer;
		private void TreeViewItemTrackerGeneral_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerGeneral;
		private void TreeViewItemTrackerStats_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerStats;
		private void TreeViewItemTrackerExporting_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerExporting;
		private void TreeViewItemTrackerImporting_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerImporting;
		private void TreeViewItemTrackerLogging_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerLogging;
		private void TreeViewItemOverlayInteractivity_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsOverlayInteractivity;
		private void TreeViewItemTrackerSettings_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerSettings;
		private void TreeViewItemTrackerPlugins_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerPlugins;
		private void TreeViewItemTrackerAppearance_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerAppearance;
		private void TreeViewItemTrackerBackups_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerBackups;
		private void TreeViewItemTrackerHotKeys_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerHotKeys;
		private void TreeViewItemTrackerNotifications_OnSelected(object sender, RoutedEventArgs e) => ContentControlOptions.Content = OptionsTrackerNotifications;
	}
}