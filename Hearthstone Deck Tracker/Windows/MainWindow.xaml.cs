#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using static System.Windows.Visibility;
using Application = System.Windows.Application;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
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
			MenuItemSetDeckUrl.Header = string.IsNullOrEmpty(deck.Url) ? "LINK TO UR_L" : "LINK TO NEW UR_L";
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(deck.Url) ? Collapsed : Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(deck.Url) ? Collapsed : Visible;
			MenuItemArchive.Visibility = DeckPickerList.SelectedDecks.Any(d => !d.Archived) ? Visible : Collapsed;
			MenuItemUnarchive.Visibility = DeckPickerList.SelectedDecks.Any(d => d.Archived) ? Visible : Collapsed;
			SeparatorDeck1.Visibility = deck.IsArenaDeck ? Collapsed : Visible;
			MenuItemOpenHearthStats.Visibility = deck.HasHearthStatsId ? Visible : Collapsed;
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
			DeckPickerList.UpdateAutoSelectToggleButton();
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

		private void MenuItemReplayLastGame_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var newest =
					Directory.GetFiles(Config.Instance.ReplayDir).Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).FirstOrDefault();
				if(newest != null)
					ReplayReader.LaunchReplayViewer(newest.FullName);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
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
					ReplayReader.LaunchReplayViewer(dialog.FileName);
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
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
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
			if(deck.Equals(DeckList.Instance.ActiveDeck))
				UseDeck(deck);
			Console.WriteLine(version);
		}

		private void MenuItemSaveAsNew_OnClick(object sender, RoutedEventArgs e) => SaveDeckWithOverwriteCheck(new SerializableVersion(1, 0), true);

		private void DeckPickerList_OnOnDoubleClick(DeckPicker sender, Deck deck)
		{
			if(deck == null)
				return;
			SetNewDeck(deck, true);
		}

		private void MenuItemLogin_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.ShowLoginDialog = true;
			Config.Save();
			Restart();
		}

		public void LoadHearthStatsMenu()
		{
			if(HearthStatsAPI.IsLoggedIn)
			{
				MenuItemLogout.Header = $"LOGOUT ({HearthStatsAPI.LoggedInAs})";
				MenuItemLogin.Visibility = Collapsed;
				MenuItemLogout.Visibility = Visible;
				SeparatorLogout.Visibility = Visible;
			}
			EnableHearthStatsMenu(HearthStatsAPI.IsLoggedIn);
		}

		public void EnableHearthStatsMenu(bool enable)
		{
			MenuItemCheckBoxAutoSyncBackground.IsEnabled = enable;
			MenuItemCheckBoxAutoUploadDecks.IsEnabled = enable;
			MenuItemCheckBoxAutoUploadGames.IsEnabled = enable;
			MenuItemCheckBoxSyncOnStart.IsEnabled = enable;
			MenuItemHearthStatsForceFullSync.IsEnabled = enable;
			MenuItemHearthStatsSync.IsEnabled = enable;
			MenuItemCheckBoxAutoDeleteDecks.IsEnabled = enable;
			MenuItemCheckBoxAutoDeleteGames.IsEnabled = enable;
			MenuItemDeleteHearthStatsDeck.IsEnabled = enable;
		}

		private void MenuItemHearthStatsSync_OnClick(object sender, RoutedEventArgs e) => HearthStatsManager.SyncAsync();

		private void SaveConfig(Action action)
		{
			if(!_initialized)
				return;
			action.Invoke();
			Config.Save();
		}

		private void MenuItemCheckBoxSyncOnStart_OnChecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsSyncOnStart = true);
		private void MenuItemCheckBoxSyncOnStart_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsSyncOnStart = false);
		private void MenuItemCheckBoxAutoUploadDecks_OnChecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoUploadNewDecks = true);
		private void MenuItemCheckBoxAutoUploadDecks_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoUploadNewDecks = false);
		private void MenuItemCheckBoxAutoUploadGames_OnChecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoUploadNewGames = true);
		private void MenuItemCheckBoxAutoUploadGames_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoUploadNewGames = false);
		private void MenuItemCheckBoxAutoSyncBackground_OnChecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoSyncInBackground = true);
		private void MenuItemCheckBoxAutoSyncBackground_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoSyncInBackground = false);

		private void BtnCloseNews_OnClick(object sender, RoutedEventArgs e) => NewsUpdater.ToggleNewsVisibility();

		private void BtnNewsPrevious_OnClick(object sender, RoutedEventArgs e) => NewsUpdater.PreviousNewsItem();

		private void BtnNewsNext_OnClick(object sender, RoutedEventArgs e) => NewsUpdater.NextNewsItem();


		private async void MenuItemHearthStatsForceFullSync_OnClick(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Full sync", "This may take a while, are you sure?", MessageDialogStyle.AffirmativeAndNegative,
				                      new MessageDialogs.Settings {AffirmativeButtonText = "start full sync", NegativeButtonText = "cancel"});
			if(result == MessageDialogResult.Affirmative)
				HearthStatsManager.SyncAsync(true);
		}

		private async void MenuItemLogout_OnClick(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Logout?", "Are you sure you want to logout?", MessageDialogStyle.AffirmativeAndNegative,
				                      new MessageDialogs.Settings {AffirmativeButtonText = "logout", NegativeButtonText = "cancel"});
			if(result != MessageDialogResult.Affirmative)
				return;
			if (!HearthStatsAPI.Logout())
			{
				await
					this.ShowMessageAsync("Error deleting stored credentials",
										  "You will be logged in automatically on the next start. To avoid this manually delete the \"hearthstats\" file at "
										  + Config.Instance.HearthStatsFilePath);
			}
			Restart();
		}

		private async void MenuItemDeleteHearthStatsDeck_OnClick(object sender, RoutedEventArgs e)
		{
			var decks = DeckPickerList.SelectedDecks;
			if(!decks.Any(d => d.HasHearthStatsId))
			{
				await this.ShowMessageAsync("None synced", "None of the selected decks have HearthStats ids.");
				return;
			}
			var dialogResult =
				await
				this.ShowMessageAsync("Delete " + decks.Count + " deck(s) on HearthStats?",
				                      "This will delete the deck(s) and all associated games ON HEARTHSTATS, as well as reset all stored IDs. The decks or games in the tracker (this) will NOT be deleted.\n\n Are you sure?",
				                      MessageDialogStyle.AffirmativeAndNegative,
				                      new MessageDialogs.Settings {AffirmativeButtonText = "delete", NegativeButtonText = "cancel"});

			if(dialogResult != MessageDialogResult.Affirmative)
				return;
			var controller = await this.ShowProgressAsync("Deleting decks...", "");
			var deleteSuccessful = await HearthStatsManager.DeleteDeckAsync(decks);
			await controller.CloseAsync();
			if(!deleteSuccessful)
			{
				await
					this.ShowMessageAsync("Problem deleting decks",
										  "There was a problem deleting the deck. All local IDs will be reset anyway, you can manually delete the deck online.");
			}
			foreach(var deck in decks)
			{
				deck.ResetHearthstatsIds();
				deck.DeckStats.HearthStatsDeckId = null;
				deck.DeckStats.Games.ForEach(g => g.ResetHearthstatsIds());
				deck.Versions.ForEach(v =>
				{
					v.DeckStats.HearthStatsDeckId = null;
					v.DeckStats.Games.ForEach(g => g.ResetHearthstatsIds());
					v.ResetHearthstatsIds();
				});
			}
			DeckList.Save();
		}

		public async Task<bool> CheckHearthStatsDeckDeletion()
		{
			if(Config.Instance.HearthStatsAutoDeleteDecks.HasValue)
				return Config.Instance.HearthStatsAutoDeleteDecks.Value;
			var dialogResult =
				await
				this.ShowMessageAsync("Delete deck on HearthStats?", "You can change this setting at any time in the HearthStats menu.",
				                      MessageDialogStyle.AffirmativeAndNegative,
				                      new MessageDialogs.Settings {AffirmativeButtonText = "yes (always)", NegativeButtonText = "no (never)"});
			Config.Instance.HearthStatsAutoDeleteDecks = dialogResult == MessageDialogResult.Affirmative;
			MenuItemCheckBoxAutoDeleteDecks.IsChecked = Config.Instance.HearthStatsAutoDeleteDecks;
			Config.Save();
			return Config.Instance.HearthStatsAutoDeleteDecks != null && Config.Instance.HearthStatsAutoDeleteDecks.Value;
		}

		private void MenuItemCheckBoxAutoDeleteDecks_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoDeleteDecks = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoDeleteDecks_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoDeleteDecks = false);

		private void MenuItemCheckBoxAutoDeleteGames_OnChecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoDeleteMatches = true);

		private void MenuItemCheckBoxAutoDeleteGames_OnUnchecked(object sender, RoutedEventArgs e) => SaveConfig(() => Config.Instance.HearthStatsAutoDeleteMatches = false);

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

		public void UpdateIntroLabelVisibility() => OnPropertyChanged(nameof(IntroductionLabelVisibility));

		public string LastSync
		{
			get
			{
				if(Config.Instance.LastHearthStatsGamesSync == 0)
					return "NEVER";
				var time = HearthStatsManager.TimeSinceLastSync;
				if(time.TotalDays > 7)
					return "> 1 WEEK AGO";
				if (time.TotalDays > 1)
					return (int)time.TotalDays + " DAYS AGO";
				if ((int)time.TotalDays > 0)
					return (int)time.TotalDays + " DAY AGO";
				if (time.TotalHours > 1)
					return (int)time.TotalHours + " HOURS AGO";
				if ((int)time.TotalHours > 0)
					return (int)time.TotalHours + " HOUR AGO";
				if ((int)time.TotalMinutes > 0)
					return (int)time.TotalMinutes + " MIN AGO";
				return "< 1 MIN AGO";
			}
		}

		private void MenuItemHearthStats_OnSubmenuOpened(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(LastSync));

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

		private bool _closeAnyway;

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
				if(HearthStatsManager.SyncInProgress && !_closeAnyway)
				{
					e.Cancel = true;
					var result =
						await
						this.ShowMessageAsync("WARNING! Sync with HearthStats in progress!",
						                      "Closing Hearthstone Deck Tracker now can cause data inconsistencies. Are you sure?",
						                      MessageDialogStyle.AffirmativeAndNegative,
						                      new MessageDialogs.Settings {AffirmativeButtonText = "close anyway", NegativeButtonText = "wait"});
					if(result == MessageDialogResult.Negative)
					{
						while(HearthStatsManager.SyncInProgress)
							await Task.Delay(100);
						await this.ShowMessage("Sync is complete.", "You can close Hearthstone Deck Tracker now.");
					}
					else
					{
						_closeAnyway = true;
						Close();
					}
				}
				Core.UpdateOverlay = false;
				Core.Update = false;

				//wait for update to finish, might otherwise crash when overlay gets disposed
				for(var i = 0; i < 100; i++)
				{
					if(Core.CanShutdown)
						break;
					await Task.Delay(50);
				}

				ReplayReader.CloseViewers();

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
				await LogReaderManager.Stop(true);
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

		private void BtnDonate_OnClick(object sender, RoutedEventArgs e)
		{
			BtnDonateContextMenu.Placement = PlacementMode.Bottom;
			BtnDonateContextMenu.PlacementTarget = BtnDonate;
			BtnDonateContextMenu.IsOpen = true;
		}

		private void BtnPaypal_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ");
		private void BtnPatreon_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl("https://www.patreon.com/HearthstoneDeckTracker");

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
			Close();
			Process.Start(Application.ResourceAssembly.Location);
			if(Application.Current != null)
				Application.Current.Shutdown();
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
			if(lastSelected == null)
				return;
			var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == lastSelected.Id);
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
	}
}