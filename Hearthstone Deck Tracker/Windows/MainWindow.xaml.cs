#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.Updating;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Windows.MainWindowControls;
#if(SQUIRREL)
using Squirrel;
#else
using MahApps.Metro.Controls.Dialogs;
#endif
using static System.Windows.Visibility;
#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		internal void ShowReplayFromFileDialog()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Title = "Select Replay File",
					DefaultExt = "*.hdtreplay",
					Filter = "HDT Replay|*.hdtreplay",
					InitialDirectory = Config.Instance.ReplayDir
				};
				var dialogResult = dialog.ShowDialog();
				if(dialogResult == System.Windows.Forms.DialogResult.OK)
					ReplayLauncher.ShowReplay(dialog.FileName, true).Forget();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private void ComboBoxDeckVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized || DeckPickerList.ChangedSelection)
				return;
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			var version = ComboBoxDeckVersion.SelectedItem as SerializableVersion;
			if(version == null || deck.SelectedVersion == version)
				return;
			deck.SelectVersion(version);
			deck.StatsUpdated();
			DeckPickerList.UpdateDeck(deck);
		}

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ButtonCloseStatsFlyout_OnClick(object sender, RoutedEventArgs e) => FlyoutStats.IsOpen = false;

		private async void ButtonSwitchStatsToNewWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.StatsInWindow = true;
			Config.Save();
			StatsFlyoutContentControl.Content = null;
			Core.Windows.StatsWindow.ContentControl.Content = Core.StatsOverview;
			Core.Windows.StatsWindow.WindowState = WindowState.Normal;
			Core.Windows.StatsWindow.Show();
			Core.StatsOverview.UpdateStats();
			FlyoutStats.IsOpen = false;
			await Task.Delay(100);
			Core.Windows.StatsWindow.Activate();
		}

#region Properties

		private bool _initialized => Core.Initialized;

		private double _heightChangeDueToSearchBox;
		public const int SearchBoxHeight = 30;

		public string IntroductionLabelText
			=> Config.Instance.ConstructedAutoImportNew ? "ENTER THE 'PLAY' MENU TO AUTOMATICALLY IMPORT YOUR DECKS" : "ADD NEW DECKS BY CLICKING 'NEW' OR 'IMPORT'";

		public Visibility IntroductionLabelVisibility => DeckList.Instance.Decks.Any() ? Collapsed : Visible;

		public Visibility MenuItemReplayClaimAccountVisibility => Account.Instance.Status == AccountStatus.Anonymous ? Visible : Collapsed;
		public Visibility MenuItemReplayMyAccountVisibility => Account.Instance.Status == AccountStatus.Anonymous ? Collapsed : Visible;

		public Visibility HsReplayButtonVisibility
		{
			get
			{
				var deck = DeckPickerList.SelectedDecks.FirstOrDefault()?.GetSelectedDeckVersion() ?? DeckList.Instance.ActiveDeckVersion;
				if(deck != null && HsReplayDataManager.Decks.AvailableDecks.Contains(deck.ShortId))
					return Visible;
				return Collapsed;
			}
		}

		public void UpdateIntroLabelVisibility() => OnPropertyChanged(nameof(IntroductionLabelVisibility));

#endregion

