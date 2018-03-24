using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.FlyoutControls.Options;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.HSReplay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
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
		public readonly TrackerNotifications OptionsTrackerNotifications = new TrackerNotifications();
		public readonly TrackerPlugins OptionsTrackerPlugins = new TrackerPlugins();
		public readonly TrackerSettings OptionsTrackerSettings = new TrackerSettings();
		public readonly TrackerStats OptionsTrackerStats = new TrackerStats();
		public readonly HSReplayAccount OptionsHSReplayAccount = new HSReplayAccount();
		public readonly HSReplayReplays OptionsHSReplayReplays = new HSReplayReplays();
		public readonly HSReplayCollection OptionsHSReplayCollection = new HSReplayCollection();
		public readonly StreamingTwitchExtension OptionsStreamingTwitchExtension = new StreamingTwitchExtension();
		public readonly StreamingCapturableOverlay OptionsStreamingCapturableOverlay = new StreamingCapturableOverlay();
		public readonly OptionsSearch OptionsSearch = new OptionsSearch();
		private string _contentHeader;
		private object _optionsContent;
		private readonly object[] _hsreplayOptions;

		public OptionsMain()
		{
			_hsreplayOptions = new object[] { OptionsHSReplayAccount, OptionsHSReplayCollection ,OptionsHSReplayReplays };
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
				OnPropertyChanged(nameof(HSReplayHeaderVisibility));
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
			OptionsTrackerStats.Load();
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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void TreeViewItemStreamingTwitchExtension_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Streaming_TwitchExt_Header");
			OptionsContent = OptionsStreamingTwitchExtension;
			OptionsStreamingTwitchExtension.UpdateTwitchData();
			OptionsStreamingTwitchExtension.UpdateAccountName();
		}

		private void TreeViewItemStreamingCapturableOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Streaming_CaptureableOverlay_Header");
			OptionsContent = OptionsStreamingCapturableOverlay;
		}

		public bool TwitchExtensionMenuSelected => Equals(OptionsContent, OptionsStreamingTwitchExtension);

		public Visibility HSReplayHeaderVisibility =>
			_hsreplayOptions.Any(x => x == OptionsContent) ? Visibility.Visible : Visibility.Collapsed;

		public ICommand HSReplayBannerCommand =>
			new Command(() => Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("", "options_banner")));

		private void TreeViewItemHSReplayAccount_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_HSReplay_Account_Header");
			OptionsContent = OptionsHSReplayAccount;
		}

		private void TreeViewItemHSReplayReplays_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_Replays_Header");
			OptionsContent = OptionsHSReplayReplays;
		}

		private void TreeViewItemHSReplayCollection_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_HSReplay_Collection_Header");
			OptionsContent = OptionsHSReplayCollection;
			OptionsHSReplayCollection.UpdateSyncAge();
		}
	}
}
