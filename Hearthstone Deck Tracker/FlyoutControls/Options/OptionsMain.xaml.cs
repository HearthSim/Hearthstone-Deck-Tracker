using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.FlyoutControls.Options;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.HSReplay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class OptionsMain : INotifyPropertyChanged
	{
		public readonly OverlayDeckWindows OptionsOverlayDeckWindows = new OverlayDeckWindows();
		public readonly OverlayGeneral OptionsOverlayGeneral = new OverlayGeneral();
		public readonly OverlayOpponent OptionsOverlayOpponent = new OverlayOpponent();
		public readonly OverlayPlayer OptionsOverlayPlayer = new OverlayPlayer();
		public readonly TrackerAppearance OptionsTrackerAppearance = new TrackerAppearance();
		public readonly TrackerBackups OptionsTrackerBackups = new TrackerBackups();
		public readonly TrackerMainWindow OptionsTrackerMainWindow = new TrackerMainWindow();
		public readonly TrackerHotKeys OptionsTrackerHotKeys = new TrackerHotKeys();
		public readonly TrackerImporting OptionsTrackerImporting = new TrackerImporting();
		public readonly TrackerNotifications OptionsTrackerNotifications = new TrackerNotifications();
		public readonly TrackerPlugins OptionsTrackerPlugins = new TrackerPlugins();
		public readonly TrackerSystem OptionsTrackerSystem = new TrackerSystem();
		public readonly TrackerStats OptionsTrackerStats = new TrackerStats();
		public readonly HSReplayAccount OptionsHSReplayAccount = new HSReplayAccount();
		public readonly HSReplayReplays OptionsHSReplayReplays = new HSReplayReplays();
		public readonly HSReplayCollection OptionsHSReplayCollection = new HSReplayCollection();
		public readonly StreamingTwitchExtension OptionsStreamingTwitchExtension = new StreamingTwitchExtension();
		public readonly StreamingCapturableOverlay OptionsStreamingCapturableOverlay = new StreamingCapturableOverlay();
		public readonly OverlayMulligan OptionsOverlayMulligan = new OverlayMulligan();
		public readonly OverlayTheOutfinder OptionsOverlayTheOutfinder = new OverlayTheOutfinder();
		public readonly OverlayCounters OptionsOverlayCounters = new OverlayCounters();
		public readonly OverlayRelatedCards OptionsOverlayRelatedCards = new OverlayRelatedCards();
		public readonly OverlayBattlegrounds OptionsOverlayBattlegrounds = new OverlayBattlegrounds();
		public readonly OverlayArena OptionsOverlayArena = new OverlayArena();
		public readonly OverlayMercenaries OptionsOverlayMercenaries = new OverlayMercenaries();
		private readonly OptionsSearch _search;
		private TreeViewItem? _pageBeforeEmptySearch;
		private string? _contentHeader;
		private object? _optionsContent;
		private readonly object[] _hsreplayOptions;

		public OptionsMain()
		{
			_hsreplayOptions = new object[] { OptionsHSReplayAccount, OptionsHSReplayCollection ,OptionsHSReplayReplays };
			InitializeComponent();
			Helper.OptionsMain = this;
			_search = new OptionsSearch(Pages);
			try
			{
				foreach(var treeItem in TreeViewOptions.Items.Cast<TreeViewItem>())
					treeItem.IsExpanded = true;
				TreeViewItemHSReplayAccount.IsSelected = true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			AdvancedOptions.Instance.PropertyChanged += (_, e) =>
			{
				// wait for the advanced options bindings to update first
				if(e.PropertyName == nameof(AdvancedOptions.Show))
					Dispatcher.BeginInvoke(new Action(() => ApplySearchFilter(false)));
			};
			LocalizeDictionary.Instance.PropertyChanged += (_, e) =>
			{
				// wait for the localized headers and checkbox texts to update first
				if(e.PropertyName == nameof(LocalizeDictionary.Culture))
					Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ApplySearchFilter(false)));
			};
		}

		public void FocusSearch()
		{
			TextBoxSearch.Focus();
			TextBoxSearch.SelectAll();
		}

		public void ShowPage(TreeViewItem page)
		{
			TextBoxSearch.Clear();
			ApplySearchToPageFilters("", null);
			page.IsSelected = true;
		}

		private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e) => ApplySearchFilter(true);

		private void TextBoxSearch_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Escape || TextBoxSearch.Text.Length == 0)
				return;
			TextBoxSearch.Clear();
			e.Handled = true;
		}

		private void ApplySearchFilter(bool queryChanged)
		{
			var query = TextBoxSearch.Text.Trim();
			List<TreeViewItem>? matches = null;
			var hasAdvanced = false;
			if(OptionsSearch.IsValidQuery(query))
				(matches, hasAdvanced) = _search.FindPages(query);
			AdvancedOptions.Instance.Revealed = hasAdvanced;
			foreach(var category in TreeViewOptions.Items.Cast<TreeViewItem>())
			{
				var anyVisible = false;
				foreach(var page in category.Items.Cast<TreeViewItem>())
				{
					var visible = matches?.Contains(page) ?? true;
					SetFilterVisibility(page, visible);
					anyVisible |= visible;
				}
				SetFilterVisibility(category, anyVisible);
			}
			// clearing the search keeps the page filters until a new query is typed
			if(queryChanged && query.Length > 0)
				ApplySearchToPageFilters(query, matches);
			if(matches == null)
			{
				// the selected page may have been an advanced page revealed by the search
				if(OptionsContent == null || TreeViewOptions.SelectedItem is not TreeViewItem { Visibility: Visibility.Visible })
					(_pageBeforeEmptySearch is { Visibility: Visibility.Visible } ? _pageBeforeEmptySearch : FirstVisiblePage()).IsSelected = true;
				_pageBeforeEmptySearch = null;
				return;
			}
			if(matches.Count == 0)
			{
				if(TreeViewOptions.SelectedItem is TreeViewItem previous)
				{
					_pageBeforeEmptySearch = IsPage(previous) ? previous : null;
					previous.IsSelected = false;
				}
				ContentHeader = null;
				OptionsContent = null;
				return;
			}
			_pageBeforeEmptySearch = null;
			if(queryChanged || TreeViewOptions.SelectedItem is not TreeViewItem selected || !matches.Contains(selected))
				matches[0].IsSelected = true;
		}

		private void ApplySearchToPageFilters(string query, List<TreeViewItem>? matches)
		{
			foreach(var (treeViewItem, page) in Pages)
			{
				if(page is IOptionsPageWithFilter filterable)
					filterable.Filter = matches?.Contains(treeViewItem) == true && filterable.HasFilterMatches(query) ? query : "";
			}
		}

		private bool IsPage(TreeViewItem item) => Pages.Any(x => x.TreeViewItem == item);

		private TreeViewItem FirstVisiblePage() => Pages.First(x => x.TreeViewItem.Visibility == Visibility.Visible).TreeViewItem;

		// keep the advanced options visibility bindings intact
		private static void SetFilterVisibility(TreeViewItem item, bool visible)
		{
			if(visible && item.GetBindingExpression(VisibilityProperty) is { } binding)
				binding.UpdateTarget();
			else
				item.SetCurrentValue(VisibilityProperty, visible ? Visibility.Visible : Visibility.Collapsed);
		}

		public string? ContentHeader
		{
			get => _contentHeader;
			set
			{
				_contentHeader = value;
				OnPropertyChanged();
			}
		}

		public object? OptionsContent
		{
			get => _optionsContent;
			set
			{
				_optionsContent = value;
				ScrollContentToTop();
				OnPropertyChanged();
				OnPropertyChanged(nameof(HSReplayHeaderVisibility));
				OnPropertyChanged(nameof(ContentScrollBarVisibility));
			}
		}

		public (TreeViewItem TreeViewItem, UserControl Page)[] Pages => new (TreeViewItem, UserControl)[]
		{
			(TreeViewItemHSReplayAccount, OptionsHSReplayAccount),
			(TreeViewItemHSReplayCollection, OptionsHSReplayCollection),
			(TreeViewItemHSReplayReplays, OptionsHSReplayReplays),
			(TreeViewItemOverlayGeneral, OptionsOverlayGeneral),
			(TreeViewItemOverlayMulligan, OptionsOverlayMulligan),
			(TreeViewItemOverlayTheOutfinder, OptionsOverlayTheOutfinder),
			(TreeViewItemOverlayCounters, OptionsOverlayCounters),
			(TreeViewItemOverlayRelatedCards, OptionsOverlayRelatedCards),
			(TreeViewItemOverlayDeckWindows, OptionsOverlayDeckWindows),
			(TreeViewItemOverlayPlayer, OptionsOverlayPlayer),
			(TreeViewItemOverlayOpponent, OptionsOverlayOpponent),
			(TreeViewItemOverlayBattlegrounds, OptionsOverlayBattlegrounds),
			(TreeViewItemOverlayArena, OptionsOverlayArena),
			(TreeViewItemOverlayMercenaries, OptionsOverlayMercenaries),
			(TreeViewItemTrackerMainWindow, OptionsTrackerMainWindow),
			(TreeViewItemTrackerStats, OptionsTrackerStats),
			(TreeViewItemTrackerImporting, OptionsTrackerImporting),
			(TreeViewItemTrackerNotifications, OptionsTrackerNotifications),
			(TreeViewItemTrackerPlugins, OptionsTrackerPlugins),
			(TreeViewItemTrackerHotKeys, OptionsTrackerHotKeys),
			(TreeViewItemTrackerBackups, OptionsTrackerBackups),
			(TreeViewItemTrackerAppearance, OptionsTrackerAppearance),
			(TreeViewItemTrackerSystem, OptionsTrackerSystem),
			(TreeViewItemStreamingCapturableOverlay, OptionsStreamingCapturableOverlay),
			(TreeViewItemStreamingTwitchExtension, OptionsStreamingTwitchExtension),
		};

		public ScrollBarVisibility ContentScrollBarVisibility =>
			OptionsContent is IOptionsPageWithOwnScrolling ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;

		public void Reset()
		{
			TextBoxSearch.Clear();
			TreeViewItemHSReplayAccount.IsSelected = true;
			ScrollContentToTop();
			ScrollTreeToTop();
		}

		private void ScrollTreeToTop() => Helper.FindVisualChildren<ScrollViewer>(TreeViewOptions).FirstOrDefault()?.ScrollToTop();

		private void ScrollContentToTop() => ScrollViewerContent.ScrollToTop();

		public void Load(GameV2 game)
		{
			OptionsOverlayGeneral.Load();
			OptionsOverlayDeckWindows.Load(game);
			OptionsOverlayPlayer.Load(game);
			OptionsOverlayOpponent.Load();
			OptionsTrackerSystem.Load();
			OptionsTrackerImporting.Load();
			OptionsTrackerStats.Load();
			OptionsTrackerMainWindow.Load();
			OptionsTrackerAppearance.Load();
			OptionsTrackerBackups.Load();
			OptionsTrackerNotifications.Load();
			OptionsOverlayMulligan.Load();
			OptionsOverlayTheOutfinder.Load();
			OptionsOverlayBattlegrounds.Load();
			OptionsOverlayArena.Load();
			OptionsOverlayMercenaries.Load();
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_General_Header");
			OptionsContent = OptionsOverlayGeneral;
			OptionsOverlayGeneral.Load();
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

		private void TreeViewItemTrackerMainWindow_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_MainWindow_Header");
			OptionsContent = OptionsTrackerMainWindow;
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

		private void TreeViewItemTrackerSystem_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Tracker_System_Header");
			OptionsContent = OptionsTrackerSystem;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

		private void TreeViewItemMulliganOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Mulligan_Header");
			OptionsContent = OptionsOverlayMulligan;
			OptionsOverlayMulligan.Load();
		}

		private void TreeViewItemOverlayTheOutfinder_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = "The Outfinder";
			OptionsContent = OptionsOverlayTheOutfinder;
			OptionsOverlayTheOutfinder.Load();
		}

		// Note: intentionally not part of Load(GameV2) below. Loading this page builds the counter
		// catalog, which instantiates every counter; that must not happen on the startup path.
		private void TreeViewItemOverlayCounters_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Counters_Header");
			OptionsContent = OptionsOverlayCounters;
			OptionsOverlayCounters.Load();
		}

		// Note: intentionally not part of Load(GameV2) below, for the same reason as the counters
		// page: loading it builds the catalog, which instantiates every related card.
		private void TreeViewItemOverlayRelatedCards_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_RelatedCards_Header");
			OptionsContent = OptionsOverlayRelatedCards;
			OptionsOverlayRelatedCards.Load();
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

		private void TreeViewItemBattlegroundsOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Battlegrounds_Header");
			OptionsContent = OptionsOverlayBattlegrounds;
			OptionsOverlayBattlegrounds.UpdateDisabledState();
			OptionsOverlayBattlegrounds.Load();
		}

		private void TreeViewItemArenaOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Arena_Header");
			OptionsContent = OptionsOverlayArena;
			OptionsOverlayArena.Load();
		}

		private void TreeViewItemMercenariesOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentHeader = LocUtil.Get("Options_Overlay_Mercenaries_Header");
			OptionsContent = OptionsOverlayMercenaries;
		}
	}
}