#region Constructor

		public MainWindow()
		{
			InitializeComponent();
			MainWindowMenu.DataContext = new MainWindowMenuViewModel(this);
			TagControlEdit.StackPanelFilterOptions.Visibility = Collapsed;
			TagControlEdit.GroupBoxSortingAllConstructed.Visibility = Collapsed;
			TagControlEdit.GroupBoxSortingArena.Visibility = Collapsed;
			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			FlyoutNotes.ClosingFinished += (sender, args) => DeckNotesEditor.SaveDeck();
			DeckPickerList.PropertyChanged += DeckPickerList_PropertyChanged;
			WarningFlyout.OnComplete += () =>
			{
				FlyoutWarnings.IsOpen = false;
				Config.Instance.CheckConfigWarnings();
			};
#if(DEV)
			StatusBarDev.Visibility = Visible;
			Title += " [DEV]";
#elif(DEBUG)
			Title += " [DEBUG]";
#endif
			Config.Instance.OnConfigWarning += warning =>
			{
				WarningFlyout.SetConfigWarning(warning);
				FlyoutWarnings.IsOpen = true;
			};
			Config.Instance.CheckConfigWarnings();

#if(!SQUIRREL)
			Updater.Status.Changed += async value =>
			{
				if(value != UpdaterState.Available)
					return;
				ActivateWindow();
				var result = await this.ShowMessageAsync(
					title: Utility.LocUtil.Get("MainWindow_StatusBarUpdate_NewUpdateAvailable"),
					message: Utility.LocUtil.Get("MainWindow_ShowMessage_UpdateDialog"),
					style: MessageDialogStyle.AffirmativeAndNegative,
					settings: new MessageDialogs.Settings
					{
						AffirmativeButtonText = Utility.LocUtil.Get("Button_Download"),
						NegativeButtonText = Utility.LocUtil.Get("Button_Notnow")
					}
				);
				if(result == MessageDialogResult.Affirmative)
					Updater.StartUpdate();
			};
#endif

			HsReplayDataManager.Decks.OnLoaded += () =>
			{
				DeckPickerList.RefreshDisplayedDecks();
				OnPropertyChanged(nameof(HsReplayButtonVisibility));
				Influx.OnHsReplayDataLoaded();
			};

			HSReplayNetOAuth.Authenticated += ActivateWindow;

			Remote.Config.Loaded += data =>
			{
				OnPropertyChanged(nameof(CollectionSyncingBannerVisbiility));
				OnPropertyChanged(nameof(CollectionSyncingBannerRemovable));
			};

			HSReplayNetHelper.CollectionUploaded += (_, _) =>
			{
				OnPropertyChanged(nameof(CollectionSyncingBannerRemovable));
			};

			HSReplayNetOAuth.LoggedOut += () =>
			{
				OnPropertyChanged(nameof(CollectionSyncingBannerVisbiility));
				OnPropertyChanged(nameof(CollectionSyncingBannerRemovable));
			};

			CardDefsManager.CardsChanged += () =>
			{
				Helper.SortCardCollection(ListViewDeck.Items);
			};

			ErrorManager.ErrorAdded += data =>
			{
				OnPropertyChanged(nameof(Errors));
				OnPropertyChanged(nameof(ErrorIconVisibility));
				OnPropertyChanged(nameof(ErrorCount));
				if(data.ShowFlyout)
					FlyoutErrors.IsOpen = true;
			};

			ErrorManager.ErrorRemoved += _ =>
			{
				OnPropertyChanged(nameof(Errors));
				OnPropertyChanged(nameof(ErrorIconVisibility));
				OnPropertyChanged(nameof(ErrorCount));
				if(Errors.Count == 0)
					FlyoutErrors.IsOpen = false;
			};
		}

		private void DeckPickerList_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "ArchivedClassVisible")
				MinHeight += DeckPickerList.ArchivedClassVisible ? DeckPickerClassItem.Big : -DeckPickerClassItem.Big;

			if(e.PropertyName == "VisibilitySearchBar")
			{
				if(DeckPickerList.SearchBarVisibile)
				{
					var oldHeight = Height;
					MinHeight += SearchBoxHeight;
					_heightChangeDueToSearchBox = Height - oldHeight;
				}
				else
				{
					MinHeight -= SearchBoxHeight;
					Height -= _heightChangeDueToSearchBox;
				}
			}
		}

		private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(e.HeightChanged)
				_heightChangeDueToSearchBox = 0;
		}

		public Thickness TitleBarMargin => new Thickness(0, TitlebarHeight, 0, 0);

#endregion

