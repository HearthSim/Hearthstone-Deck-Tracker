#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Updating;
using MahApps.Metro.Controls.Dialogs;
#if(SQUIRREL)
	using Squirrel;
#endif
using static System.Windows.Visibility;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		private const string LocLink = "MainWindow_Menu_Deck_LinkUrl";
		private const string LocLinkNew = "MainWindow_Menu_Deck_LinkNewUrl";

		public event PropertyChangedEventHandler PropertyChanged;

		public async void UseDeck(Deck selected)
		{
			if(selected != null)
				DeckList.Instance.ActiveDeck = selected;
			await Core.Reset();
		}

		internal void UpdateMenuItemVisibility()
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			MenuItemMoveDecktoArena.Visibility = deck.IsArenaDeck ? Collapsed : Visible;
			MenuItemMoveDeckToConstructed.Visibility = deck.IsArenaDeck ? Visible : Collapsed;
			MenuItemMissingCards.Visibility = deck.MissingCards.Any() ? Visible : Collapsed;
			MenuItemSetDeckUrl.Visibility = deck.IsArenaDeck ? Collapsed : Visible;
			MenuItemSetDeckUrl.Header = string.IsNullOrEmpty(deck.Url) ? LocUtil.Get(LocLink, true) : LocUtil.Get(LocLinkNew, true);
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(deck.Url) ? Collapsed : Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(deck.Url) ? Collapsed : Visible;
			MenuItemArchive.Visibility = DeckPickerList.SelectedDecks.Any(d => !d.Archived) ? Visible : Collapsed;
			MenuItemUnarchive.Visibility = DeckPickerList.SelectedDecks.Any(d => d.Archived) ? Visible : Collapsed;
			SeparatorDeck1.Visibility = deck.IsArenaDeck ? Collapsed : Visible;
		}

		public void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if(selected == null)
				return;
			ListViewDeck.ItemsSource = selected.GetSelectedDeckVersion().Cards;
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
		}

		private void UpdateDeckHistoryPanel(Deck selected, bool isNewDeck)
		{
			DeckHistoryPanel.Children.Clear();
			DeckCurrentVersion.Text = $"v{selected.SelectedVersion.Major}.{selected.SelectedVersion.Minor}";
			if(isNewDeck)
			{
				MenuItemSaveVersionCurrent.IsEnabled = false;
				MenuItemSaveVersionMinor.IsEnabled = false;
				MenuItemSaveVersionMajor.IsEnabled = false;
				MenuItemSaveVersionCurrent.Visibility = Collapsed;
				MenuItemSaveVersionMinor.Visibility = Collapsed;
				MenuItemSaveVersionMajor.Visibility = Collapsed;
			}
			else
			{
				MenuItemSaveVersionCurrent.IsEnabled = true;
				MenuItemSaveVersionMinor.IsEnabled = true;
				MenuItemSaveVersionMajor.IsEnabled = true;
				MenuItemSaveVersionCurrent.Visibility = Visible;
				MenuItemSaveVersionMinor.Visibility = Visible;
				MenuItemSaveVersionMajor.Visibility = Visible;
				MenuItemSaveVersionCurrent.Header = _newDeck.Version.ToString("v{M}.{m} (current)");
				MenuItemSaveVersionMinor.Header = $"v{_newDeck.Version.Major}.{_newDeck.Version.Minor + 1}";
				MenuItemSaveVersionMajor.Header = $"v{_newDeck.Version.Major + 1}.{0}";
			}

			if(selected.Versions.Count > 0)
			{
				var current = selected;
				foreach(var prevVersion in selected.Versions.OrderByDescending(d => d.Version))
				{
					var versionChange = new DeckVersionChange
					{
						Label = {Text = $"{prevVersion.Version.ToString("v{M}.{m}")} -> {current.Version.ToString("v{M}.{m}")}"},
						ListViewDeck = {ItemsSource = current - prevVersion}
					};
					DeckHistoryPanel.Children.Add(versionChange);
					current = prevVersion;
				}
			}
		}

		public void AutoDeckDetection(bool enable)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoDeckDetection = enable;
			Config.Save();
			Options.OptionsTrackerGeneral.CheckBoxAutoDeckDetection.IsChecked = enable;
			Core.TrayIcon.SetContextMenuProperty(TrayIcon.AutoSelectDeckMenuItemName, TrayIcon.CheckedProperty, enable);
		}

		public void SortClassCardsFirst(bool classFirst)
		{
			if(!_initialized)
				return;
			Options.OptionsTrackerGeneral.CheckBoxClassCardsFirst.IsChecked = classFirst;
			Config.Instance.CardSortingClassFirst = classFirst;
			Config.Save();
			Helper.SortCardCollection(Core.MainWindow.ListViewDeck.ItemsSource, classFirst);
			Core.TrayIcon.SetContextMenuProperty(TrayIcon.ClassCardsFirstMenuItemName, TrayIcon.CheckedProperty, classFirst);
		}

		private void MenuItemReplayFromFile_OnClick(object sender, RoutedEventArgs e)
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
					ReplayLauncher.ShowReplay(dialog.FileName, true);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private void MenuItemReplaySelectGame_OnClick(object sender, RoutedEventArgs e) => ShowStats(false, true);

		private void MenuItemSaveVersionCurrent_OnClick(object sender, RoutedEventArgs e) => SaveDeckWithOverwriteCheck(_newDeck.Version);

		private void MenuItemSaveVersionMinor_OnClick(object sender, RoutedEventArgs e) => SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMinor(_newDeck.Version));

		private void MenuItemSaveVersionMajor_OnClick(object sender, RoutedEventArgs e) => SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMajor(_newDeck.Version));

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
			DeckList.Save();
			DeckPickerList.UpdateDecks(forceUpdate: new[] {deck});
			UpdateDeckList(deck);
			ManaCurveMyDecks.UpdateValues();
			OnPropertyChanged(nameof(HsReplayButtonVisibility));
			if(deck.Equals(DeckList.Instance.ActiveDeck))
				UseDeck(deck);
			Console.WriteLine(version);
		}

		private void MenuItemSaveAsNew_OnClick(object sender, RoutedEventArgs e) => SaveDeckWithOverwriteCheck(new SerializableVersion(1, 0), true);

		private void DeckPickerList_OnOnDoubleClick(DeckPicker sender, Deck deck)
		{
			if(deck?.Equals(DeckList.Instance.ActiveDeck) ?? true)
				return;
			SelectDeck(deck, true);
		}

		private void BtnCloseNews_OnClick(object sender, RoutedEventArgs e) => NewsManager.ToggleNewsVisibility();

		private void BtnNewsPrevious_OnClick(object sender, RoutedEventArgs e) => NewsManager.PreviousNewsItem();

		private void BtnNewsNext_OnClick(object sender, RoutedEventArgs e) => NewsManager.NextNewsItem();

		private void MetroWindow_LocationChanged(object sender, EventArgs e) => MovedLeft = null;


		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void BtnStartHearthstone_Click(object sender, RoutedEventArgs e) => Helper.StartHearthstoneAsync().Forget();

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

		[Obsolete("Use API.Core.OverlayWindow", true)] //for plugin compatibility
		public OverlayWindow Overlay => Core.Overlay;

		private bool _initialized => Core.Initialized;

		public bool EditingDeck;
		private Deck _newDeck;
		private bool _newDeckUnsavedChanges;
		private Deck _originalDeck;

		private double _heightChangeDueToSearchBox;
		public const int SearchBoxHeight = 30;

		public int StatusBarNewsHeight => 20;

		public bool ShowToolTip => Config.Instance.TrackerCardToolTips;

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
				if(deck != null && HsReplayDecks.AvailableDecks.Contains(deck.ShortId))
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
			Trace.Listeners.Add(new TextBoxTraceListener(Options.OptionsTrackerLogging.TextBoxLog));
			EnableMenuItems(false);
			TagControlEdit.StackPanelFilterOptions.Visibility = Collapsed;
			TagControlEdit.GroupBoxSortingAllConstructed.Visibility = Collapsed;
			TagControlEdit.GroupBoxSortingArena.Visibility = Collapsed;
			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			FlyoutNotes.ClosingFinished += (sender, args) => DeckNotesEditor.SaveDeck();
			WarningFlyout.OnComplete += () =>
			{
				FlyoutWarnings.IsOpen = false;
				Config.Instance.CheckConfigWarnings();
			};
