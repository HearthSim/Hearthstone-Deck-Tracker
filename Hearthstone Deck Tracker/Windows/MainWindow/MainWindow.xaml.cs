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
using System.Windows.Forms;
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
using MahApps.Metro.Controls.Dialogs;
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
			Core.Game.Reset();

			if(selected != null)
			{
				DeckList.Instance.ActiveDeck = selected;
                Core.Game.SetPremadeDeck((Deck)selected.Clone());
				UpdateMenuItemVisibility();
			}
			//needs to be true for automatic deck detection to work
			await LogReaderManager.Restart();
			Core.Overlay.Update(false);
			Core.Overlay.UpdatePlayerCards();
			Core.Windows.PlayerWindow.UpdatePlayerCards();
		}

		private void UpdateMenuItemVisibility()
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			MenuItemMoveDecktoArena.Visibility = deck.IsArenaDeck ? Visibility.Collapsed : Visibility.Visible;
			MenuItemMoveDeckToConstructed.Visibility = deck.IsArenaDeck ? Visibility.Visible : Visibility.Collapsed;
			MenuItemMissingCards.Visibility = deck.MissingCards.Any() ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUpdateDeck.Visibility = string.IsNullOrEmpty(deck.Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenUrl.Visibility = string.IsNullOrEmpty(deck.Url) ? Visibility.Collapsed : Visibility.Visible;
			MenuItemArchive.Visibility = DeckPickerList.SelectedDecks.Any(d => !d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			MenuItemUnarchive.Visibility = DeckPickerList.SelectedDecks.Any(d => d.Archived) ? Visibility.Visible : Visibility.Collapsed;
			SeparatorDeck1.Visibility = string.IsNullOrEmpty(deck.Url) && !deck.MissingCards.Any() ? Visibility.Collapsed : Visibility.Visible;
			MenuItemOpenHearthStats.Visibility = deck.HasHearthStatsId ? Visibility.Visible : Visibility.Collapsed;
		}

		public void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if(selected != null)
			{
				ListViewDeck.ItemsSource = selected.GetSelectedDeckVersion().Cards;
				Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private void UpdateDeckHistoryPanel(Deck selected, bool isNewDeck)
		{
			DeckHistoryPanel.Children.Clear();
			DeckCurrentVersion.Text = string.Format("v{0}.{1}", selected.SelectedVersion.Major, selected.SelectedVersion.Minor);
			if(isNewDeck)
			{
				MenuItemSaveVersionCurrent.IsEnabled = false;
				MenuItemSaveVersionMinor.IsEnabled = false;
				MenuItemSaveVersionMajor.IsEnabled = false;
				MenuItemSaveVersionCurrent.Visibility = Visibility.Collapsed;
				MenuItemSaveVersionMinor.Visibility = Visibility.Collapsed;
				MenuItemSaveVersionMajor.Visibility = Visibility.Collapsed;
			}
			else
			{
				MenuItemSaveVersionCurrent.IsEnabled = true;
				MenuItemSaveVersionMinor.IsEnabled = true;
				MenuItemSaveVersionMajor.IsEnabled = true;
				MenuItemSaveVersionCurrent.Visibility = Visibility.Visible;
				MenuItemSaveVersionMinor.Visibility = Visibility.Visible;
				MenuItemSaveVersionMajor.Visibility = Visibility.Visible;
				MenuItemSaveVersionCurrent.Header = _newDeck.Version.ToString("v{M}.{m} (current)");
				MenuItemSaveVersionMinor.Header = string.Format("v{0}.{1}", _newDeck.Version.Major, _newDeck.Version.Minor + 1);
				MenuItemSaveVersionMajor.Header = string.Format("v{0}.{1}", _newDeck.Version.Major + 1, 0);
			}

			if(selected.Versions.Count > 0)
			{
				var current = selected;
				foreach(var prevVersion in selected.Versions.OrderByDescending(d => d.Version))
				{
					var versionChange = new DeckVersionChange();
					versionChange.Label.Text = string.Format("{0} -> {1}", prevVersion.Version.ToString("v{M}.{m}"),
					                                         current.Version.ToString("v{M}.{m}"));
					versionChange.ListViewDeck.ItemsSource = current - prevVersion;
					DeckHistoryPanel.Children.Add(versionChange);
					current = prevVersion;
				}
			}
		}

		private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			AutoDeckDetection(true);
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			AutoDeckDetection(false);
		}

		public void AutoDeckDetection(bool enable)
		{
			CheckboxDeckDetection.IsChecked = enable;
			Config.Instance.AutoDeckDetection = enable;
			Config.Save();
			Core.TrayIcon.SetContextMenuProperty("autoSelectDeck", "Checked", enable);
			//MenuItem autoSelectMenuItem=(MenuItem)ContextMenu.Items[1];
		}

		private void CheckboxClassCardsFirst_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			SortClassCardsFirst(true);
		}

		private void CheckboxClassCardsFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			SortClassCardsFirst(false);
		}

	    public void SortClassCardsFirst(bool classFirst)
		{
			CheckboxClassCardsFirst.IsChecked = classFirst;
			Config.Instance.CardSortingClassFirst = classFirst;
			Config.Save();
			Helper.SortCardCollection(Core.MainWindow.ListViewDeck.ItemsSource, classFirst);
			Core.TrayIcon.SetContextMenuProperty("classCardsFirst", "Checked", classFirst);
		}


		private void MenuItemQuickFilter_Click(object sender, EventArgs e)
		{
			var tag = ((TextBlock)sender).Text;
			var actualTag = SortFilterDecksFlyout.Tags.FirstOrDefault(t => t.Name.ToUpperInvariant() == tag);
			if(actualTag != null)
			{
				var tags = new List<string> {actualTag.Name};
				SortFilterDecksFlyout.SetSelectedTags(tags);
				Config.Instance.SelectedTags = tags;
				Config.Save();
				DeckPickerList.UpdateDecks();
				Core.Windows.StatsWindow.StatsControl.LoadOverallStats();
				DeckStatsFlyout.LoadOverallStats();
			}
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
				Logger.WriteLine(ex.ToString(), "LastReplay");
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
				Logger.WriteLine(ex.ToString(), "ReplayFromFile");
			}
		}

		private void MenuItemReplaySelectGame_OnClick(object sender, RoutedEventArgs e)
		{
			if(Config.Instance.StatsInWindow)
			{
				Core.Windows.StatsWindow.WindowState = WindowState.Normal;
				Core.Windows.StatsWindow.Show();
				Core.Windows.StatsWindow.Activate();
				Core.Windows.StatsWindow.StatsControl.TabControlCurrentOverall.SelectedIndex = 1;
				Core.Windows.StatsWindow.StatsControl.TabControlOverall.SelectedIndex = 1;
			}
			else
			{
				FlyoutDeckStats.IsOpen = true;
				DeckStatsFlyout.TabControlCurrentOverall.SelectedIndex = 1;
				DeckStatsFlyout.TabControlOverall.SelectedIndex = 1;
			}
		}

		private void MenuItemSaveVersionCurrent_OnClick(object sender, RoutedEventArgs e)
		{
			SaveDeckWithOverwriteCheck(_newDeck.Version);
		}

		private void MenuItemSaveVersionMinor_OnClick(object sender, RoutedEventArgs e)
		{
			SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMinor(_newDeck.Version));
		}

		private void MenuItemSaveVersionMajor_OnClick(object sender, RoutedEventArgs e)
		{
			SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMajor(_newDeck.Version));
		}

		private void ComboBoxDeckVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized || DeckPickerList.ChangedSelection)
				return;
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			var version = ComboBoxDeckVersion.SelectedItem as SerializableVersion;
			if(version != null && deck.SelectedVersion != version)
			{
				deck.SelectVersion(version);
				DeckList.Save();
				DeckPickerList.UpdateDecks(forceUpdate: new[] {deck});
				UpdateDeckList(deck);
				ManaCurveMyDecks.UpdateValues();
				if(deck.Equals(DeckList.Instance.ActiveDeck))
					UseDeck(deck);
				Console.WriteLine(version);
			}
		}

		private void MenuItemSaveAsNew_OnClick(object sender, RoutedEventArgs e)
		{
			SaveDeckWithOverwriteCheck(new SerializableVersion(1, 0), true);
		}

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
				MenuItemLogout.Header = string.Format("LOGOUT ({0})", HearthStatsAPI.LoggedInAs);
				MenuItemLogin.Visibility = Visibility.Collapsed;
				MenuItemLogout.Visibility = Visibility.Visible;
				SeparatorLogout.Visibility = Visibility.Visible;
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

		private void MenuItemHearthStatsSync_OnClick(object sender, RoutedEventArgs e)
		{
			HearthStatsManager.SyncAsync();
		}

		private void MenuItemCheckBoxSyncOnStart_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsSyncOnStart = true;
			Config.Save();
		}

		private void MenuItemCheckBoxSyncOnStart_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsSyncOnStart = false;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoUploadDecks_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoUploadNewDecks = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoUploadDecks_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoUploadNewDecks = false;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoUploadGames_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoUploadNewGames = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoUploadGames_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoUploadNewGames = false;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoSyncBackground_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoSyncInBackground = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoSyncBackground_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoSyncInBackground = false;
			Config.Save();
		}

		private void BtnCloseNews_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.IgnoreNewsId = NewsUpdater.CurrentNewsId;
			Config.Save();
			StatusBarNews.Visibility = Visibility.Collapsed;
			MinHeight -= StatusBarNewsHeight;
			TopRow.Height = new GridLength(0);
		}
		
		private void BtnNewsPrevious_OnClick(object sender, RoutedEventArgs e)
        {
            NewsUpdater.PreviousNewsItem();
        }

        private void BtnNewsNext_OnClick(object sender, RoutedEventArgs e)
        {
            NewsUpdater.NextNewsItem();
        }


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
			if(result == MessageDialogResult.Affirmative)
			{
				var deletedFile = HearthStatsAPI.Logout();
				if(!deletedFile)
				{
					await
						this.ShowMessageAsync("Error deleting stored credentials",
						                      "You will be logged in automatically on the next start. To avoid this manually delete the \"hearthstats\" file at "
						                      + Config.Instance.HearthStatsFilePath);
				}
				Restart();
			}
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

			if(dialogResult == MessageDialogResult.Affirmative)
			{
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

		public async Task<bool> CheckHearthStatsMatchDeletion()
		{
			if(Config.Instance.HearthStatsAutoDeleteMatches.HasValue)
				return Config.Instance.HearthStatsAutoDeleteMatches.Value;
			var dialogResult =
				await
				this.ShowMessageAsync("Delete match(es) on HearthStats?", "You can change this setting at any time in the HearthStats menu.",
				                      MessageDialogStyle.AffirmativeAndNegative,
				                      new MessageDialogs.Settings {AffirmativeButtonText = "yes (always)", NegativeButtonText = "no (never)"});
			Config.Instance.HearthStatsAutoDeleteMatches = dialogResult == MessageDialogResult.Affirmative;
			MenuItemCheckBoxAutoDeleteGames.IsChecked = Config.Instance.HearthStatsAutoDeleteMatches;
			Config.Save();
			return Config.Instance.HearthStatsAutoDeleteMatches != null && Config.Instance.HearthStatsAutoDeleteMatches.Value;
		}

		private void MenuItemCheckBoxAutoDeleteDecks_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoDeleteDecks = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoDeleteDecks_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoDeleteDecks = false;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoDeleteGames_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoDeleteMatches = true;
			Config.Save();
		}

		private void MenuItemCheckBoxAutoDeleteGames_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HearthStatsAutoDeleteMatches = false;
			Config.Save();
		}

		private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			_movedLeft = null;
		}


		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Properties
		
		[Obsolete("Use API.Core.OverlayWindow")]   //for plugin compatibility
		public OverlayWindow Overlay { get { return Core.Overlay; } }

        private bool _initialized { get { return Core.Initialized; } }
		public bool EditingDeck;


		public bool IsShowingIncorrectDeckMessage;
		public bool NeedToIncorrectDeckMessage;
		private Deck _newDeck;
		private bool _newDeckUnsavedChanges;
		private Deck _originalDeck;

        private double _heightChangeDueToSearchBox;
		public const int SearchBoxHeight = 30;
		public int StatusBarNewsHeight { get { return 20; } }

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		#endregion

		#region Constructor

	    public MainWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(new TextBoxTraceListener(Options.OptionsTrackerLogging.TextBoxLog));
            EnableMenuItems(false);
            TagControlEdit.OperationSwitch.Visibility = Visibility.Collapsed;
            TagControlEdit.GroupBoxSortingAllConstructed.Visibility = Visibility.Collapsed;
            TagControlEdit.GroupBoxSortingArena.Visibility = Visibility.Collapsed;
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

		public Thickness TitleBarMargin
		{
			get { return new Thickness(0, TitlebarHeight, 0, 0); }
		}

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
		        if (HearthStatsManager.SyncInProgress && !_closeAnyway)
		        {
		            e.Cancel = true;
		            var result =
		                await
		                    this.ShowMessageAsync("WARNING! Sync with HearthStats in progress!",
		                        "Closing Hearthstone Deck Tracker now can cause data inconsistencies. Are you sure?",
		                        MessageDialogStyle.AffirmativeAndNegative,
		                        new MessageDialogs.Settings
		                        {
		                            AffirmativeButtonText = "close anyway",
		                            NegativeButtonText = "wait"
		                        });
		            if (result == MessageDialogResult.Negative)
		            {
		                while (HearthStatsManager.SyncInProgress)
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
		        for (var i = 0; i < 100; i++)
		        {
		            if (Core.CanShutdown)
		                break;
		            await Task.Delay(50);
		        }

		        ReplayReader.CloseViewers();

		        Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
		        //Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;
		        Config.Instance.SelectedDeckPickerClasses = DeckPickerList.SelectedClasses.ToArray();

		        Config.Instance.WindowWidth =
		            (int) (Width - (GridNewDeck.Visibility == Visibility.Visible ? GridNewDeck.ActualWidth : 0));
		        Config.Instance.WindowHeight = (int) (Height - _heightChangeDueToSearchBox);
		        Config.Instance.TrackerWindowTop = (int) Top;
		        Config.Instance.TrackerWindowLeft = (int) (Left + (_movedLeft.HasValue ? _movedLeft.Value : 0));

		        //position of add. windows is NaN if they were never opened.
		        if (!double.IsNaN(Core.Windows.PlayerWindow.Left))
		            Config.Instance.PlayerWindowLeft = (int) Core.Windows.PlayerWindow.Left;
		        if (!double.IsNaN(Core.Windows.PlayerWindow.Top))
		            Config.Instance.PlayerWindowTop = (int) Core.Windows.PlayerWindow.Top;
		        Config.Instance.PlayerWindowHeight = (int) Core.Windows.PlayerWindow.Height;

		        if (!double.IsNaN(Core.Windows.OpponentWindow.Left))
		            Config.Instance.OpponentWindowLeft = (int) Core.Windows.OpponentWindow.Left;
		        if (!double.IsNaN(Core.Windows.OpponentWindow.Top))
		            Config.Instance.OpponentWindowTop = (int) Core.Windows.OpponentWindow.Top;
		        Config.Instance.OpponentWindowHeight = (int) Core.Windows.OpponentWindow.Height;

		        if (!double.IsNaN(Core.Windows.TimerWindow.Left))
		            Config.Instance.TimerWindowLeft = (int) Core.Windows.TimerWindow.Left;
		        if (!double.IsNaN(Core.Windows.TimerWindow.Top))
		            Config.Instance.TimerWindowTop = (int) Core.Windows.TimerWindow.Top;
		        Config.Instance.TimerWindowHeight = (int) Core.Windows.TimerWindow.Height;
		        Config.Instance.TimerWindowWidth = (int) Core.Windows.TimerWindow.Width;

		        if (!double.IsNaN(Core.Windows.StatsWindow.Left))
		            Config.Instance.StatsWindowLeft = (int) Core.Windows.StatsWindow.Left;
		        if (!double.IsNaN(Core.Windows.StatsWindow.Top))
		            Config.Instance.StatsWindowTop = (int) Core.Windows.StatsWindow.Top;
		        Config.Instance.StatsWindowHeight = (int) Core.Windows.StatsWindow.Height;
		        Config.Instance.StatsWindowWidth = (int) Core.Windows.StatsWindow.Width;

                Core.TrayIcon.NotifyIcon.Visible = false;
		        Core.Overlay.Close();
		        await LogReaderManager.Stop();
		        Core.Windows.TimerWindow.Shutdown();
		        Core.Windows.PlayerWindow.Shutdown();
		        Core.Windows.OpponentWindow.Shutdown();
		        Core.Windows.StatsWindow.Shutdown();
		        Config.Save();
		        DeckList.Save();
		        DeckStatsList.Save();
		        PluginManager.SavePluginsSettings();
		        PluginManager.Instance.UnloadPlugins();
		    }
		    catch (Exception)
		    {
		        //doesnt matter
		    }
		    finally
		    {
		        Application.Current.Shutdown();
		    }
		}

		private void BtnSortFilter_Click(object sender, RoutedEventArgs e)
		{
			FlyoutSortFilter.IsOpen = !FlyoutSortFilter.IsOpen;
		}


		private void BtnOptions_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutOptions.IsOpen = true;
		}

		private void BtnHelp_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutHelp.IsOpen = true;
		}

		private void BtnDonate_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ");
		}

		#endregion

		#region GENERAL METHODS

		public async void ShowIncorrectDeckMessage()
		{
			if(Core.Game.Player.DrawnCards.Count == 0)
			{
				IsShowingIncorrectDeckMessage = false;
				return;
			}

			//wait for player hero to be detected and at least 3 cards to be drawn
			for(var i = 0; i < 50; i++)
			{
				if(Core.Game.Player.Class != null && Core.Game.Player.DrawnCards.Count >= 3)
					break;
				await Task.Delay(100);
			}
			if(string.IsNullOrEmpty(Core.Game.Player.Class) || Core.Game.Player.DrawnCards.Count < 3)
			{
				IsShowingIncorrectDeckMessage = false;
				Logger.WriteLine("No player hero detected or less then 3 cards drawn. Not showing dialog.", "IncorrectDeckMessage");
				return;
			}
			await Task.Delay(1000);

			var decks =
				DeckList.Instance.Decks.Where(
				                              d =>
				                              d.Class == Core.Game.Player.Class && !d.Archived && d.IsArenaRunCompleted != true
				                              && Core.Game.Player.DrawnCardIdsTotal.Distinct().All(id => d.GetSelectedDeckVersion().Cards.Any(c => id == c.Id))
											  && Core.Game.Player.DrawnCards.All(c => d.GetSelectedDeckVersion().Cards.Any(c2 => c2.Id == c.Id && c2.Count >= c.Count)))
				        .ToList();

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
            Core.Game.NoMatchingDeck = decks.Count == 0;
			
			if(decks.Count == 1 && Config.Instance.AutoSelectDetectedDeck)
			{
				var deck = decks.First();
				Logger.WriteLine("Automatically selected deck: " + deck.Name, "IncorrectDeckMessage");
				DeckPickerList.SelectDeck(deck);
				UpdateDeckList(deck);
				UseDeck(deck);
			}
			else
			{
				decks.Add(new Deck("Use no deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, false, new List<Card>(),
				                   SerializableVersion.Default, new List<Deck>(), false, "", Guid.Empty, ""));
				if(decks.Count == 1 && DeckList.Instance.ActiveDeckVersion != null)
					decks.Add(new Deck("No match - Keep using active deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, false, new List<Card>(),
								   SerializableVersion.Default, new List<Deck>(), false, "", Guid.Empty, ""));
				var dsDialog = new DeckSelectionDialog(decks);
				dsDialog.ShowDialog();

				var selectedDeck = dsDialog.SelectedDeck;

				if(selectedDeck != null)
				{
					if(selectedDeck.Name == "Use no deck")
						SelectDeck(null, true);
					else if(selectedDeck.Name == "No match - Keep using active deck")
					{
						Core.Game.IgnoreIncorrectDeck = DeckList.Instance.ActiveDeck;
						Logger.WriteLine(string.Format("Now ignoring {0} as an incorrect deck", DeckList.Instance.ActiveDeck.Name), "IncorrectDeckMessage");
					}
					else
					{
						Logger.WriteLine("Selected deck: " + selectedDeck.Name, "IncorrectDeckMessage");
						DeckPickerList.SelectDeck(selectedDeck);
						UpdateDeckList(selectedDeck);
						UseDeck(selectedDeck);
					}
				}
				else
				{
					this.ShowMessage("Deck detection disabled.", "Can be re-enabled in \"DECKS\" menu.");
					CheckboxDeckDetection.IsChecked = false;
					Config.Save();
				}
			}

			IsShowingIncorrectDeckMessage = false;
			NeedToIncorrectDeckMessage = false;
		}

		private void MinimizeToTray()
		{
            Core.TrayIcon.NotifyIcon.Visible = true;
			Hide();
			Visibility = Visibility.Collapsed;
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
				Logger.WriteLine(ex.ToString(), "ActivatingWindow");
			}
		}

		#endregion

		#region MY DECKS - GUI

		private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
		{
			SelectDeck(null, true);
		}

		private void BtnDeckStats_Click(object sender, RoutedEventArgs e)
		{
			ShowStats();
		}

		public void ShowStats()
		{

			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(Config.Instance.StatsInWindow)
			{
				Core.Windows.StatsWindow.StatsControl.SetDeck(deck);
				Core.Windows.StatsWindow.WindowState = WindowState.Normal;
				Core.Windows.StatsWindow.Show();
				Core.Windows.StatsWindow.Activate();
			}
			else
			{
				DeckStatsFlyout.SetDeck(deck);
				FlyoutDeckStats.IsOpen = true;
			}
		}

		private void DeckPickerList_OnSelectedDeckChanged(DeckPicker sender, Deck deck)
		{
			SelectDeck(deck, Config.Instance.AutoUseDeck);
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
					//Overlay.ListViewPlayer.ItemsSource = Core.Game.PlayerDeck;
					//PlayerWindow.ListViewPlayer.ItemsSource = Core.Game.PlayerDeck;
					//Logger.WriteLine("Set player itemsource as PlayerDeck", "Tracker");
					while(DeckList.Instance.LastDeckClass.Any(ldc => ldc.Class == deck.Class))
					{
						var lastSelected = DeckList.Instance.LastDeckClass.FirstOrDefault(ldc => ldc.Class == deck.Class);
						if(lastSelected != null)
							DeckList.Instance.LastDeckClass.Remove(lastSelected);
						else
							break;
					}
					DeckList.Instance.LastDeckClass.Add(new DeckInfo {Class = deck.Class, Name = deck.Name, Id = deck.DeckId});
					DeckList.Save();

					Logger.WriteLine("Switched to deck: " + deck.Name, "Tracker");

					int useNoDeckMenuItem = Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("useNoDeck");
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
				{
					DeckPickerList.DeselectDeck();
					//Overlay.ListViewPlayer.ItemsSource = Core.Game.PlayerDrawn;
					//PlayerWindow.ListViewPlayer.ItemsSource = Core.Game.PlayerDrawn;
					//Logger.WriteLine("set player item source to PlayerDrawn", "Tracker");
				}

				int useNoDeckMenuItem = Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("useNoDeck");
                Core.TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Checked = true;
			}

			//set up stats
			var statsTitle = string.Format("Stats{0}", deck == null ? "" : ": " + deck.Name);
			Core.Windows.StatsWindow.Title = statsTitle;
			FlyoutDeckStats.Header = statsTitle;
			Core.Windows.StatsWindow.StatsControl.SetDeck(deck);
			DeckStatsFlyout.SetDeck(deck);

			if(setActive)
				UseDeck(deck);
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
			if(lastSelected != null)
			{
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == lastSelected.Id);
				if(deck != null)
				{
					DeckPickerList.SelectDeck(deck);
					SelectDeck(deck, true);
				}
			}
		}

		private void UpdatePanelVersionComboBox(Deck deck)
		{
			ComboBoxDeckVersion.ItemsSource = deck != null ? deck.VersionsIncludingSelf : null;
			ComboBoxDeckVersion.SelectedItem = deck != null ? deck.SelectedVersion : null;
			PanelVersionComboBox.Visibility = deck != null && deck.HasVersions ? Visibility.Visible : Visibility.Collapsed;
		}

		#endregion

		#region Errors

		public ObservableCollection<Error> Errors
		{
			get { return ErrorManager.Errors; }
		}

		public Visibility ErrorIconVisibility
		{
			get { return ErrorManager.ErrorIconVisibility; }
		}

		public string ErrorCount
		{
			get { return ErrorManager.Errors.Count > 1 ? string.Format("({0})", ErrorManager.Errors.Count) : ""; }
		}

		private void BtnErrors_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutErrors.IsOpen = !FlyoutErrors.IsOpen;
		}

		public void ErrorsPropertyChanged()
		{
			OnPropertyChanged("Errors");
			OnPropertyChanged("ErrorIconVisibility");
			OnPropertyChanged("ErrorCount");
		}

		#endregion

		private void BtnStartHearthstone_Click(object sender, RoutedEventArgs e)
		{
			Helper.StartHearthstoneAsync();
		}

	}
}