#region GENERAL GUI

		private void MetroWindow_StateChanged(object sender, EventArgs e)
		{
			if(Config.Instance.MinimizeToTray && WindowState == WindowState.Minimized)
				MinimizeToTray();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
			{
				if(!double.IsNaN(Left))
					Config.Instance.TrackerWindowLeft = (int)Left;
				if(!double.IsNaN(Top))
					Config.Instance.TrackerWindowTop = (int)Top;
				if(!double.IsNaN(Width) && Width > 0)
					Config.Instance.WindowWidth = (int)Width;

				var height = Height - _heightChangeDueToSearchBox;
				if(!double.IsNaN(height) && height > 0)
					Config.Instance.WindowHeight = (int)height;

				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				return;
			}

			e.Cancel = true;
			if (Config.Instance.CloseToTray)
				MinimizeToTray();
			else
			{
				// Round trip through Core.Shutdown. This will set IsExiting and close this window again.
				_ = Core.Shutdown();
			}

		}

		private void BtnOptions_OnClick(object sender, RoutedEventArgs e) => FlyoutOptions.IsOpen = true;
		private void BtnHelp_OnClick(object sender, RoutedEventArgs e) => FlyoutHelp.IsOpen = true;
		private void BtnDiscord_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://hsreplay.net/discord/");
		private void BtnTwitter_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://twitter.com/hsreplaynet");

#endregion