#if(DEBUG)
			Title += " [DEBUG]";
#endif
#if(DEV)
			StatusBarDev.Visibility = Visible;
#endif
			Config.Instance.OnConfigWarning += warning =>
			{
				WarningFlyout.SetConfigWarning(warning);
				FlyoutWarnings.IsOpen = true;
			};
			Config.Instance.CheckConfigWarnings();

			HsReplayDecks.OnLoaded += () =>
			{
				DeckPickerList.RefreshDisplayedDecks();
				OnPropertyChanged(nameof(HsReplayButtonVisibility));
			};
		}

		public void LoadAndUpdateDecks()
		{
			UpdateDeckList(DeckList.Instance.ActiveDeck);
			UpdateDbListView();
			SelectDeck(DeckList.Instance.ActiveDeck, true);
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			DeckPickerList.PropertyChanged += DeckPickerList_PropertyChanged;
			DeckPickerList.UpdateDecks();
			DeckPickerList.UpdateArchivedClassVisibility();
			ManaCurveMyDecks.UpdateValues();
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

		private async void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				Log.Info("Shutting down...");
				Influx.OnAppExit(Helper.GetCurrentVersion());
				Core.UpdateOverlay = false;
				Core.Update = false;

				//wait for update to finish, might otherwise crash when overlay gets disposed
				for(var i = 0; i < 100; i++)
				{
					if(Core.CanShutdown)
						break;
					await Task.Delay(50);
				}

				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				//Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;
				Config.Instance.SelectedDeckPickerClasses = DeckPickerList.SelectedClasses.ToArray();

				Config.Instance.WindowWidth = (int)(Width - (GridNewDeck.Visibility == Visible ? GridNewDeck.ActualWidth : 0));
				Config.Instance.WindowHeight = (int)(Height - _heightChangeDueToSearchBox);
				Config.Instance.TrackerWindowTop = (int)Top;
				Config.Instance.TrackerWindowLeft = (int)(Left + (MovedLeft ?? 0));

				//position of add. windows is NaN if they were never opened.
				if(!double.IsNaN(Core.Windows.PlayerWindow.Left))
					Config.Instance.PlayerWindowLeft = (int)Core.Windows.PlayerWindow.Left;
				if(!double.IsNaN(Core.Windows.PlayerWindow.Top))
					Config.Instance.PlayerWindowTop = (int)Core.Windows.PlayerWindow.Top;
				Config.Instance.PlayerWindowHeight = (int)Core.Windows.PlayerWindow.Height;

				if(!double.IsNaN(Core.Windows.OpponentWindow.Left))
					Config.Instance.OpponentWindowLeft = (int)Core.Windows.OpponentWindow.Left;
				if(!double.IsNaN(Core.Windows.OpponentWindow.Top))
					Config.Instance.OpponentWindowTop = (int)Core.Windows.OpponentWindow.Top;
				Config.Instance.OpponentWindowHeight = (int)Core.Windows.OpponentWindow.Height;

				if(!double.IsNaN(Core.Windows.TimerWindow.Left))
					Config.Instance.TimerWindowLeft = (int)Core.Windows.TimerWindow.Left;
				if(!double.IsNaN(Core.Windows.TimerWindow.Top))
					Config.Instance.TimerWindowTop = (int)Core.Windows.TimerWindow.Top;
				Config.Instance.TimerWindowHeight = (int)Core.Windows.TimerWindow.Height;
				Config.Instance.TimerWindowWidth = (int)Core.Windows.TimerWindow.Width;

				Core.TrayIcon.NotifyIcon.Visible = false;
				Core.Overlay.Close();
				await Core.StopLogWacher();
				Core.Windows.TimerWindow.Shutdown();
				Core.Windows.PlayerWindow.Shutdown();
				Core.Windows.OpponentWindow.Shutdown();
				Config.Save();
				DeckList.Save();
				DeckStatsList.Save();
				PluginManager.SavePluginsSettings();
				PluginManager.Instance.UnloadPlugins();
			}
			catch(Exception)
			{
				//doesnt matter
			}
			finally
			{
				Application.Current.Shutdown();
			}
		}

		private void BtnOptions_OnClick(object sender, RoutedEventArgs e) => FlyoutOptions.IsOpen = true;
		private void BtnHelp_OnClick(object sender, RoutedEventArgs e) => FlyoutHelp.IsOpen = true;

