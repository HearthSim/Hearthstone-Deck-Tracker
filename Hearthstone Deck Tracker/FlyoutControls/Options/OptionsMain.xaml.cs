﻿#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.FlyoutControls.Options;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

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
		public readonly OverlayStreaming OptionsOverlayStreaming = new OverlayStreaming();
		public readonly TrackerAppearance OptionsTrackerAppearance = new TrackerAppearance();
		public readonly TrackerBackups OptionsTrackerBackups = new TrackerBackups();
		public readonly TrackerGeneral OptionsTrackerGeneral = new TrackerGeneral();
		public readonly TrackerHotKeys OptionsTrackerHotKeys = new TrackerHotKeys();
		public readonly TrackerImporting OptionsTrackerImporting = new TrackerImporting();
		public readonly TrackerLogging OptionsTrackerLogging = new TrackerLogging();
		public readonly TrackerNotifications OptionsTrackerNotifications = new TrackerNotifications();
		public readonly TrackerPlugins OptionsTrackerPlugins = new TrackerPlugins();
		public readonly TrackerSettings OptionsTrackerSettings = new TrackerSettings();
		public readonly TrackerStats OptionsTrackerStats = new TrackerStats();
		public readonly TrackerReplays OptionsTrackerReplays = new TrackerReplays();
		public readonly OptionsSearch OptionsSearch = new OptionsSearch();

		public OptionsMain()
		{
			InitializeComponent();
			Helper.OptionsMain = this;
			try
			{
				foreach(var treeItem in TreeViewOptions.Items.Cast<TreeViewItem>())
					treeItem.ExpandSubtree();
				TreeViewOptions.Items.Cast<TreeViewItem>().ToArray()[1].Items.Cast<TreeViewItem>().First().IsSelected = true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
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
			OptionsTrackerImporting.Load(game);
			OptionsTrackerLogging.Load(game);
			OptionsTrackerStats.Load();
			OptionsTrackerLogging.Load(game);
			OptionsTrackerGeneral.Load();
			OptionsTrackerAppearance.Load();
			OptionsTrackerBackups.Load();
			OptionsTrackerNotifications.Load();
		}

		private void TreeViewItemSearch_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Search_Header");
			ContentControlOptions.Content = OptionsSearch;
		}

		#region OnSelect events for left side of Options -> Overlay
		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_General_Header");
			ContentControlOptions.Content = OptionsOverlayGeneral;
		}

		private void TreeViewItemOverlayDeckWindows_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_Windows_Header");
			ContentControlOptions.Content = OptionsOverlayDeckWindows;
		}

		private void TreeViewItemOverlayPlayer_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_Player_Header");
			ContentControlOptions.Content = OptionsOverlayPlayer;
		}

		private void TreeViewItemOverlayOpponent_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_Opponent_Header");
			ContentControlOptions.Content = OptionsOverlayOpponent;
		}

		private void TreeViewItemOverlayInteractivity_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_Interactivity_Header");
			ContentControlOptions.Content = OptionsOverlayInteractivity;
		}

		private void TreeViewItemOverlayStreaming_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Overlay_Streaming_Header");
			ContentControlOptions.Content = OptionsOverlayStreaming;
		}
		#endregion

		#region OnSelect events for left side of Options -> Tracker
		private void TreeViewItemTrackerGeneral_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_General_Header");
			ContentControlOptions.Content = OptionsTrackerGeneral;
		}

		private void TreeViewItemTrackerStats_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Stats_Header");
			ContentControlOptions.Content = OptionsTrackerStats;
		}

		private void TreeViewItemTrackerReplays_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Replays_Header");
			ContentControlOptions.Content = OptionsTrackerReplays;
		}

		private void TreeViewItemTrackerImporting_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Importing_Header");
			ContentControlOptions.Content = OptionsTrackerImporting;
		}

		private void TreeViewItemTrackerNotifications_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Notifications_Header");
			ContentControlOptions.Content = OptionsTrackerNotifications;
		}

		private void TreeViewItemTrackerPlugins_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Plugins_Header");
			ContentControlOptions.Content = OptionsTrackerPlugins;
		}

		private void TreeViewItemTrackerHotKeys_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Hotkeys_Header");
			ContentControlOptions.Content = OptionsTrackerHotKeys;
		}

		private void TreeViewItemTrackerBackups_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Backups_Header");
			ContentControlOptions.Content = OptionsTrackerBackups;
		}

		private void TreeViewItemTrackerAppearance_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Appearance_Header");
			ContentControlOptions.Content = OptionsTrackerAppearance;
		}

		private void TreeViewItemTrackerSettings_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Settings_Header");
			ContentControlOptions.Content = OptionsTrackerSettings;
		}

		private void TreeViewItemTrackerLogging_OnSelected(object sender, RoutedEventArgs e)
		{
			OptionsRightDynamicHeader.Header = LocUtil.Get("Options_Tracker_Logging_Header");
			ContentControlOptions.Content = OptionsTrackerLogging;
		}
		#endregion

		private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) => e.Handled = true;
	}
}