#region GENERAL METHODS

		private void MinimizeToTray()
		{
			Core.TrayIcon.NotifyIcon.Visible = true;
			Hide();
			Visibility = Collapsed;
			ShowInTaskbar = false;
		}

		public void ActivateWindow()
		{
			try
			{
				Show();
				ShowInTaskbar = true;
				Activate();
				WindowState = WindowState.Normal;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

#endregion

#region MY DECKS - GUI

		internal void ShowStats(bool arena, bool matches)
		{
			if(Config.Instance.StatsInWindow)
			{
				StatsFlyoutContentControl.Content = null;
				Core.Windows.StatsWindow.ContentControl.Content = Core.StatsOverview;
				Core.Windows.StatsWindow.WindowState = WindowState.Normal;
				Core.Windows.StatsWindow.Show();
				Core.Windows.StatsWindow.Activate();
			}
			else
			{
				Core.Windows.StatsWindow.ContentControl.Content = null;
				StatsFlyoutContentControl.Content = Core.StatsOverview;
				FlyoutStats.IsOpen = true;
			}
			if(arena)
			{
				if(matches)
					Core.StatsOverview.TreeViewItemArenaRunsOverview.IsSelected = true;
				else
				{
					Core.StatsOverview.TreeViewItemArenaRunsSummary.IsSelected = true;
					HSReplayNetClientAnalytics.OnShowPersonalStats(ClickAction.Action.StatsArena, SubFranchise.Arena);
				}
				Core.StatsOverview.ContentControlFilter.Content = Core.StatsOverview.ArenaFilters;
			}
			else
			{
				if(matches)
					Core.StatsOverview.TreeViewItemConstructedGames.IsSelected = true;
				else
				{
					Core.StatsOverview.TreeViewItemConstructedSummary.IsSelected = true;
					HSReplayNetClientAnalytics.OnShowPersonalStats(ClickAction.Action.StatsConstructed, null);
				}
				Core.StatsOverview.ContentControlFilter.Content = Core.StatsOverview.ConstructedFilters;
			}
			Core.StatsOverview.UpdateStats();
		}

		private void DeckPickerList_OnSelectedDeckChanged(DeckPicker sender, List<Deck>? decks)
		{
			var active = DeckList.Instance.ActiveDeck;
			MainWindowMenu.SelectedDecks = (!decks?.Any() ?? false) && active != null ? new List<Deck> { active } : decks ?? new List<Deck>();

			var deck = decks?.FirstOrDefault() ?? active;
			UpdateDeck(deck);
		}

		private void UpdateDeck(Deck? deck)
		{
			if(_displayedDeck != null)
				_displayedDeck.SelectedVersionChanged -= UpdateDisplayedDeck;
			if(deck != null)
				deck.SelectedVersionChanged += UpdateDisplayedDeck;

			_displayedDeck = deck;
			UpdateDisplayedDeck();
		}

		private Deck? _displayedDeck;
		private void UpdateDisplayedDeck()
		{
			var deck = _displayedDeck;

			if(Config.Instance.AutoUseDeck)
				DeckList.Instance.ActiveDeck = deck;

			if(deck != null)
			{
				DeckPickerList.ClearFromCache(deck);

				//set up tags
				TagControlEdit.SetSelectedTags(DeckPickerList.SelectedDecks);
				DeckPickerList.MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
				DeckPickerList.MenuItemQuickSetTag.Items.Refresh();

				if(FlyoutDeckScreenshot.IsOpen)
					DeckScreenshotFlyout.Deck = deck.GetSelectedDeckVersion();
				if(FlyoutDeckExport.IsOpen)
					DeckExportFlyout.Deck = deck.GetSelectedDeckVersion();

				if(FlyoutDeckHistory.IsOpen)
				{
					if(deck.HasVersions)
						DeckHistoryFlyout.Deck = deck;
					else
						FlyoutDeckHistory.IsOpen = false;
				}
			}

			OnPropertyChanged(nameof(HsReplayButtonVisibility));

			var version = deck?.GetSelectedDeckVersion();
			ListViewDeck.ItemsSource = null;
			// always update the sideboard to ensure we hide the header if empty
			PlayerSideboards.Update(version?.Sideboards, true);
			if(version != null)
			{
				ListViewDeck.ItemsSource = Helper.ResolveZilliax3000(version.Cards, version.Sideboards);
				Helper.SortCardCollection(ListViewDeck.Items);
			}

			ManaCurveMyDecks.SetDeck(deck);

			ComboBoxDeckVersion.ItemsSource = deck?.VersionsIncludingSelf;
			ComboBoxDeckVersion.SelectedItem = deck?.SelectedVersion;
			PanelVersionComboBox.Visibility = deck is { HasVersions: true } ? Visible : Collapsed;

			GroupBoxHsReplayDeckInfo.Visibility = deck?.IsArenaDeck == true || deck?.IsDungeonDeck == true || deck?.IsDuelsDeck == true ? Collapsed : Visible;
			DeckCharts.SetDeck(deck);
			HsReplayDeckInfo.SetDeck(deck);
		}

#endregion

#region Errors

		public ObservableCollection<Error> Errors => ErrorManager.Errors;
		public Visibility ErrorIconVisibility => ErrorManager.ErrorIconVisibility;
		public string ErrorCount => ErrorManager.Errors.Count > 1 ? $"({ErrorManager.Errors.Count})" : "";

		private void BtnErrors_OnClick(object sender, RoutedEventArgs e) => FlyoutErrors.IsOpen = !FlyoutErrors.IsOpen;

#endregion

		private void HyperlinkUpdateNow_OnClick(object sender, RoutedEventArgs e) => Updater.StartUpdate();

		private void HyperlinkDevDiscord_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://discord.gg/hearthsim-devs");

		public void ShowDeckEditorFlyout(Deck deck, bool isNewDeck)
		{
			DeckEditorFlyout.SetDeck(deck, isNewDeck);
			FlyoutDeckEditor.IsOpen = true;
		}

		internal async void ShowNewDeckMessage(string hero)
		{
			var deck = new Deck {Class = hero};
			var type = await this.ShowDeckTypeDialog();
			if(type == null)
				return;
			if(type == DeckType.Arena)
				deck.IsArenaDeck = true;
			else if(type == DeckType.Brawl)
			{
				if(!DeckList.Instance.AllTags.Contains("Brawl"))
				{
					DeckList.Instance.AllTags.Add("Brawl");
					DeckList.Save();
					ReloadTags();
				}
				deck.Tags.Add("Brawl");
			}
			ShowDeckEditorFlyout(deck, true);
		}

		private void MyGamesFilters_OnClick(object sender, MouseButtonEventArgs e)
		{
			Options.TreeViewItemTrackerStats.IsSelected = true;
			FlyoutOptions.IsOpen = true;
		}

		public void DisplayFiltersUpdated()
		{
			foreach(var deck in DeckList.Instance.Decks)
				deck.StatsUpdated();
			DeckPickerList.UpdateDecks();
			Core.Overlay.Update(true);
			var selected = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			DeckCharts.SetDeck(selected);
			HsReplayDeckInfo.SetDeck(selected);
			OnPropertyChanged(nameof(ActiveFiltersWarningVisibility));
		}

		public Visibility ActiveFiltersWarningVisibility => Config.Instance.DisplayedMode != GameMode.All
															|| Config.Instance.DisplayedStats != DisplayedStats.All
															|| Config.Instance.DisplayedTimeFrame != DisplayedTimeFrame.AllTime ? Visible : Collapsed;

		public void UpdateMyGamesPanelVisibility()
		{
			const int baseWidth = 1100;
			if(Config.Instance.ShowMyGamesPanel)
			{
				MyGamesPanel.Visibility = Visible;
				MinWidth = baseWidth;
			}
			else
			{
				MinWidth = baseWidth - MyGamesPanel.Width - MyGamesPanel.Margin.Left;
				MyGamesPanel.Visibility = Collapsed;
			}
		}

		private void MainWindow_OnActivated(object sender, EventArgs e)
		{
			Influx.OnMainWindowActivated();
			UITheme.RefreshWindowsAccent().Forget();
			if(Options.TwitchExtensionMenuSelected && Options.OptionsStreamingTwitchExtension.AwaitingTwitchAccountConnection)
				Options.OptionsStreamingTwitchExtension.RefreshTwitchAccounts();
		}

		private void MainWindow_OnDeactivated(object sender, EventArgs e)
		{
			Influx.OnMainWindowDeactivated();
		}

		private void RemovableBanner_OnClick(object sender, EventArgs e)
		{
			var authenticated = HSReplayNetOAuth.IsFullyAuthenticated;
			var collectionSynced = Account.Instance.CollectionState.Any();
			Influx.OnCollectionSyncingBannerClicked(authenticated, collectionSynced);
			if(!authenticated || !collectionSynced)
			{
				Options.TreeViewItemHSReplayCollection.IsSelected = true;
				FlyoutOptions.IsOpen = true;
				if(!authenticated)
				{
					var successUrl = Helper.BuildHsReplayNetUrl("decks", "collection_syncing_banner",
						new[] { "modal=collection" });
					HSReplayNetHelper.TryAuthenticate(successUrl).Forget();
				}
			}
			else
				HSReplayNetHelper.OpenDecksUrlWithCollection("collection_syncing_banner");
		}

		private void RemovableBanner_OnClose(object sender, EventArgs e)
		{
			Influx.OnCollectionSyncingBannerClosed();
			Config.Instance.HideCollectionSyncingBanner = CollectionBannerId;
			Config.Save();
			OnPropertyChanged(nameof(CollectionSyncingBannerVisbiility));
		}

		public Visibility CollectionSyncingBannerVisbiility
		{
			get
			{
				if(!(Remote.Config.Data?.CollectionBanner?.Visible ?? true))
					return Collapsed;
				if(Config.Instance.HideCollectionSyncingBanner >= CollectionBannerId)
				{
					var synced = Account.Instance.CollectionState.Any();
					var removablePostSync = Remote.Config.Data?.CollectionBanner?.RemovablePostSync ?? false;
					var removablePreSync = Remote.Config.Data?.CollectionBanner?.RemovablePreSync ?? false;
					if(synced && removablePostSync || !synced && removablePreSync)
						return Collapsed;
				}
				return Visible;
			}
		}

		private int CollectionBannerId => Remote.Config.Data?.CollectionBanner?.RemovalId ?? 0;

		public bool CollectionSyncingBannerRemovable
		{
			get
			{
				var synced = Account.Instance.CollectionState.Any();
				return !synced && (Remote.Config.Data?.CollectionBanner?.RemovablePreSync ?? false)
					|| synced && (Remote.Config.Data?.CollectionBanner?.RemovablePostSync ?? false);
			}
		}
	}
}