#endregion

#region GENERAL METHODS
		
		private void MinimizeToTray()
		{
			Core.TrayIcon.NotifyIcon.Visible = true;
			Hide();
			Visibility = Collapsed;
			ShowInTaskbar = false;
		}


		public void Restart()
		{
#if(SQUIRREL)
			UpdateManager.RestartApp();
#else
			Close();
			Process.Start(Application.ResourceAssembly.Location);
			if(Application.Current != null)
				Application.Current.Shutdown();
#endif
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

		private void BtnArenaStats_Click(object sender, RoutedEventArgs e) => ShowStats(true, false);

		private void BtnConstructedStats_Click(object sender, RoutedEventArgs e) => ShowStats(false, false);

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
					Core.StatsOverview.TreeViewItemArenaRunsSummary.IsSelected = true;
				Core.StatsOverview.ContentControlFilter.Content = Core.StatsOverview.ArenaFilters;
			}
			else
			{
				if(matches)
					Core.StatsOverview.TreeViewItemConstructedGames.IsSelected = true;
				else
					Core.StatsOverview.TreeViewItemConstructedSummary.IsSelected = true;
				Core.StatsOverview.ContentControlFilter.Content = Core.StatsOverview.ConstructedFilters;
			}
			Core.StatsOverview.UpdateStats();
		}

		private void DeckPickerList_OnSelectedDeckChanged(DeckPicker sender, Deck deck)
		{
			SelectDeck(deck, Config.Instance.AutoUseDeck);
			UpdateMenuItemVisibility();
		}

		public void SelectDeck(Deck deck, bool setActive)
		{
			if(DeckList.Instance.ActiveDeck != null)
				DeckPickerList.ClearFromCache(DeckList.Instance.ActiveDeck);
			if(deck != null)
			{
				//set up notes
				DeckNotesEditor.SetDeck(deck);
				var flyoutHeader = deck.Name.Length >= 20 ? string.Join("", deck.Name.Take(17)) + "..." : deck.Name;
				FlyoutNotes.Header = flyoutHeader;

				//set up tags
				TagControlEdit.SetSelectedTags(DeckPickerList.SelectedDecks);
				MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
				MenuItemQuickSetTag.Items.Refresh();
				DeckPickerList.MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
				DeckPickerList.MenuItemQuickSetTag.Items.Refresh();

				OnPropertyChanged(nameof(HsReplayButtonVisibility));

				//set and save last used deck for class
				if(setActive)
				{
					while(DeckList.Instance.LastDeckClass.Any(ldc => ldc.Class == deck.Class))
					{
						var lastSelected = DeckList.Instance.LastDeckClass.FirstOrDefault(ldc => ldc.Class == deck.Class);
						if(lastSelected != null)
							DeckList.Instance.LastDeckClass.Remove(lastSelected);
						else
							break;
					}
					if(Core.Initialized)
					{
						DeckList.Instance.LastDeckClass.Add(new DeckInfo { Class = deck.Class, Name = deck.Name, Id = deck.DeckId });
						DeckList.Save();
					}

					Log.Info($"Switched to deck: {deck.Name} ({deck.SelectedVersion.ShortVersionString})");

					int useNoDeckMenuItem = Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey(TrayIcon.UseNoDeckMenuItemName);
					Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Checked = false;
				}
			}
			else
			{
				Core.Game.IsUsingPremade = false;

				if(DeckList.Instance.ActiveDeck != null)
					DeckList.Instance.ActiveDeck.IsSelectedInGui = false;

				DeckList.Instance.ActiveDeck = null;
				if(setActive)
					DeckPickerList.DeselectDeck();

				var useNoDeckMenuItem = Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey(TrayIcon.UseNoDeckMenuItemName);
				Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Checked = true;
			}

			if(setActive)
				UseDeck(deck);
			DeckPickerList.SelectDeck(deck);
			UpdateDeckList(deck);
			EnableMenuItems(deck != null);
			ManaCurveMyDecks.SetDeck(deck);
			UpdatePanelVersionComboBox(deck);
			if(setActive)
			{
				Core.Overlay.ListViewPlayer.Items.Refresh();
				Core.Windows.PlayerWindow.ListViewPlayer.Items.Refresh();
			}
			DeckManagerEvents.OnDeckSelected.Execute(deck);
		}

		public void SelectLastUsedDeck()
		{
			var lastSelected = DeckList.Instance.LastDeckClass.LastOrDefault();
			var deck = DeckList.Instance.Decks.FirstOrDefault(d => lastSelected == null || d.DeckId == lastSelected.Id);
			if(deck == null)
				return;
			DeckPickerList.SelectDeck(deck);
			SelectDeck(deck, true);
		}

		private void UpdatePanelVersionComboBox(Deck deck)
		{
			ComboBoxDeckVersion.ItemsSource = deck?.VersionsIncludingSelf;
			ComboBoxDeckVersion.SelectedItem = deck?.SelectedVersion;
			PanelVersionComboBox.Visibility = deck != null && deck.HasVersions ? Visible : Collapsed;
		}

