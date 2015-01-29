﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Properties

		public readonly Decks DeckList;
		public readonly List<Deck> DefaultDecks;
		public readonly Version NewVersion;
		public readonly OpponentWindow OpponentWindow;
		public readonly OverlayWindow Overlay;
		public readonly PlayerWindow PlayerWindow;
		public readonly StatsWindow StatsWindow;
		public readonly TimerWindow TimerWindow;
		private readonly string _decksPath;
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
		private bool _doUpdate;
		private DateTime _lastUpdateCheck;
		private Deck _newDeck;
		private bool _newDeckUnsavedChanges;
		private Deck _originalDeck;
		private bool _tempUpdateCheckDisabled;
		private Version _updatedVersion;

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		public Visibility VersionComboBoxVisibility
		{
			get
			{
				return DeckPickerList.SelectedDeck != null && DeckPickerList.SelectedDeck.HasVersions ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion

		#region Constructor

		public MainWindow()
		{
			// Set working directory to path of executable
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			InitializeComponent();

			Trace.Listeners.Add(new TextBoxTraceListener(Options.TextBoxLog));

			EnableMenuItems(false);

			try
			{
				if(File.Exists("Updater_new.exe"))
				{
					if(File.Exists("Updater.exe"))
						File.Delete("Updater.exe");
					File.Move("Updater_new.exe", "Updater.exe");
				}
			}
			catch
			{
				Logger.WriteLine("Error updating updater");
			}

			Helper.MainWindow = this;
			/*_configPath =*/
			Config.Load();
			HsLogReader.Create();

			var configVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion) ? null : new Version(Config.Instance.CreatedByVersion);

			Version currentVersion;
			if(Config.Instance.CheckForUpdates)
			{
				currentVersion = Helper.CheckForUpdates(out NewVersion);
				_lastUpdateCheck = DateTime.Now;
			}
			else
				currentVersion = Helper.GetCurrentVersion();

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

			_decksPath = Config.Instance.DataDir + "PlayerDecks.xml";
			SetupDeckListFile();
			try
			{
				DeckList = XmlManager<Decks>.Load(_decksPath);
			}
			catch(Exception e)
			{
				MessageBox.Show(
				                e.Message + "\n\n" + e.InnerException + "\n\n If you don't know how to fix this, please delete " + _decksPath
				                + " (this will cause you to lose your decks).", "Error loading PlayerDecks.xml");
				Application.Current.Shutdown();
			}

			foreach(var deck in DeckList.DecksList)
				DeckPickerList.AddDeck(deck);

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
			_notifyIcon.ContextMenu.MenuItems.Add("Use no deck", (sender, args) => DeselectDeck());
			_notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Autoselect deck")
			{
				MenuItems =
				{
					new MenuItem("On", (sender, args) => AutoDeckDetection(true)),
					new MenuItem("Off", (sender, args) => AutoDeckDetection(false))
				}
			});
			_notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Class cards first")
			{
				MenuItems =
				{
					new MenuItem("Yes", (sender, args) => SortClassCardsFirst(true)),
					new MenuItem("No", (sender, args) => SortClassCardsFirst(false))
				}
			});
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
			if(!DeckList.AllTags.Contains("All"))
			{
				DeckList.AllTags.Add("All");
				WriteDecks();
			}
			if(!DeckList.AllTags.Contains("Favorite"))
			{
				if(DeckList.AllTags.Count > 1)
					DeckList.AllTags.Insert(1, "Favorite");
				else
					DeckList.AllTags.Add("Favorite");
				WriteDecks();
			}
			if(!DeckList.AllTags.Contains("Arena"))
			{
				DeckList.AllTags.Add("Arena");
				WriteDecks();
			}
			if(!DeckList.AllTags.Contains("Constructed"))
			{
				DeckList.AllTags.Add("Constructed");
				WriteDecks();
			}
			if(!DeckList.AllTags.Contains("None"))
			{
				DeckList.AllTags.Add("None");
				WriteDecks();
			}

			Options.ComboboxAccent.ItemsSource = ThemeManager.Accents;
			Options.ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			Options.ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			Options.ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			Options.ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

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

			FillElementSorters();

			//this has to happen before reader starts
			var lastDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == Config.Instance.LastDeck);
			DeckPickerList.SelectDeck(lastDeck);

			TurnTimer.Create(90);

			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			TagControlEdit.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlEdit.PnlSortDecks.Visibility = Visibility.Collapsed;


			UpdateDbListView();

			_doUpdate = _foundHsDirectory;

			Options.MainWindowInitialized();

			DeckPickerList.UpdateList();
			if(lastDeck != null)
			{
				DeckPickerList.SelectDeck(lastDeck);
				UpdateDeckList(lastDeck);
				UseDeck(lastDeck);
			}

			if(_foundHsDirectory)
				HsLogReader.Instance.Start();

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			DeckPickerList.SortDecks();

			CopyReplayFiles();

			UpdateOverlayAsync();

			_initialized = true;
		}

		#endregion

		#region GENERAL GUI

		private void MetroWindow_Activated(object sender, EventArgs e)
		{
			Topmost = true;
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			Topmost = false;
		}

		private void MetroWindow_StateChanged(object sender, EventArgs e)
		{
			if(Config.Instance.MinimizeToTray && WindowState == WindowState.Minimized)
				MinimizeToTray();
		}

		private async void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				_doUpdate = false;

				//wait for update to finish, might otherwise crash when overlay gets disposed
				for(var i = 0; i < 100; i++)
				{
					if(_canShowDown)
						break;
					await Task.Delay(50);
				}

				ReplayReader.CloseViewers();

				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;

				Config.Instance.WindowWidth = (int)(Width - (GridNewDeck.Visibility == Visibility.Visible ? GridNewDeck.ActualWidth : 0));
				Config.Instance.WindowHeight = (int)Height;
				Config.Instance.TrackerWindowTop = (int)Top;
				Config.Instance.TrackerWindowLeft = (int)Left;

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
				WriteDecks();
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

		public void ShowIncorrectDeckMessage()
		{
			var decks =
				DeckList.DecksList.Where(d => d.Class == Game.PlayingAs && Game.PlayerDrawn.All(c => d.GetSelectedDeckVersion().Cards.Contains(c)))
				        .ToList();

			if(decks.Contains(DeckPickerList.GetSelectedDeckVersion()))
				decks.Remove(DeckPickerList.GetSelectedDeckVersion());

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			if(decks.Count == 1 && Config.Instance.AutoSelectDetectedDeck)
			{
				var deck = decks.First();
				Logger.WriteLine("Automatically selected deck: " + deck.Name);
				DeckPickerList.SelectDeck(deck);
				UpdateDeckList(deck);
				UseDeck(deck);
			}
			else if(decks.Count > 0)
			{
				decks.Add(new Deck("Use no deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, SerializableVersion.Default,
				                   new List<Deck>()));
				var dsDialog = new DeckSelectionDialog(decks);

				//todo: System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=ClassColor; DataItem=null; target element is 'GradientStop' (HashCode=7260326); target property is 'Color' (type 'Color')
				//when opened for seconds time. why?
				dsDialog.ShowDialog();


				var selectedDeck = dsDialog.SelectedDeck;

				if(selectedDeck != null)
				{
					if(selectedDeck.Name == "Use no deck")
						DeselectDeck();
					else
					{
						Logger.WriteLine("Selected deck: " + selectedDeck.Name);
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
			Visibility = Visibility.Collapsed;
			ShowInTaskbar = false;
		}

		private async void UpdateOverlayAsync()
		{
			var hsForegroundChanged = false;
			while(_doUpdate)
			{
				if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				{
					Overlay.UpdatePosition();

					if(!_tempUpdateCheckDisabled && Config.Instance.CheckForUpdates)
					{
						if(!Game.IsRunning && (DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
						{
							Version newVersion;
							var currentVersion = Helper.CheckForUpdates(out newVersion);
							if(currentVersion != null && newVersion != null)
								ShowNewUpdateMessage(newVersion);
							_lastUpdateCheck = DateTime.Now;
						}
					}

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
						Logger.WriteLine("Exited game");
						HsLogReader.Instance.ClearLog();
						Game.Reset();
						if(DeckPickerList.SelectedDeck != null)
							Game.SetPremadeDeck((Deck)DeckPickerList.SelectedDeck.Clone());
						HsLogReader.Instance.Reset(true);

						if(Config.Instance.CloseWithHearthstone)
							Close();
					}
					Game.IsRunning = false;
				}

				if(Config.Instance.NetDeckClipboardCheck.HasValue && Config.Instance.NetDeckClipboardCheck.Value && _initialized
				   && !User32.IsHearthstoneInForeground())
					await CheckClipboardForNetDeckImport();

				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_canShowDown = true;
		}

		private async Task<bool> CheckClipboardForNetDeckImport()
		{
			try
			{
				if(Clipboard.ContainsText())
				{
					var clipboardContent = Clipboard.GetText();
					if(clipboardContent.StartsWith("netdeckimport"))
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
						clipboardLines.RemoveAt(0); //"netdeckimport"

						var deck = ParseCardString(clipboardLines.Aggregate((c, n) => c + "\n" + n));
						if(deck != null)
						{
							deck.Url = url;
							deck.Note = url;
							deck.Name = deckName;
							SetNewDeck(deck);
							if(Config.Instance.AutoSaveOnImport)
								await SaveDeckWithOverwriteCheck();
							ActivateWindow();
						}
						Clipboard.Clear();
						return true;
					}
				}
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString());
				return false;
			}
			return false;
		}

		private async void ShowNewUpdateMessage(Version newVersion = null)
		{
			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Download", NegativeButtonText = "Not now"};
			var version = newVersion ?? NewVersion;
			if(version == null)
				return;
			try
			{
				ActivateWindow();
				var newVersionString = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
				var result =
					await
					this.ShowMessageAsync("New Update available!", "Press \"Download\" to automatically download.",
					                      MessageDialogStyle.AffirmativeAndNegative, settings);

				if(result == MessageDialogResult.Affirmative)
				{
					//recheck, in case there was no immediate response to the dialog
					if((DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
					{
						Helper.CheckForUpdates(out version);
						if(version != null)
							newVersionString = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
					}
					try
					{
						Process.Start("Updater.exe", string.Format("{0} {1}", Process.GetCurrentProcess().Id, newVersionString));
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
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error showing new update message\n" + e.Message);
			}
		}

		public async Task Restart()
		{
			await this.ShowMessageAsync("Restarting tracker", "");
			Process.Start(Application.ResourceAssembly.Location);
			Application.Current.Shutdown();
		}

		public void WriteDecks()
		{
			if(_initialized)
				XmlManager<Decks>.Save(_decksPath, DeckList);
		}

		public void ActivateWindow()
		{
			Show();
			WindowState = WindowState.Normal;
			ShowInTaskbar = true;
			Activate();
		}

		#endregion

		#region MY DECKS - GUI

		private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
		{
			DeselectDeck();
		}

		public void DeselectDeck()
		{
			Logger.WriteLine("set player item source as drawn");
			Overlay.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			Game.IsUsingPremade = false;

			if(Config.Instance.StatsInWindow)
			{
				StatsWindow.Title = "Stats";
				StatsWindow.StatsControl.SetDeck(null);
			}
			else
			{
				FlyoutDeckStats.Header = "Stats";
				DeckStatsFlyout.SetDeck(null);
			}

			if(DeckPickerList.SelectedDeck != null)
				DeckPickerList.SelectedDeck.IsSelectedInGui = false;

			DeckPickerList.SelectedDeck = null;
			DeckPickerList.SelectedIndex = -1;
			DeckPickerList.ListboxPicker.Items.Refresh();

			UpdateDeckList(null);
			UseDeck(null);
			EnableMenuItems(false);
			ManaCurveMyDecks.ClearDeck();
		}

		private void BtnDeckStats_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDeck;
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
			if(deck != null)
			{
				//set up notes
				DeckNotesEditor.SetDeck(deck);
				var flyoutHeader = deck.Name.Length >= 20 ? string.Join("", deck.Name.Take(17)) + "..." : deck.Name;
				FlyoutNotes.Header = flyoutHeader;
				if(Config.Instance.StatsInWindow)
				{
					StatsWindow.Title = "Stats: " + deck.Name;
					StatsWindow.StatsControl.SetDeck(deck);
				}
				else
				{
					FlyoutDeckStats.Header = "Stats: " + deck.Name;
					DeckStatsFlyout.SetDeck(deck);
				}

				UseDeck(deck);
				Game.IsUsingPremade = true;
				//change player deck itemsource
				if(Overlay.ListViewPlayer.ItemsSource != Game.PlayerDeck)
				{
					Overlay.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					Overlay.ListViewPlayer.Items.Refresh();
					PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					PlayerWindow.ListViewPlayer.Items.Refresh();
					Logger.WriteLine("Set player itemsource as playerdeck");
				}
				UpdateDeckList(deck);
				Logger.WriteLine("Switched to deck: " + deck.Name);

				//set and save last used deck for class
				while(DeckList.LastDeckClass.Any(ldc => ldc.Class == deck.Class))
				{
					var lastSelected = DeckList.LastDeckClass.FirstOrDefault(ldc => ldc.Class == deck.Class);
					if(lastSelected != null)
						DeckList.LastDeckClass.Remove(lastSelected);
					else
						break;
				}
				DeckList.LastDeckClass.Add(new DeckInfo {Class = deck.Class, Name = deck.Name});
				WriteDecks();
				EnableMenuItems(true);
				ManaCurveMyDecks.SetDeck(deck);
				TagControlEdit.SetSelectedTags(deck.Tags);
				MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
				MenuItemQuickSetTag.Items.Refresh();
				MenuItemUpdateDeck.IsEnabled = !string.IsNullOrEmpty(deck.Url);

				ComboBoxDeckVersion.ItemsSource = deck.VersionsIncludingSelf;
				ComboBoxDeckVersion.SelectedItem = deck.SelectedVersion;
				PanelVersionComboBox.Visibility = deck.HasVersions ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				ComboBoxDeckVersion.ItemsSource = null;
				EnableMenuItems(false);
				PanelVersionComboBox.Visibility = Visibility.Collapsed;
			}
		}

		#endregion

		public void UseDeck(Deck selected)
		{
			Game.Reset();

			if(selected != null)
				Game.SetPremadeDeck((Deck)selected.Clone());

			//needs to be true for automatic deck detection to work
			HsLogReader.Instance.Reset(true);
			Overlay.Update(false);
			Overlay.SortViews();
		}

		public void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if(selected == null)
			{
				Config.Instance.LastDeck = string.Empty;
				Config.Save();
				return;
			}
			ListViewDeck.ItemsSource = selected.GetSelectedDeckVersion().Cards;
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			if(Config.Instance.LastDeck != selected.Name)
			{
				Config.Instance.LastDeck = selected.Name;
				Config.Save();
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
			Config.Instance.AutoDeckDetection = true;
			Config.Save();
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoDeckDetection = false;
			Config.Save();
		}

		private void AutoDeckDetection(bool enable)
		{
			CheckboxDeckDetection.IsChecked = enable;
			Config.Instance.AutoDeckDetection = enable;
			Config.Save();
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
		}

		private void MenuItemQuickFilter_Click(object sender, EventArgs e)
		{
			var tag = ((TextBlock)sender).Text;
			var actualTag = SortFilterDecksFlyout.Tags.FirstOrDefault(t => t.Name.ToUpperInvariant() == tag);
			if(actualTag != null)
			{
				var tags = new List<string> {actualTag.Name};
				SortFilterDecksFlyout.SetSelectedTags(tags);
				Helper.MainWindow.DeckPickerList.SetSelectedTags(tags);
				Config.Instance.SelectedTags = tags;
				Config.Save();
				Helper.MainWindow.StatsWindow.StatsControl.LoadOverallStats();
				Helper.MainWindow.DeckStatsFlyout.LoadOverallStats();
			}
		}

		private void MenuItemReplayLastGame_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var newest =
					Directory.GetFiles(Config.Instance.ReplayDir).Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).FirstOrDefault();
				if(newest != null)
					ReplayReader.Read(newest.FullName);
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString());
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
					ReplayReader.Read(dialog.FileName);
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString());
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

		private async void MenuItemSaveVersionCurrent_OnClick(object sender, RoutedEventArgs e)
		{
			await SaveDeckWithOverwriteCheck(_newDeck.Version);
		}

		private async void MenuItemSaveVersionMinor_OnClick(object sender, RoutedEventArgs e)
		{
			await SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMinor(_newDeck.Version));
		}

		private async void MenuItemSaveVersionMajor_OnClick(object sender, RoutedEventArgs e)
		{
			await SaveDeckWithOverwriteCheck(SerializableVersion.IncreaseMajor(_newDeck.Version));
		}

		private void ComboBoxDeckVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized || DeckPickerList.ChangedSelection)
				return;
			var deck = DeckPickerList.SelectedDeck;
			if(deck == null)
				return;
			var version = ComboBoxDeckVersion.SelectedItem as SerializableVersion;
			if(version != null)
			{
				DeckPickerList.RemoveDeck(deck);
				deck.SelectVersion(version);
				WriteDecks();
				DeckPickerList.AddAndSelectDeck(deck);
				DeckPickerList.UpdateList();
				UpdateDeckList(DeckPickerList.SelectedDeck);
				ManaCurveMyDecks.UpdateValues();
				UseDeck(deck);
				Console.WriteLine(version);
			}
		}
	}
}