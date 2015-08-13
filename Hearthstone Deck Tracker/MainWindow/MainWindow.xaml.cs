#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Region = Hearthstone_Deck_Tracker.Enums.Region;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void UseDeck(Deck selected)
		{
			Game.Reset();

			if(selected != null)
			{
				DeckList.Instance.ActiveDeck = selected;
				Game.SetPremadeDeck((Deck)selected.Clone());
				UpdateMenuItemVisibility();
			}
			//needs to be true for automatic deck detection to work
			HsLogReader.Instance.Reset(true);
			Overlay.Update(false);
			Overlay.SortViews();
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

		private void AutoDeckDetection(bool enable)
		{
			CheckboxDeckDetection.IsChecked = enable;
			Config.Instance.AutoDeckDetection = enable;
			Config.Save();
			SetContextMenuProperty("autoSelectDeck", "Checked", enable);
			//MenuItem autoSelectMenuItem=(MenuItem)ContextMenu.Items[1];
		}

		private void AutoDeckDetectionContextMenu()
		{
			bool enable = (bool)GetContextMenuProperty("autoSelectDeck", "Checked");
			AutoDeckDetection(!enable);
		}

		private void UseNoDeckContextMenu()
		{
			bool enable = (bool)GetContextMenuProperty("useNoDeck", "Checked");
			if(enable)
				SelectLastUsedDeck();
			else
				SelectDeck(null, true);
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

		private void SortClassCardsFirst(bool classFirst)
		{
			CheckboxClassCardsFirst.IsChecked = classFirst;
			Config.Instance.CardSortingClassFirst = classFirst;
			Config.Save();
			Helper.SortCardCollection(Helper.MainWindow.ListViewDeck.ItemsSource, classFirst);
			SetContextMenuProperty("classCardsFirst", "Checked", classFirst);
		}

		private void SortClassCardsFirstContextMenu()
		{
			bool enable = (bool)GetContextMenuProperty("classCardsFirst", "Checked");
			SortClassCardsFirst(!enable);
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
				StatsWindow.StatsControl.LoadOverallStats();
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
				StatsWindow.WindowState = WindowState.Normal;
				StatsWindow.Show();
				StatsWindow.Activate();
				StatsWindow.StatsControl.TabControlCurrentOverall.SelectedIndex = 1;
				StatsWindow.StatsControl.TabControlOverall.SelectedIndex = 1;
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

		private void LoadHearthStats()
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
			Config.Instance.IgnoreNewsId = _currentNewsId;
			Config.Save();
			StatusBarNews.Visibility = Visibility.Collapsed;
			MinHeight -= StatusBarNewsHeight;
			TopRow.Height = new GridLength(0);
		}

		private async void MenuItemHearthStatsForceFullSync_OnClick(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Full sync", "This may take a while, are you sure?", MessageDialogStyle.AffirmativeAndNegative,
				                      new MetroDialogSettings {AffirmativeButtonText = "start full sync", NegativeButtonText = "cancel"});
			if(result == MessageDialogResult.Affirmative)
				HearthStatsManager.SyncAsync(true);
		}

		private async void MenuItemLogout_OnClick(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Logout?", "Are you sure you want to logout?", MessageDialogStyle.AffirmativeAndNegative,
				                      new MetroDialogSettings {AffirmativeButtonText = "logout", NegativeButtonText = "cancel"});
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
				                      new MetroDialogSettings {AffirmativeButtonText = "delete", NegativeButtonText = "cancel"});

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
				                      new MetroDialogSettings {AffirmativeButtonText = "yes (always)", NegativeButtonText = "no (never)"});
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
				                      new MetroDialogSettings {AffirmativeButtonText = "yes (always)", NegativeButtonText = "no (never)"});
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

		private int IndexOfKeyContextMenuItem(string key)
		{
			return _notifyIcon.ContextMenu.MenuItems.IndexOfKey(key);
		}

		private void SetContextMenuProperty(string key, string property, object value)
		{
			int menuItemInd = IndexOfKeyContextMenuItem(key);
			object target = _notifyIcon.ContextMenu.MenuItems[menuItemInd];
			target.GetType().GetProperty(property).SetValue(target, value);
		}

		private object GetContextMenuProperty(string key, string property)
		{
			int menuItemInd = IndexOfKeyContextMenuItem(key);
			object target = _notifyIcon.ContextMenu.MenuItems[menuItemInd];
			return target.GetType().GetProperty(property).GetValue(target, null);
		}

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Properties

		private const int NewsCheckInterval = 300;
		private const int NewsTickerUpdateInterval = 30;
		private const int HearthStatsAutoSyncInterval = 300;
		//public readonly DeckList DeckList;
		public readonly List<Deck> DefaultDecks;
		public readonly OpponentWindow OpponentWindow;
		public readonly OverlayWindow Overlay;
		public readonly PlayerWindow PlayerWindow;
		public readonly StatsWindow StatsWindow;
		public readonly TimerWindow TimerWindow;
		private readonly bool _foundHsDirectory;
		private readonly bool _initialized;

		private readonly string _logConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		                                         + @"\Blizzard\Hearthstone\log.config";

		private readonly NotifyIcon _notifyIcon;
		private readonly bool _updatedLogConfig;

		public bool EditingDeck;

		public ReadOnlyCollection<string> EventKeys =
			new ReadOnlyCollection<string>(new[] {"None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"});

		public bool IsShowingIncorrectDeckMessage;
		public bool NeedToIncorrectDeckMessage;
		private bool _canShowDown;
		private int _currentNewsId;
		private string _currentNewsLine;
		private bool _doUpdate;
		private DateTime _lastHearthStatsSync;
		private DateTime _lastNewsCheck;
		private DateTime _lastNewsUpdate;
		private DateTime _lastUpdateCheck;
		private Deck _newDeck;
		private bool _newDeckUnsavedChanges;
		private string[] _news;
		private int _newsLine;
		private Deck _originalDeck;
		private bool _tempUpdateCheckDisabled;
		private bool _update;
		private Version _updatedVersion;

		private double _heightChangeDueToSearchBox;
		private const int SearchBoxHeight = 30;
		private const int StatusBarNewsHeight = 20;

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		#endregion

		#region Constructor

		public MainWindow()
		{
			// Set working directory to path of executable
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			InitializeComponent();
			Trace.Listeners.Add(new TextBoxTraceListener(Options.OptionsTrackerLogging.TextBoxLog));


			Helper.MainWindow = this;
			//Config.Load();
			EnableMenuItems(false);


			try
			{
				if(File.Exists("HDTUpdate_new.exe"))
				{
					if(File.Exists("HDTUpdate.exe"))
						File.Delete("HDTUpdate.exe");
					File.Move("HDTUpdate_new.exe", "HDTUpdate.exe");
				}
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error updating updater\n" + e);
			}
			try
			{
				//updater used pre v0.9.6
				if(File.Exists("Updater.exe"))
					File.Delete("Updater.exe");
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error deleting Updater.exe\n" + e);
			}


			HsLogReader.Create();

			var configVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion) ? null : new Version(Config.Instance.CreatedByVersion);

			var currentVersion = Helper.GetCurrentVersion();
			var versionString = string.Empty;
			if(currentVersion != null)
			{
				versionString = string.Format("{0}.{1}.{2}", currentVersion.Major, currentVersion.Minor, currentVersion.Build);
				Help.TxtblockVersion.Text = "Version: " + versionString;

				// Assign current version to the config instance so that it will be saved when the config
				// is rewritten to disk, thereby telling us what version of the application created it
				Config.Instance.CreatedByVersion = currentVersion.ToString();
			}

			ConvertLegacyConfig(currentVersion, configVersion);

			if(Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

			_foundHsDirectory = FindHearthstoneDir();

			if(_foundHsDirectory)
				_updatedLogConfig = UpdateLogConfigFile();

			//hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
			Game.Reset();

			if(!Directory.Exists(Config.Instance.DataDir))
				Config.Instance.Reset("DataDirPath");

			SetupDeckListFile();
			DeckList.Load();

			// Don't load active deck if it's archived
			if(DeckList.Instance.ActiveDeck != null && DeckList.Instance.ActiveDeck.Archived)
				DeckList.Instance.ActiveDeck = null;

			UpdateDeckList(DeckList.Instance.ActiveDeck);

			SetupDefaultDeckStatsFile();
			DefaultDeckStats.Load();


			SetupDeckStatsFile();
			DeckStatsList.Load();

			_notifyIcon = new NotifyIcon
			{
				Icon = new Icon(@"Images/HearthstoneDeckTracker16.ico"),
				Visible = true,
				ContextMenu = new ContextMenu(),
				Text = "Hearthstone Deck Tracker v" + versionString
			};

			MenuItem useNoDeckMenuItem = new MenuItem("Use no deck", (sender, args) => UseNoDeckContextMenu());
			useNoDeckMenuItem.Name = "useNoDeck";
			_notifyIcon.ContextMenu.MenuItems.Add(useNoDeckMenuItem);

			MenuItem autoSelectDeckMenuItem = new MenuItem("Autoselect deck", (sender, args) => AutoDeckDetectionContextMenu());
			autoSelectDeckMenuItem.Name = "autoSelectDeck";
			_notifyIcon.ContextMenu.MenuItems.Add(autoSelectDeckMenuItem);

			MenuItem classCardsFirstMenuItem = new MenuItem("Class cards first", (sender, args) => SortClassCardsFirstContextMenu());
			classCardsFirstMenuItem.Name = "classCardsFirst";
			_notifyIcon.ContextMenu.MenuItems.Add(classCardsFirstMenuItem);

			_notifyIcon.ContextMenu.MenuItems.Add("Show", (sender, args) => ActivateWindow());
			_notifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, args) => Close());
			_notifyIcon.MouseClick += (sender, args) =>
			{
				if(args.Button == MouseButtons.Left)
					ActivateWindow();
			};

			//create overlay
			Overlay = new OverlayWindow {Topmost = true};

			PlayerWindow = new PlayerWindow(Config.Instance, Game.IsUsingPremade ? Game.PlayerDeck : Game.PlayerDrawn);
			OpponentWindow = new OpponentWindow(Config.Instance, Game.OpponentCards);
			TimerWindow = new TimerWindow(Config.Instance);
			StatsWindow = new StatsWindow();

			if(Config.Instance.PlayerWindowOnStart)
				PlayerWindow.Show();
			if(Config.Instance.OpponentWindowOnStart)
				OpponentWindow.Show();
			if(Config.Instance.TimerWindowOnStartup)
				TimerWindow.Show();

			LoadConfig();
			if(!Config.Instance.NetDeckClipboardCheck.HasValue)
			{
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				                        @"Google\Chrome\User Data\Default\Extensions\lpdbiakcpmcppnpchohihcbdnojlgeel");

				if(Directory.Exists(path))
				{
					Config.Instance.NetDeckClipboardCheck = true;
					Config.Save();
				}
			}

			if(!Config.Instance.RemovedNoteUrls)
				RemoveNoteUrls();
			if(!Config.Instance.ResolvedDeckStatsIssue)
				ResolveDeckStatsIssue();

			TurnTimer.Create(90);

			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			TagControlEdit.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlEdit.GroupBoxSortingAllConstructed.Visibility = Visibility.Collapsed;
			TagControlEdit.GroupBoxSortingArena.Visibility = Visibility.Collapsed;

			FlyoutNotes.ClosingFinished += (sender, args) => DeckNotesEditor.SaveDeck();


			UpdateDbListView();

			_doUpdate = _foundHsDirectory;

			SelectDeck(DeckList.Instance.ActiveDeck, true);

			if(_foundHsDirectory)
				HsLogReader.Instance.Start();

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			DeckPickerList.PropertyChanged += DeckPickerList_PropertyChanged;
			DeckPickerList.UpdateDecks();
			DeckPickerList.UpdateArchivedClassVisibility();

			CopyReplayFiles();

			LoadHearthStats();

			UpdateOverlayAsync();
			UpdateAsync();

			BackupManager.Run();

			_initialized = true;

			PluginManager.Instance.LoadPlugins();
			Options.OptionsTrackerPlugins.Load();
			PluginManager.Instance.StartUpdateAsync();
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

		private void ResolveDeckStatsIssue()
		{
			foreach(var deck in DeckList.Instance.Decks)
			{
				foreach(var deckVersion in deck.Versions)
				{
					if(deckVersion.DeckStats.Games.Any())
					{
						var games = deckVersion.DeckStats.Games.ToList();
						foreach(var game in games)
						{
							deck.DeckStats.AddGameResult(game);
							deckVersion.DeckStats.Games.Remove(game);
						}
					}
				}
			}
			foreach(var deckStats in DeckStatsList.Instance.DeckStats)
			{
				if(deckStats.Games.Any() && !DeckList.Instance.Decks.Any(d => deckStats.BelongsToDeck(d)))
				{
					var games = deckStats.Games.ToList();
					foreach(var game in games)
					{
						var defaultStats = DefaultDeckStats.Instance.GetDeckStats(game.PlayerHero);
						if(defaultStats != null)
						{
							defaultStats.AddGameResult(game);
							deckStats.Games.Remove(game);
						}
					}
				}
			}

			DeckStatsList.Save();
			Config.Instance.ResolvedDeckStatsIssue = true;
			Config.Save();
		}

		/// <summary>
		/// v0.10.0 caused opponent names to be saved as the hero, rather than the name.
		/// </summary>
		private async void ResolveOpponentNames()
		{
			var games =
				DeckStatsList.Instance.DeckStats.SelectMany(ds => ds.Games)
				             .Where(g => g.HasReplayFile && Enum.GetNames(typeof(HeroClass)).Any(x => x == g.OpponentName))
				             .ToList();
			if(!games.Any())
			{
				Config.Instance.ResolvedOpponentNames = true;
				Config.Save();
				return;
			}
			var controller =
				await
				this.ShowProgressAsync("Fixing opponent names in recorded games...",
				                       "v0.10.0 caused opponent names to be set to their hero, rather than the actual name.\n\nThis may take a moment.\n\nYou can cancel to continue this at a later time (or not at all).",
				                       true);
			var count = 0;
			var lockMe = new object();
			await Task.Run(() =>
			{
				Parallel.ForEach(games, (game, loopState) =>
				{
					if(controller.IsCanceled)
						loopState.Stop();
					List<ReplayKeyPoint> replay = ReplayReader.LoadReplay(game.ReplayFile);
					if(replay == null)
						return;
					var last = replay.LastOrDefault();
					if(last == null)
						return;
					var opponent = last.Data.FirstOrDefault(x => x.IsOpponent);
					if(opponent == null)
						return;
					game.OpponentName = opponent.Name;
					lock(lockMe)
					{
						controller.SetProgress(1.0 * ++count / games.Count);
					}
				});
			});

			await controller.CloseAsync();
			if(controller.IsCanceled)
			{
				var fix =
					await
					this.ShowMessageAsync("Cancelled", "Fix remaining names on next start?", MessageDialogStyle.AffirmativeAndNegative,
					                      new MetroDialogSettings {AffirmativeButtonText = "next time", NegativeButtonText = "don\'t fix"});
				if(fix == MessageDialogResult.Negative)
				{
					Config.Instance.ResolvedOpponentNames = true;
					Config.Save();
				}
			}
			else
			{
				Config.Instance.ResolvedOpponentNames = true;
				Config.Save();
			}
			DeckStatsList.Save();
		}

		private async void UpdateAsync()
		{
			const string url = "https://raw.githubusercontent.com/Epix37/HDT-Data/master/news";
			_update = true;
			_lastNewsCheck = DateTime.MinValue;
			_lastNewsUpdate = DateTime.MinValue;
			_currentNewsId = Config.Instance.IgnoreNewsId;
			_lastHearthStatsSync = DateTime.Now;
			while(_update)
			{
				if((DateTime.Now - _lastNewsCheck) > TimeSpan.FromSeconds(NewsCheckInterval))
				{
					try
					{
						var oldNewsId = _currentNewsId;
						using(var client = new WebClient())
						{
							var raw = await client.DownloadStringTaskAsync(url);
							var content = raw.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
							try
							{
								_currentNewsId = int.Parse(content[0].Split(':')[1].Trim());
							}
							catch(Exception)
							{
								_currentNewsId = 0;
							}
							_news = content.Skip(1).ToArray();
						}
						if(_currentNewsId > oldNewsId
						   || StatusBarNews.Visibility == Visibility.Collapsed && _currentNewsId > Config.Instance.IgnoreNewsId)
						{
							TopRow.Height = new GridLength(20);
							StatusBarNews.Visibility = Visibility.Visible;
							MinHeight += StatusBarNewsHeight;
							UpdateNews(0);
						}
					}
					catch(Exception e)
					{
						Logger.WriteLine("Error loading news: " + e, "UpdateNews");
					}
					_lastNewsCheck = DateTime.Now;
				}
				if((DateTime.Now - _lastNewsUpdate) > TimeSpan.FromSeconds(NewsTickerUpdateInterval))
					UpdateNews();

				if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoSyncInBackground
				   && (DateTime.Now - _lastHearthStatsSync) > TimeSpan.FromSeconds(HearthStatsAutoSyncInterval))
				{
					_lastHearthStatsSync = DateTime.Now;
					HearthStatsManager.SyncAsync(background: true);
				}
				await Task.Delay(1000);
			}
		}

		private void UpdateNews(int newsLine)
		{
			if(newsLine < _news.Length && _currentNewsLine != _news[newsLine])
			{
				_currentNewsLine = _news[newsLine];
				NewsContentControl.Content = StringToTextBlock(_currentNewsLine);
			}
			_lastNewsUpdate = DateTime.Now;
		}

		private void UpdateNews()
		{
			if(_news == null || _news.Length == 0)
				return;
			_newsLine++;
			if(_newsLine > _news.Length - 1)
				_newsLine = 0;
			UpdateNews(_newsLine);
		}

		private TextBlock StringToTextBlock(string text)
		{
			var tb = new TextBlock();
			ParseMarkup(text, tb);
			return tb;
		}

		private void ParseMarkup(string text, TextBlock tb)
		{
			const string urlMarkup = @"\[(?<text>(.*?))\]\((?<url>(http[s]?://.+\..+?))\)";

			var url = Regex.Match(text, urlMarkup);
			var rest = url.Success ? text.Split(new[] {(url.Value)}, StringSplitOptions.None) : new[] {text};
			if(rest.Length == 1)
				tb.Inlines.Add(rest[0]);
			else
			{
				for(int restIndex = 0, urlIndex = 0; restIndex < rest.Length; restIndex += 2, urlIndex++)
				{
					ParseMarkup(rest[restIndex], tb);
					var link = new Hyperlink();
					link.NavigateUri = new Uri(url.Groups["url"].Value);
					link.RequestNavigate += (sender, args) => Process.Start(args.Uri.AbsoluteUri);
					link.Inlines.Add(new Run(url.Groups["text"].Value));
					link.Foreground = new SolidColorBrush(Colors.White);
					tb.Inlines.Add(link);
					ParseMarkup(rest[restIndex + 1], tb);
				}
			}
		}

		private bool ResolveDeckStatsIds()
		{
			var needToRestart = false;
			foreach(var deckStats in DeckStatsList.Instance.DeckStats)
			{
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.Name == deckStats.Name);
				if(deck != null)
				{
					deckStats.DeckId = deck.DeckId;
					deckStats.HearthStatsDeckId = deck.HearthStatsId;
					needToRestart = true;
				}
			}
			DeckStatsList.Save();
			DeckList.Save();
			Config.Instance.ResolvedDeckStatsIds = true;
			Config.Save();
			return needToRestart;
		}

		private void RemoveNoteUrls()
		{
			foreach(var deck in DeckList.Instance.Decks)
			{
				if(!string.IsNullOrEmpty(deck.Url))
					deck.Note = deck.Note.Replace(deck.Url, "").Trim();
			}
			DeckList.Save();
			Config.Instance.RemovedNoteUrls = true;
			Config.Save();
		}

		#endregion

		#region GENERAL GUI

		private bool _closeAnyway;
		private bool _showingUpdateMessage;

		private void MetroWindow_StateChanged(object sender, EventArgs e)
		{
			if(Config.Instance.MinimizeToTray && WindowState == WindowState.Minimized)
				MinimizeToTray();
		}

		private async void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				if(HearthStatsManager.SyncInProgress && !_closeAnyway)
				{
					e.Cancel = true;
					var result =
						await
						this.ShowMessageAsync("WARNING! Sync with HearthStats in progress!",
						                      "Closing Hearthstone Deck Tracker now can cause data inconsistencies. Are you sure?",
						                      MessageDialogStyle.AffirmativeAndNegative,
						                      new MetroDialogSettings {AffirmativeButtonText = "close anyway", NegativeButtonText = "wait"});
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
				_doUpdate = false;
				_update = false;

				//wait for update to finish, might otherwise crash when overlay gets disposed
				for(var i = 0; i < 100; i++)
				{
					if(_canShowDown)
						break;
					await Task.Delay(50);
				}

				ReplayReader.CloseViewers();

				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				//Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;
				Config.Instance.SelectedDeckPickerClasses = DeckPickerList.SelectedClasses.ToArray();

				Config.Instance.WindowWidth = (int)(Width - (GridNewDeck.Visibility == Visibility.Visible ? GridNewDeck.ActualWidth : 0));
				Config.Instance.WindowHeight = (int)(Height - _heightChangeDueToSearchBox);
				Config.Instance.TrackerWindowTop = (int)Top;
				Config.Instance.TrackerWindowLeft = (int)(Left + (_movedLeft.HasValue ? _movedLeft.Value : 0));

				//position of add. windows is NaN if they were never opened.
				if(!double.IsNaN(PlayerWindow.Left))
					Config.Instance.PlayerWindowLeft = (int)PlayerWindow.Left;
				if(!double.IsNaN(PlayerWindow.Top))
					Config.Instance.PlayerWindowTop = (int)PlayerWindow.Top;
				Config.Instance.PlayerWindowHeight = (int)PlayerWindow.Height;

				if(!double.IsNaN(OpponentWindow.Left))
					Config.Instance.OpponentWindowLeft = (int)OpponentWindow.Left;
				if(!double.IsNaN(OpponentWindow.Top))
					Config.Instance.OpponentWindowTop = (int)OpponentWindow.Top;
				Config.Instance.OpponentWindowHeight = (int)OpponentWindow.Height;

				if(!double.IsNaN(TimerWindow.Left))
					Config.Instance.TimerWindowLeft = (int)TimerWindow.Left;
				if(!double.IsNaN(TimerWindow.Top))
					Config.Instance.TimerWindowTop = (int)TimerWindow.Top;
				Config.Instance.TimerWindowHeight = (int)TimerWindow.Height;
				Config.Instance.TimerWindowWidth = (int)TimerWindow.Width;

				if(!double.IsNaN(StatsWindow.Left))
					Config.Instance.StatsWindowLeft = (int)StatsWindow.Left;
				if(!double.IsNaN(StatsWindow.Top))
					Config.Instance.StatsWindowTop = (int)StatsWindow.Top;
				Config.Instance.StatsWindowHeight = (int)StatsWindow.Height;
				Config.Instance.StatsWindowWidth = (int)StatsWindow.Width;

				_notifyIcon.Visible = false;
				Overlay.Close();
				HsLogReader.Instance.Stop();
				TimerWindow.Shutdown();
				PlayerWindow.Shutdown();
				OpponentWindow.Shutdown();
				StatsWindow.Shutdown();
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
			if(Game.PlayerDrawn.Count == 0)
			{
				IsShowingIncorrectDeckMessage = false;
				return;
			}

			//wait for player hero to be detected
			for(var i = 0; i < 50; i++)
			{
				if(Game.PlayingAs != null)
					break;
				await Task.Delay(100);
			}
			if(Game.PlayingAs == null)
			{
				IsShowingIncorrectDeckMessage = false;
				return;
			}

			var decks =
				DeckList.Instance.Decks.Where(
				                              d =>
				                              d.Class == Game.PlayingAs && !d.Archived
				                              && Game.PlayerDrawn.Where(c => !c.IsStolen).All(c => d.GetSelectedDeckVersion().Cards.Contains(c)))
				        .ToList();

			if(decks.Contains(DeckList.Instance.ActiveDeckVersion))
				decks.Remove(DeckList.Instance.ActiveDeckVersion);

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			Game.NoMatchingDeck = decks.Count == 0;
			if(decks.Count == 1 && Config.Instance.AutoSelectDetectedDeck)
			{
				var deck = decks.First();
				Logger.WriteLine("Automatically selected deck: " + deck.Name, "IncorrectDeckMessage");
				DeckPickerList.SelectDeck(deck);
				UpdateDeckList(deck);
				UseDeck(deck);
			}
			else if(decks.Count > 0)
			{
				decks.Add(new Deck("Use no deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, false, new List<Card>(),
				                   SerializableVersion.Default, new List<Deck>(), false, "", Guid.Empty, ""));
				var dsDialog = new DeckSelectionDialog(decks);
				dsDialog.ShowDialog();

				var selectedDeck = dsDialog.SelectedDeck;

				if(selectedDeck != null)
				{
					if(selectedDeck.Name == "Use no deck")
						SelectDeck(null, true);
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
			_notifyIcon.Visible = true;
			Hide();
			Visibility = Visibility.Collapsed;
			ShowInTaskbar = false;
		}

		private async void UpdateOverlayAsync()
		{
			UpdateCheck();
			var hsForegroundChanged = false;
			while(_doUpdate)
			{
				if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				{
					if(!Game.IsRunning || Game.CurrentRegion == Region.UNKNOWN)
					{
						//game started
						HsLogReader.Instance.GetCurrentRegion();
					}
					Overlay.UpdatePosition();

					if(!_tempUpdateCheckDisabled && Config.Instance.CheckForUpdates)
					{
						if(!Game.IsRunning && (DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0) && !_showingUpdateMessage)
							UpdateCheck();
					}

					if(!Game.IsRunning)
						Overlay.Update(true);

					Game.IsRunning = true;
					if(User32.IsHearthstoneInForeground())
					{
						if(hsForegroundChanged)
						{
							Overlay.Update(true);
							if(Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
							{
								//if player topmost is set to true before opponent:
								//clicking on the playerwindow and back to hs causes the playerwindow to be behind hs.
								//other way around it works for both windows... what?
								OpponentWindow.Topmost = true;
								PlayerWindow.Topmost = true;
								TimerWindow.Topmost = true;
							}
							hsForegroundChanged = false;
						}
					}
					else if(!hsForegroundChanged)
					{
						if(Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							PlayerWindow.Topmost = false;
							OpponentWindow.Topmost = false;
							TimerWindow.Topmost = false;
						}
						hsForegroundChanged = true;
					}
				}
				else
				{
					Overlay.ShowOverlay(false);
					if(Game.IsRunning)
					{
						//game was closed
						Logger.WriteLine("Exited game", "UpdateOverlayLoop");
						Game.CurrentRegion = Region.UNKNOWN;
						Logger.WriteLine("Reset region", "UpdateOverlayLoop");
						HsLogReader.Instance.ClearLog();
						Game.Reset();
						if(DeckList.Instance.ActiveDeck != null)
							Game.SetPremadeDeck((Deck)DeckList.Instance.ActiveDeck.Clone());
						HsLogReader.Instance.Reset(true);

						if(Config.Instance.CloseWithHearthstone)
							Close();
					}
					Game.IsRunning = false;
				}

				if(Config.Instance.NetDeckClipboardCheck.HasValue && Config.Instance.NetDeckClipboardCheck.Value && _initialized
				   && !User32.IsHearthstoneInForeground())
					CheckClipboardForNetDeckImport();

				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_canShowDown = true;
		}

		private async void UpdateCheck()
		{
			_lastUpdateCheck = DateTime.Now;
			var newVersion = await Helper.CheckForUpdates(false);
			if(newVersion != null)
				ShowNewUpdateMessage(newVersion, false);
			else if(Config.Instance.CheckForBetaUpdates)
			{
				newVersion = await Helper.CheckForUpdates(true);
				if(newVersion != null)
					ShowNewUpdateMessage(newVersion, true);
			}
		}

		private bool CheckClipboardForNetDeckImport()
		{
			try
			{
				if(Clipboard.ContainsText())
				{
					var clipboardContent = Clipboard.GetText();
					if(clipboardContent.StartsWith("netdeckimport") || clipboardContent.StartsWith("trackerimport"))
					{
						var clipboardLines = clipboardContent.Split('\n').ToList();
						var deckName = clipboardLines.FirstOrDefault(line => line.StartsWith("name:"));
						if(!string.IsNullOrEmpty(deckName))
						{
							clipboardLines.Remove(deckName);
							deckName = deckName.Replace("name:", "").Trim();
						}
						var url = clipboardLines.FirstOrDefault(line => line.StartsWith("url:"));
						if(!string.IsNullOrEmpty(url))
						{
							clipboardLines.Remove(url);
							url = url.Replace("url:", "").Trim();
						}
						bool? isArenaDeck = null;
						var arena = clipboardLines.FirstOrDefault(line => line.StartsWith("arena:"));
						if(!string.IsNullOrEmpty(arena))
						{
							clipboardLines.Remove(arena);
							bool isArena;
							if(bool.TryParse(arena.Replace("arena:", "").Trim(), out isArena))
								isArenaDeck = isArena;
						}
						var localized = false;
						var nonEnglish = clipboardLines.FirstOrDefault(line => line.StartsWith("nonenglish:"));
						if(!string.IsNullOrEmpty(nonEnglish))
						{
							clipboardLines.Remove(nonEnglish);
							bool.TryParse(nonEnglish.Replace("nonenglish:", "").Trim(), out localized);
						}
						var tagsRaw = clipboardLines.FirstOrDefault(line => line.StartsWith("tags:"));
						var tags = new List<string>();
						if(!string.IsNullOrEmpty(tagsRaw))
						{
							clipboardLines.Remove(tagsRaw);
							tags = tagsRaw.Replace("tags:", "").Trim().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
						}
						clipboardLines.RemoveAt(0); //"netdeckimport" / "trackerimport"

						var deck = ParseCardString(clipboardLines.Aggregate((c, n) => c + "\n" + n), localized);
						if(deck != null)
						{
							if(tags.Any())
							{
								var reloadTags = false;
								foreach(var tag in tags)
								{
									if(!DeckList.Instance.AllTags.Contains(tag))
									{
										DeckList.Instance.AllTags.Add(tag);
										reloadTags = true;
									}
									deck.Tags.Add(tag);
								}
								if(reloadTags)
								{
									DeckList.Save();
									Helper.MainWindow.ReloadTags();
								}
							}

							if(isArenaDeck.HasValue)
								deck.IsArenaDeck = isArenaDeck.Value;
							deck.Url = url;
							deck.Name = deckName;
							SetNewDeck(deck);
							if(Config.Instance.AutoSaveOnImport)
								SaveDeckWithOverwriteCheck();
							ActivateWindow();
						}
						Clipboard.Clear();
						return true;
					}
				}
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "NetDeckClipbardCheck");
				return false;
			}
			return false;
		}

		private async void ShowNewUpdateMessage(Version newVersion, bool beta)
		{
			if(_showingUpdateMessage)
				return;
			_showingUpdateMessage = true;

			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Download", NegativeButtonText = "Not now"};
			if(newVersion == null)
			{
				_showingUpdateMessage = false;
				return;
			}
			try
			{
				await Task.Delay(10000);
				ActivateWindow();
				while(Visibility != Visibility.Visible || WindowState == WindowState.Minimized)
					await Task.Delay(100);
				var newVersionString = string.Format("{0}.{1}.{2}", newVersion.Major, newVersion.Minor, newVersion.Build);
				var betaString = beta ? " BETA" : "";
				var result =
					await
					this.ShowMessageAsync("New" + betaString + " Update available!", "Press \"Download\" to automatically download.",
					                      MessageDialogStyle.AffirmativeAndNegative, settings);

				if(result == MessageDialogResult.Affirmative)
				{
					//recheck, in case there was no immediate response to the dialog
					if((DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
					{
						newVersion = await Helper.CheckForUpdates(beta);
						if(newVersion != null)
							newVersionString = string.Format("{0}.{1}.{2}", newVersion.Major, newVersion.Minor, newVersion.Build);
					}
					try
					{
						Process.Start("HDTUpdate.exe", string.Format("{0} {1}", Process.GetCurrentProcess().Id, newVersionString));
						Close();
						Application.Current.Shutdown();
					}
					catch
					{
						Logger.WriteLine("Error starting updater");
						Process.Start(releaseDownloadUrl);
					}
				}
				else
					_tempUpdateCheckDisabled = true;

				_showingUpdateMessage = false;
			}
			catch(Exception e)
			{
				_showingUpdateMessage = false;
				Logger.WriteLine("Error showing new update message\n" + e.Message);
			}
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
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(Config.Instance.StatsInWindow)
			{
				StatsWindow.StatsControl.SetDeck(deck);
				StatsWindow.WindowState = WindowState.Normal;
				StatsWindow.Show();
				StatsWindow.Activate();
			}
			else
			{
				DeckStatsFlyout.SetDeck(deck);
				FlyoutDeckStats.IsOpen = true;
			}
		}

		private void DeckPickerList_OnSelectedDeckChanged(DeckPicker sender, Deck deck)
		{
			SelectDeck(deck, false);
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
					Overlay.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					Logger.WriteLine("Set player itemsource as PlayerDeck", "Tracker");
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

					int useNoDeckMenuItem = _notifyIcon.ContextMenu.MenuItems.IndexOfKey("useNoDeck");
					_notifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Checked = false;
				}
			}
			else
			{
				Game.IsUsingPremade = false;

				if(DeckList.Instance.ActiveDeck != null)
					DeckList.Instance.ActiveDeck.IsSelectedInGui = false;

				DeckList.Instance.ActiveDeck = null;
				if(setActive)
				{
					DeckPickerList.DeselectDeck();
					Overlay.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
					PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
					Logger.WriteLine("set player item source to PlayerDrawn", "Tracker");
				}

				int useNoDeckMenuItem = _notifyIcon.ContextMenu.MenuItems.IndexOfKey("useNoDeck");
				_notifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Checked = true;
			}

			//set up stats
			var statsTitle = string.Format("Stats{0}", deck == null ? "" : ": " + deck.Name);
			StatsWindow.Title = statsTitle;
			FlyoutDeckStats.Header = statsTitle;
			StatsWindow.StatsControl.SetDeck(deck);
			DeckStatsFlyout.SetDeck(deck);

			if(setActive)
				UseDeck(deck);
			UpdateDeckList(deck);
			EnableMenuItems(deck != null);
			ManaCurveMyDecks.SetDeck(deck);
			UpdatePanelVersionComboBox(deck);
			if(setActive)
			{
				Overlay.ListViewPlayer.Items.Refresh();
				PlayerWindow.ListViewPlayer.Items.Refresh();
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
					SelectDeck(deck, true);
					DeckPickerList.UpdateDecks();
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
	}
}