#endregion

#region Errors

		public ObservableCollection<Error> Errors => ErrorManager.Errors;

		public Visibility ErrorIconVisibility => ErrorManager.ErrorIconVisibility;

		public string ErrorCount => ErrorManager.Errors.Count > 1 ? $"({ErrorManager.Errors.Count})" : "";

		private void BtnErrors_OnClick(object sender, RoutedEventArgs e) => FlyoutErrors.IsOpen = !FlyoutErrors.IsOpen;

		public void ErrorsPropertyChanged()
		{
			OnPropertyChanged(nameof(Errors));
			OnPropertyChanged(nameof(ErrorIconVisibility));
			OnPropertyChanged(nameof(ErrorCount));
		}

#endregion

		private void HyperlinkUpdateNow_OnClick(object sender, RoutedEventArgs e) => Updater.StartUpdate();

		private async void MenuItemLastGamesReplay_OnClick(object sender, RoutedEventArgs e)
		{
			var game = (e.OriginalSource as MenuItem)?.DataContext as GameStats;
			if(game == null)
				return;
			await ReplayLauncher.ShowReplay(game, true);
		}

		private void MenuItemReplayClaimAccount_OnClick(object sender, RoutedEventArgs e)
		{
			Options.TreeViewItemTrackerReplays.IsSelected = true;
			FlyoutOptions.IsOpen = true;
		}

		private void MenuItemReplayMyAccount_OnClick(object sender, RoutedEventArgs e)
			=> Helper.TryOpenUrl("https://hsreplay.net/games/mine/?utm_source=hdt&utm_medium=client");

		private void MenuItemReplays_OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			OnPropertyChanged(nameof(MenuItemReplayClaimAccountVisibility));
			OnPropertyChanged(nameof(MenuItemReplayMyAccountVisibility));
		}

		private void MenuItemHsReplay_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://hsreplay.net/?utm_source=hdt&utm_medium=client");

		private void HyperlinkDevDiscord_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://discord.gg/CBnAFhX");

		private void BtnHsReplayDeckDetail_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault()?.GetSelectedDeckVersion() ?? DeckList.Instance.ActiveDeckVersion;
			if(deck?.ShortId != null)
				Helper.TryOpenUrl($"https://hsreplay.net/decks/{deck.ShortId}/?utm_source=hdt&utm_medium=client");
		}
	}
}
