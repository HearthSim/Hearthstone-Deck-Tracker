﻿#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.FlyoutControls.Options;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming;
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
	public partial class OptionsMain : INotifyPropertyChanged
	{
		public readonly OverlayDeckWindows OptionsOverlayDeckWindows = new OverlayDeckWindows();
		public readonly OverlayGeneral OptionsOverlayGeneral = new OverlayGeneral();
		public readonly OverlayInteractivity OptionsOverlayInteractivity = new OverlayInteractivity();
		public readonly OverlayOpponent OptionsOverlayOpponent = new OverlayOpponent();
		public readonly OverlayPlayer OptionsOverlayPlayer = new OverlayPlayer();
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
		public readonly StreamingTwitchExtension OptionsStreamingTwitchExtension = new StreamingTwitchExtension();
		public readonly StreamingCapturableOverlay OptionsStreamingCapturableOverlay = new StreamingCapturableOverlay();
		public readonly OptionsSearch OptionsSearch = new OptionsSearch();
		private string _contentHeader;
		private object _optionsContent;

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

		public string ContentHeader
		{
			get => _contentHeader;
			set
			{
				_contentHeader = value; 
				OnPropertyChanged();
			}
		}

		public object OptionsContent
		{
			get => _optionsContent;
			set
			{
				_optionsContent = value;
				OnPropertyChanged();
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
			OptionsTrackerImporting.Load();
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
			ContentHeader = LocUtil.Get("Options_Search_Header");
			OptionsContent = OptionsSearch;
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_General_Header");
			OptionsContent = OptionsOverlayGeneral;
		}

		private void TreeViewItemOverlayDeckWindows_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Windows_Header");
			OptionsContent = OptionsOverlayDeckWindows;
		}

		private void TreeViewItemOverlayPlayer_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Player_Header");
			OptionsContent = OptionsOverlayPlayer;
		}

		private void TreeViewItemOverlayOpponent_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Opponent_Header");
			OptionsContent = OptionsOverlayOpponent;
		}

		private void TreeViewItemOverlayInteractivity_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Interactivity_Header");
			OptionsContent = OptionsOverlayInteractivity;
		}

		private void TreeViewItemTrackerGeneral_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_General_Header");
			OptionsContent = OptionsTrackerGeneral;
		}

		private void TreeViewItemTrackerStats_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Stats_Header");
			OptionsContent = OptionsTrackerStats;
		}

		private void TreeViewItemTrackerReplays_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Replays_Header");
			OptionsContent = OptionsTrackerReplays;
		}

		private void TreeViewItemTrackerImporting_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Importing_Header");
			OptionsContent = OptionsTrackerImporting;
		}

		private void TreeViewItemTrackerNotifications_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Notifications_Header");
			OptionsContent = OptionsTrackerNotifications;
		}

		private void TreeViewItemTrackerPlugins_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Plugins_Header");
			OptionsContent = OptionsTrackerPlugins;
		}

		private void TreeViewItemTrackerHotKeys_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Hotkeys_Header");
			OptionsContent = OptionsTrackerHotKeys;
		}

		private void TreeViewItemTrackerBackups_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Backups_Header");
			OptionsContent = OptionsTrackerBackups;
		}

		private void TreeViewItemTrackerAppearance_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Appearance_Header");
			OptionsContent = OptionsTrackerAppearance;
		}

		private void TreeViewItemTrackerSettings_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Settings_Header");
			OptionsContent = OptionsTrackerSettings;
		}

		private void TreeViewItemTrackerLogging_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Logging_Header");
			OptionsContent = OptionsTrackerLogging;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void TreeViewItemStreamingTwitchExtension_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = "Twitch Extension";
			OptionsContent = OptionsStreamingTwitchExtension;
			OptionsStreamingTwitchExtension.UpdateTwitchData();
			OptionsStreamingTwitchExtension.UpdateAccountName();
		}

		private void TreeViewItemStreamingCapturableOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = "Capturable Overlay";
			OptionsContent = OptionsStreamingCapturableOverlay;
		}

		public bool TwitchExtensionMenuSelected => Equals(OptionsContent, OptionsStreamingTwitchExtension);
	}
}
