#region

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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ContextMenu = System.Windows.Forms.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Point = System.Drawing.Point;
using SystemColors = System.Windows.SystemColors;

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
		public readonly Version NewVersion;
		public readonly OpponentWindow OpponentWindow;
		public readonly OverlayWindow Overlay;
		public readonly PlayerWindow PlayerWindow;
		public readonly StatsWindow StatsWindow;
		public readonly TimerWindow TimerWindow;
		private readonly string _configPath;
		private readonly string _decksPath;
		private readonly bool _foundHsDirectory;
		private readonly bool _initialized;

		private readonly string _logConfigPath =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
			@"\Blizzard\Hearthstone\log.config";

		private readonly NotifyIcon _notifyIcon;
		private readonly bool _updatedLogConfig;

		public bool EditingDeck;

		public ReadOnlyCollection<string> EventKeys =
			new ReadOnlyCollection<string>(new[] {"None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"});

		public bool IsShowingIncorrectDeckMessage;
		public bool NeedToIncorrectDeckMessage;
		public Deck NewDeck;
		private bool _canShowDown;
		private bool _doUpdate;
		private bool _newContainsDeck;
		private Version _updatedVersion;

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
			_configPath = Config.Load();
			HsLogReader.Create();

			var configVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion)
				                    ? null
				                    : new Version(Config.Instance.CreatedByVersion);

			Version currentVersion;
			if(Config.Instance.CheckForUpdates)
			{
				currentVersion = Helper.CheckForUpdates(out NewVersion);
				_lastUpdateCheck = DateTime.Now;
			}
			else
				currentVersion = Helper.GetCurrentVersion();

			if(currentVersion != null)
			{
				TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", currentVersion.Major, currentVersion.Minor,
				                                     currentVersion.Build);

				// Assign current version to the config instance so that it will be saved when the config
				// is rewritten to disk, thereby telling us what version of the application created it
				Config.Instance.CreatedByVersion = currentVersion.ToString();
			}

			ConvertLegacyConfig(currentVersion, configVersion);

			if(Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

			if(Config.Instance.GenerateLog)
			{
				Directory.CreateDirectory("Logs");
				var listener = new TextWriterTraceListener(Config.Instance.LogFilePath);
				Trace.Listeners.Add(listener);
				Trace.AutoFlush = true;
			}

			_foundHsDirectory = FindHearthstoneDir();

			if(_foundHsDirectory)
				_updatedLogConfig = UpdateLogConfigFile();

			//hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
			//Game.Create();
			Game.Reset();

			_decksPath = Config.Instance.HomeDir + "PlayerDecks.xml";
			SetupDeckListFile();
			try
			{
				DeckList = XmlManager<Decks>.Load(_decksPath);
			}
			catch(Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + _decksPath +
					" (this will cause you to lose your decks).",
					"Error loading PlayerDecks.xml");
				Application.Current.Shutdown();
			}

			foreach(var deck in DeckList.DecksList)
				DeckPickerList.AddDeck(deck);

			SetupDeckStatsFile();
			DeckStatsList.Load();

			_notifyIcon = new NotifyIcon {Icon = new Icon(@"Images/HearthstoneDeckTracker.ico"), Visible = true, ContextMenu = new ContextMenu()};
			_notifyIcon.ContextMenu.MenuItems.Add("Show", (sender, args) => ActivateWindow());
			_notifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, args) => Close());
			_notifyIcon.MouseClick += (sender, args) => { if(args.Button == MouseButtons.Left) ActivateWindow(); };

			NewDeck = new Deck();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;

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

			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

			LoadConfig();

			FillElementSorters();

			//this has to happen before reader starts
			var lastDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == Config.Instance.LastDeck);
			DeckPickerList.SelectDeck(lastDeck);

			DeckOptionsFlyout.DeckOptionsButtonClicked += sender => { FlyoutDeckOptions.IsOpen = false; };

			DeckImportFlyout.DeckOptionsButtonClicked += sender => { FlyoutDeckImport.IsOpen = false; };

			TurnTimer.Create(90);

			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			TagControlNewDeck.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlMyDecks.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlNewDeck.PnlSortDecks.Visibility = Visibility.Collapsed;
			TagControlMyDecks.PnlSortDecks.Visibility = Visibility.Collapsed;

			//SortFilterDecksFlyout.SelectedTagsChanged += SortFilterDecksFlyoutOnSelectedTagsChanged;
			//SortFilterDecksFlyout.OperationChanged += SortFilterDecksFlyoutOnOperationChanged;


			UpdateDbListView();

			_doUpdate = _foundHsDirectory;
			UpdateOverlayAsync();

			_initialized = true;

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
		}

		private void FillElementSorters()
		{
			ElementSorterPlayer.IsPlayer = true;
			foreach(var itemName in Config.Instance.PanelOrderPlayer)
			{
				switch(itemName)
				{
					case "Deck Title":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Deck Title", Config.Instance.ShowDeckTitle, value => Config.Instance.ShowDeckTitle = value, true));
						break;
					case "Cards":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Cards", !Config.Instance.HidePlayerCards, value => Config.Instance.HidePlayerCards = !value, true));
						break;
					case "Card Counter":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HidePlayerCardCount, value => Config.Instance.HidePlayerCardCount = !value, true));
						break;
					case "Draw Chances":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideDrawChances, value => Config.Instance.HideDrawChances = !value, true));
						break;
					case "Wins":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Wins", Config.Instance.ShowDeckWins, value => Config.Instance.ShowDeckWins = value, true));
						break;
				}
			}
			Overlay.UpdatePlayerLayout();
			PlayerWindow.UpdatePlayerLayout();

			ElementSorterOpponent.IsPlayer = false;
			foreach(var itemName in Config.Instance.PanelOrderOpponent)
			{
				switch(itemName)
				{
					case "Cards":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Cards", !Config.Instance.HideOpponentCards, value => Config.Instance.HideOpponentCards = !value, false));
						break;
					case "Card Counter":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HideOpponentCardCount, value => Config.Instance.HideOpponentCardCount = !value, false));
						break;
					case "Draw Chances":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideOpponentDrawChances, value => Config.Instance.HideOpponentDrawChances = !value, false));
						break;
					case "Win Rate":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Win Rate", Config.Instance.ShowWinRateAgainst, value => Config.Instance.ShowWinRateAgainst = value, false));
						break;
				}
			}
			Overlay.UpdateOpponentLayout();
			OpponentWindow.UpdateOpponentLayout();
		}

		private void SetupDeckStatsFile()
		{
			var appDataPath = Config.Instance.AppDataPath + @"\DeckStats.xml";
			var appDataGamesDirPath = Config.Instance.AppDataPath + @"\Games";
			const string localPath = "DeckStats.xml";
			const string localGamesDirPath = "Games";
			if(Config.Instance.SaveInAppData)
			{
				if(File.Exists(localPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						Helper.CopyFolder(appDataGamesDirPath, appDataGamesDirPath + time);
						Directory.Delete(appDataGamesDirPath, true);
						Logger.WriteLine("Created backups of deckstats and games in appdata");
					}
					File.Move(localPath, appDataPath);
					Logger.WriteLine("Moved DeckStats to appdata");
					Helper.CopyFolder(localGamesDirPath, appDataGamesDirPath);
					Directory.Delete(localGamesDirPath, true);
					Logger.WriteLine("Moved Games to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(localPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(localPath, localPath + time);
					Helper.CopyFolder(localGamesDirPath, localGamesDirPath + time);
					Directory.Delete(localGamesDirPath, true);
					Logger.WriteLine("Created backups of deckstats and games locally");
				}
				File.Move(appDataPath, localPath);
				Logger.WriteLine("Moved DeckStats to local");
				Helper.CopyFolder(appDataGamesDirPath, localGamesDirPath);
				Directory.Delete(appDataGamesDirPath, true);
				Logger.WriteLine("Moved Games to appdata");
			}

			var filePath = Config.Instance.HomeDir + "DeckStats.xml";
			//load saved decks
			if(!File.Exists(filePath))
			{
				//avoid overwriting decks file with new releases.
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DeckStatsList></DeckStatsList>");
			}
		}

		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var config = Config.Instance;
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if(configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				// We previously assumed negative pixel coordinates were invalid, but in fact they can go negative
				// with multi-screen setups. Negative positions were being used to represent 'no specific position'
				// as a default. That means that when the windows are created for the first time, we let the operating
				// system decide where to place them. As we should not be using negative positions for this purpose, since
				// they are in fact a valid range of pixel positions, we now use nullable types instead. The default
				// 'no specific position' is now expressed when the positions are null.
				{
					if(config.TrackerWindowLeft.HasValue && config.TrackerWindowLeft.Value < 0)
					{
						config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
						converted = true;
					}
					if(config.TrackerWindowTop.HasValue && config.TrackerWindowTop.Value < 0)
					{
						config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
						converted = true;
					}

					if(config.PlayerWindowLeft.HasValue && config.PlayerWindowLeft.Value < 0)
					{
						config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
						converted = true;
					}
					if(config.PlayerWindowTop.HasValue && config.PlayerWindowTop.Value < 0)
					{
						config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
						converted = true;
					}

					if(config.OpponentWindowLeft.HasValue && config.OpponentWindowLeft.Value < 0)
					{
						config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
						converted = true;
					}
					if(config.OpponentWindowTop.HasValue && config.OpponentWindowTop.Value < 0)
					{
						config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
						converted = true;
					}

					if(config.TimerWindowLeft.HasValue && config.TimerWindowLeft.Value < 0)
					{
						config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
						converted = true;
					}
					if(config.TimerWindowTop.HasValue && config.TimerWindowTop.Value < 0)
					{
						config.TimerWindowTop = Config.Defaults.TimerWindowTop;
						converted = true;
					}
				}

				// Player and opponent window heights were previously set to zero as a default, and then
				// a bit of logic was used when creating the windows: if height == 0, then set height to 400.
				// This was a little pointless and also inconsistent with the way the default timer window
				// dimensions were implemented. Unfortunately we cannot make this consistent without
				// breaking legacy config files, where the height will still be stored as zero. So
				// we handle the changed semantics here.
				{
					if(config.PlayerWindowHeight == 0)
					{
						config.PlayerWindowHeight = Config.Defaults.PlayerWindowHeight;
						converted = true;
					}

					if(config.OpponentWindowHeight == 0)
					{
						config.OpponentWindowHeight = Config.Defaults.OpponentWindowHeight;
						converted = true;
					}
				}
			}
			else if(configVersion <= v0_3_21) // Config must be between v0.3.20 and v0.3.21 inclusive
				// It was still possible in 0.3.21 to see (-32000, -32000) window positions
				// under certain circumstances (GitHub issue #135).
			{
				if(config.TrackerWindowLeft == -32000)
				{
					config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
					converted = true;
				}
				if(config.TrackerWindowTop == -32000)
				{
					config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
					converted = true;
				}

				if(config.PlayerWindowLeft == -32000)
				{
					config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
					converted = true;
				}
				if(config.PlayerWindowTop == -32000)
				{
					config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
					converted = true;
				}

				if(config.OpponentWindowLeft == -32000)
				{
					config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
					converted = true;
				}
				if(config.OpponentWindowTop == -32000)
				{
					config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
					converted = true;
				}

				if(config.TimerWindowLeft == -32000)
				{
					config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
					converted = true;
				}
				if(config.TimerWindowTop == -32000)
				{
					config.TimerWindowTop = Config.Defaults.TimerWindowTop;
					converted = true;
				}

				//player scaling used to beincreased by a very minimal about to curcumvent some problem,
				//should no longer be required. not sure is the increment is actually noticable, but resetting can't hurt
				if(config.OverlayOpponentScaling > 100)
				{
					config.OverlayOpponentScaling = 100;
					converted = true;
				}
				if(config.OverlayPlayerScaling > 100)
				{
					config.OverlayPlayerScaling = 100;
					converted = true;
				}
			}

			if(converted)
			{
				Config.SaveBackup();
				Config.Save();
			}

			if(configVersion != null && currentVersion > configVersion)
				_updatedVersion = currentVersion;
		}

		private bool FindHearthstoneDir()
		{
			var found = false;
			if(string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory) ||
			   !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using(
					var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone")
					)
				{
					if(hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the installlocation actually is correct (possibly moved?)
						if(File.Exists(hsDir + @"\Hearthstone.exe"))
						{
							Config.Instance.HearthstoneDirectory = hsDir;
							Config.Save();
							found = true;
						}
					}
				}
			}
			else
				found = true;

			return found;
		}

		private bool UpdateLogConfigFile()
		{
			var updated = false;
			//check for log config and create if not existing
			try
			{
				//always overwrite is true by default. 
				if(!File.Exists(_logConfigPath))
				{
					updated = true;
					File.Copy("Files/log.config", _logConfigPath, true);
					Logger.WriteLine(string.Format("Copied log.config to {0} (did not exist)", _configPath));
				}
				else
				{
					//update log.config if newer
					var localFile = new FileInfo(_logConfigPath);
					var file = new FileInfo("Files/log.config");
					if(file.LastWriteTime > localFile.LastWriteTime)
					{
						updated = true;
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (file newer)", _configPath));
					}
					else if(Config.Instance.AlwaysOverwriteLogConfig)
					{
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (AlwaysOverwriteLogConfig)", _configPath));
					}
				}
			}
			catch(Exception e)
			{
				if(_updatedLogConfig)
				{
					MessageBox.Show(
						e.Message + "\n\n" + e.InnerException +
						"\n\n Please manually copy the log.config from the Files directory to \"%LocalAppData%/Blizzard/Hearthstone\".",
						"Error writing log.config");
					Application.Current.Shutdown();
				}
			}
			return updated;
		}

		private void SetupDeckListFile()
		{
			var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
			const string localPath = "PlayerDecks.xml";
			if(Config.Instance.SaveInAppData)
			{
				if(File.Exists(localPath))
				{
					if(File.Exists(appDataPath))
						//backup in case the file already exists
						File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
					File.Move(localPath, appDataPath);
					Logger.WriteLine("Moved decks to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(localPath))
					//backup in case the file already exists
					File.Move(localPath, localPath + DateTime.Now.ToFileTime());
				File.Move(appDataPath, localPath);
				Logger.WriteLine("Moved decks to local");
			}

			//load saved decks
			if(!File.Exists(_decksPath))
			{
				//avoid overwriting decks file with new releases.
				using(var sr = new StreamWriter(_decksPath, false))
					sr.WriteLine("<Decks></Decks>");
			}
			else if(!File.Exists(_decksPath + ".old"))
				//the new playerdecks.xml wont work with versions below 0.2.19, make copy
				File.Copy(_decksPath, _decksPath + ".old");
		}

		#endregion

		#region GENERAL GUI

		private int _lastSelectedTab;

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

		private void MinimizeToTray()
		{
			_notifyIcon.Visible = true;
			//_notifyIcon.ShowBalloonTip(2000, "Hearthstone Deck Tracker", "Minimized to tray", ToolTipIcon.Info);
			Hide();
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

				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;

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

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if(presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			if(!_foundHsDirectory)
			{
				ShowHsNotInstalledMessage();
				return;
			}
			if(NewVersion != null)
				ShowNewUpdateMessage();
			if(_updatedVersion != null)
				ShowUpdateNotesMessage(_updatedVersion);

			if(_updatedLogConfig)
			{
				ShowMessage("Restart Hearthstone",
				            "This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			//preload the manacurve in new deck
			TabControlTracker.SelectedIndex = 1;
			TabControlTracker.UpdateLayout();
			TabControlTracker.SelectedIndex = 0;

			ManaCurveMyDecks.UpdateValues();
		}

		private void TabControlTracker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			if(_lastSelectedTab == TabControlTracker.SelectedIndex) return;
			_lastSelectedTab = TabControlTracker.SelectedIndex;
			UpdateTabMarker();
		}

		private async void UpdateTabMarker()
		{
			var tabItem = TabControlTracker.SelectedItem as TabItem;
			if(tabItem == null) return;
			await Task.Delay(50);
			SelectedTabMarker.Width = tabItem.ActualWidth;
			var offset = TabControlTracker.Items.Cast<TabItem>().TakeWhile(t => t != tabItem).Sum(t => t.ActualWidth);
			SelectedTabMarker.Margin = new Thickness(offset, 40, 0, 0);
		}

		#endregion

		#region GENERAL METHODS

		private DateTime _lastUpdateCheck;
		private bool _tempUpdateCheckDisabled;

		public void ShowIncorrectDeckMessage()
		{
			var decks =
				DeckList.DecksList.Where(
					d => d.Class == Game.PlayingAs && Game.PlayerDrawn.All(c => d.Cards.Contains(c))
					).ToList();
			if(decks.Contains(DeckPickerList.SelectedDeck))
				decks.Remove(DeckPickerList.SelectedDeck);

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			if(decks.Count > 0)
			{
				var dsDialog = new DeckSelectionDialog(decks);

				//todo: System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=ClassColor; DataItem=null; target element is 'GradientStop' (HashCode=7260326); target property is 'Color' (type 'Color')
				//when opened for seconds time. why?
				dsDialog.ShowDialog();


				var selectedDeck = dsDialog.SelectedDeck;

				if(selectedDeck != null)
				{
					Logger.WriteLine("Selected deck: " + selectedDeck.Name);
					DeckPickerList.SelectDeck(selectedDeck);
					UpdateDeckList(selectedDeck);
					UseDeck(selectedDeck);
				}
				else
				{
					Logger.WriteLine("No deck selected. disabled deck detection.");
					CheckboxDeckDetection.IsChecked = false;
					SaveConfig(false);
				}
			}

			IsShowingIncorrectDeckMessage = false;
			NeedToIncorrectDeckMessage = false;
		}

		private void LoadConfig()
		{
			if(Config.Instance.TrackerWindowTop.HasValue)
				Top = Config.Instance.TrackerWindowTop.Value;
			if(Config.Instance.TrackerWindowLeft.HasValue)
				Left = Config.Instance.TrackerWindowLeft.Value;

			var titleBarCorners = new[]
				{
					new Point((int)Left + 5, (int)Top + 5),
					new Point((int)(Left + Width) - 5, (int)Top + 5),
					new Point((int)Left + 5, (int)(Top + TitlebarHeight) - 5),
					new Point((int)(Left + Width) - 5, (int)(Top + TitlebarHeight) - 5)
				};
			if(!Screen.AllScreens.Any(s => titleBarCorners.Any(c => s.WorkingArea.Contains(c))))
			{
				Top = 100;
				Left = 100;
			}

			if(Config.Instance.StartMinimized)
			{
				WindowState = WindowState.Minimized;
				if(Config.Instance.MinimizeToTray)
					MinimizeToTray();
			}

			var theme = string.IsNullOrEmpty(Config.Instance.ThemeName)
				            ? ThemeManager.DetectAppStyle().Item1
				            : ThemeManager.AppThemes.First(t => t.Name == Config.Instance.ThemeName);
			var accent = string.IsNullOrEmpty(Config.Instance.AccentName)
				             ? ThemeManager.DetectAppStyle().Item2
				             : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
			ComboboxTheme.SelectedItem = theme;
			ComboboxAccent.SelectedItem = accent;

			CheckboxSaveAppData.IsChecked = Config.Instance.SaveInAppData;

			Height = Config.Instance.WindowHeight;
			Game.HighlightCardsInHand = Config.Instance.HighlightCardsInHand;
			Game.HighlightDiscarded = Config.Instance.HighlightDiscarded;
			CheckboxHideOverlayInBackground.IsChecked = Config.Instance.HideInBackground;
			CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			CheckboxPlayerWindowOpenAutomatically.IsChecked = Config.Instance.PlayerWindowOnStart;
			CheckboxOpponentWindowOpenAutomatically.IsChecked = Config.Instance.OpponentWindowOnStart;
			CheckboxTimerTopmost.IsChecked = Config.Instance.TimerWindowTopmost;
			CheckboxTimerWindow.IsChecked = Config.Instance.TimerWindowOnStartup;
			CheckboxTimerTopmostHsForeground.IsChecked = Config.Instance.TimerWindowTopmostIfHsForeground;
			CheckboxTimerTopmostHsForeground.IsEnabled = Config.Instance.TimerWindowTopmost;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			CheckboxWinTopmostHsForeground.IsChecked = Config.Instance.WindowsTopmostIfHsForeground;
			CheckboxWinTopmostHsForeground.IsEnabled = Config.Instance.WindowsTopmost;
			CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxExportName.IsChecked = Config.Instance.ExportSetDeckName;
			CheckboxPrioGolden.IsChecked = Config.Instance.PrioritizeGolden;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			ToggleSwitchExtraFeatures.IsChecked = Config.Instance.ExtraFeatures;
			CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			CheckboxRecordArena.IsChecked = Config.Instance.RecordArena;
			CheckboxRecordCasual.IsChecked = Config.Instance.RecordCasual;
			CheckboxRecordFriendly.IsChecked = Config.Instance.RecordFriendly;
			CheckboxRecordOther.IsChecked = Config.Instance.RecordOther;
			CheckboxRecordPractice.IsChecked = Config.Instance.RecordPractice;
			CheckboxRecordRanked.IsChecked = Config.Instance.RecordRanked;
			CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			CheckboxDiscardGame.IsChecked = Config.Instance.DiscardGameIfIncorrectDeck;
			CheckboxExportPasteClipboard.IsChecked = Config.Instance.ExportPasteClipboard;
			CheckboxGoldenFeugen.IsChecked = Config.Instance.OwnsGoldenFeugen;
			CheckboxGoldenStalagg.IsChecked = Config.Instance.OwnsGoldenStalagg;
			CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			CheckboxStatsInWindow.IsChecked = Config.Instance.StatsInWindow;

			SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;

			DeckPickerList.ShowAll = Config.Instance.ShowAllDecks;
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;

			var delay = Config.Instance.DeckExportDelay;
			ComboboxExportSpeed.SelectedIndex = delay < 40 ? 0 : delay < 60 ? 1 : delay < 100 ? 2 : delay < 150 ? 3 : 4;

			SortFilterDecksFlyout.LoadTags(DeckList.AllTags);

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			var tags = new List<string>(DeckList.AllTags);
			tags.Remove("All");
			TagControlNewDeck.LoadTags(tags);
			TagControlMyDecks.LoadTags(tags);
			DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == Operation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;

			ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
				                               ? "#696969"
				                               : Config.Instance.WindowsBackgroundHex;
			UpdateAdditionalWindowsBackground();

			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";
			ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";
			ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
			CheckboxHideManaCurveNewDeck.IsChecked = Config.Instance.ManaCurveNewDeck;
			ManaCurveNewDeck.Visibility = Config.Instance.ManaCurveNewDeck ? Visibility.Visible : Visibility.Collapsed;

			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = Config.Instance.AdditionalOverlayTooltips;

			CheckboxDeckSortingClassFirst.IsChecked = Config.Instance.CardSortingClassFirst;

			DeckStatsFlyout.LoadConfig();
			GameDetailsFlyout.LoadConfig();
			StatsWindow.StatsControl.LoadConfig();
			StatsWindow.GameDetailsFlyout.LoadConfig();
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
					if(!User32.IsHearthstoneInForeground() && !hsForegroundChanged)
					{
						if(Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							PlayerWindow.Topmost = false;
							OpponentWindow.Topmost = false;
							TimerWindow.Topmost = false;
						}
						hsForegroundChanged = true;
					}
					else if(hsForegroundChanged && User32.IsHearthstoneInForeground())
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
				else
				{
					Overlay.ShowOverlay(false);
					if(Game.IsRunning)
					{
						//game was closed
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
				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_canShowDown = true;
		}

		private async void ShowNewUpdateMessage(Version newVersion = null)
		{
			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Download", NegativeButtonText = "Not now"};
			var version = newVersion ?? NewVersion;
			var newVersionString = string.Format("{0}.{1}.{2}", version.Major, version.Minor,
			                                     version.Build);
			var result =
				await
				this.ShowMessageAsync("New Update available!",
				                      "Press \"Download\" to automatically download.",
				                      MessageDialogStyle.AffirmativeAndNegative, settings);

			if(result == MessageDialogResult.Affirmative)
			{
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

		private async void ShowUpdateNotesMessage(Version current)
		{
			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Show update notes", NegativeButtonText = "Close"};

			var result = await this.ShowMessageAsync("Update successful", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Affirmative)
				Process.Start(releaseDownloadUrl);
		}

		public async void ShowMessage(string title, string message)
		{
			await this.ShowMessageAsync(title, message);
		}

		private async void ShowHsNotInstalledMessage()
		{
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Ok", NegativeButtonText = "Select manually"};
			var result =
				await
				this.ShowMessageAsync("Hearthstone install directory not found",
				                      "Hearthstone Deck Tracker will not work properly if Hearthstone is not installed on your machine (obviously).",
				                      MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Negative)
			{
				var dialog = new OpenFileDialog
					{
						Title = "Select Hearthstone.exe",
						DefaultExt = "Hearthstone.exe",
						Filter = "Hearthstone.exe|Hearthstone.exe"
					};
				var dialogResult = dialog.ShowDialog();

				if(dialogResult == true)
				{
					Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
					Config.Save();
					await Restart();
				}
			}
		}

		private async Task Restart()
		{
			await this.ShowMessageAsync("Restarting tracker", "");
			Process.Start(Application.ResourceAssembly.Location);
			Application.Current.Shutdown();
		}

		public void WriteDecks()
		{
			XmlManager<Decks>.Save(_decksPath, DeckList);
		}

		public void ActivateWindow()
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		#endregion

		#region MY DECKS - GUI

		private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
		{
			Logger.WriteLine("set player item source as drawn");
			Overlay.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			Game.IsUsingPremade = false;

			if(DeckPickerList.SelectedDeck != null)
				DeckPickerList.SelectedDeck.IsSelectedInGui = false;

			DeckPickerList.SelectedDeck = null;
			DeckPickerList.SelectedIndex = -1;
			DeckPickerList.ListboxPicker.Items.Refresh();

			UpdateDeckList(null);
			UseDeck(null);
			EnableDeckButtons(false);
			ManaCurveMyDecks.ClearDeck();
		}

		public void EnableDeckButtons(bool enable)
		{
			DeckOptionsFlyout.EnableButtons(enable);
			BtnEditDeck.IsEnabled = enable;
			BtnDeckOptions.IsEnabled = enable;
		}

		private async void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDeck;
			if(selectedDeck == null) return;

			if(_newContainsDeck)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
				var result =
					await
					this.ShowMessageAsync("Found unfinished deck", "New Deck Section still contains an unfinished deck. Discard?",
					                      MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Negative)
				{
					TabControlTracker.SelectedIndex = 1;
					return;
				}
			}

			ClearNewDeckSection();
			EditingDeck = true;
			_newContainsDeck = true;
			NewDeck = (Deck)selectedDeck.Clone();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;
			ManaCurveNewDeck.SetDeck(NewDeck);

			if(ComboBoxSelectClass.Items.Contains(NewDeck.Class))
				ComboBoxSelectClass.SelectedValue = NewDeck.Class;

			TextBoxDeckName.Text = NewDeck.Name;
			UpdateNewDeckHeader(true);
			UpdateDbListView();


			TagControlNewDeck.SetSelectedTags(NewDeck.Tags);

			TabControlTracker.SelectedIndex = 1;
		}

		private void BtnSetTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutNewDeckSetTags.IsOpen = !FlyoutNewDeckSetTags.IsOpen;
		}

		public async Task ShowSavedFileMessage(string fileName, string dir)
		{
			var settings = new MetroDialogSettings {NegativeButtonText = "Open folder"};
			var result =
				await
				this.ShowMessageAsync("", "Saved to\n\"" + fileName + "\"", MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Negative)
				Process.Start(Path.GetDirectoryName(Application.ResourceAssembly.Location) + "\\" + dir);
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

		private void BtnDeckOptions_Click(object sender, RoutedEventArgs e)
		{
			FlyoutDeckOptions.IsOpen = true;
		}

		#endregion

		#region MY DECKS - METHODS

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
			ListViewDeck.ItemsSource = selected.Cards;

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			Config.Instance.LastDeck = selected.Name;
			Config.Save();
		}

		#endregion

		#region NEW DECK GUI

		private void ComboBoxFilterClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			NewDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newContainsDeck = true;
			UpdateDbListView();

			ManaCurveNewDeck.UpdateValues();
		}

		private async void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
		{
			NewDeck.Cards =
				new ObservableCollection<Card>(
					NewDeck.Cards.OrderBy(c => c.Cost).ThenByDescending(c => c.Type).ThenBy(c => c.Name).ToList());
			ListViewNewDeck.ItemsSource = NewDeck.Cards;

			var deckName = TextBoxDeckName.Text;
			if(EditingDeck)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Overwrite", NegativeButtonText = "Save as new"};
				var result =
					await
					this.ShowMessageAsync("Saving deck", "How do you wish to save the deck?", MessageDialogStyle.AffirmativeAndNegative,
					                      settings);
				if(result == MessageDialogResult.Affirmative)
					SaveDeck(true);
				else if(result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else if(DeckList.DecksList.Any(d => d.Name == deckName))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Overwrite", NegativeButtonText = "Set new name"};
				var result =
					await
					this.ShowMessageAsync("A deck with that name already exists", "Overwriting the deck can not be undone!",
					                      MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Affirmative)
				{
					Deck oldDeck;
					while((oldDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == deckName)) != null)
					{
						var deckStats = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.Name == oldDeck.Name);
						if(deckStats != null)
						{
							foreach(var game in deckStats.Games)
								game.DeleteGameFile();
							DeckStatsList.Instance.DeckStats.Remove(deckStats);
							DeckStatsList.Save();
							Logger.WriteLine("Deleted deckstats for deck: " + oldDeck.Name);
						}
						DeckList.DecksList.Remove(oldDeck);
						DeckPickerList.RemoveDeck(oldDeck);
					}

					SaveDeck(true);
				}
				else if(result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else
				SaveDeck(false);

			FlyoutNewDeckSetTags.IsOpen = false;
		}

		private void ComboBoxFilterMana_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			UpdateDbListView();
		}

		private void ComboboxNeutral_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			UpdateDbListView();
		}

		private void TextBoxDBFilter_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var index = ListViewDB.SelectedIndex;
			Card card = null;
			switch(e.Key)
			{
				case Key.Enter:
					if(ListViewDB.SelectedItem != null)
						card = (Card)ListViewDB.SelectedItem;
					else if(ListViewDB.Items.Count > 0)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D1:
					if(ListViewDB.Items.Count > 0)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D2:
					if(ListViewDB.Items.Count > 1)
						card = (Card)ListViewDB.Items[1];
					break;
				case Key.D3:
					if(ListViewDB.Items.Count > 2)
						card = (Card)ListViewDB.Items[2];
					break;
				case Key.D4:
					if(ListViewDB.Items.Count > 3)
						card = (Card)ListViewDB.Items[3];
					break;
				case Key.D5:
					if(ListViewDB.Items.Count > 4)
						card = (Card)ListViewDB.Items[4];
					break;
				case Key.Down:
					if(index < ListViewDB.Items.Count - 1)
						ListViewDB.SelectedIndex += 1;
					break;
				case Key.Up:
					if(index > 0)
						ListViewDB.SelectedIndex -= 1;
					break;
			}
			if(card != null)
			{
				AddCardToDeck((Card)card.Clone());
				e.Handled = true;
			}
		}

		private void BtnImport_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutDeckImport.IsOpen = true;
			DeckImportFlyout.BtnLastGame.IsEnabled = Game.DrawnLastGame != null;
		}

		private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if(card == null) return;
				AddCardToDeck((Card)card.Clone());
				_newContainsDeck = true;
			}
		}

		private void ListViewNewDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				RemoveCardFromDeck(card);
			}
		}

		private void ListViewNewDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if(string.IsNullOrEmpty(card.Name)) return;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDbListView();
		}

		private void BtnClear_Click(object sender, RoutedEventArgs e)
		{
			ShowClearNewDeckMessage();
		}

		#endregion

		#region NEW DECK METHODS

		private void UpdateDbListView()
		{
			var selectedClass = ComboBoxSelectClass.SelectedValue.ToString();
			var selectedNeutral = ComboboxNeutral.SelectedValue.ToString();
			if(selectedClass == "Select a Class")
				ListViewDB.Items.Clear();
			else
			{
				ListViewDB.Items.Clear();

				var formattedInput = Helper.RemoveDiacritics(TextBoxDBFilter.Text.ToLowerInvariant(), true);
				var words = formattedInput.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				foreach(var card in Game.GetActualCards())
				{
					var cardName = Helper.RemoveDiacritics(card.LocalizedName.ToLowerInvariant(), true);
					if(!Config.Instance.UseFullTextSearch && !cardName.Contains(formattedInput))
						continue;
					if(Config.Instance.UseFullTextSearch && words.Any(w => !cardName.Contains(w)
					                                                       && !(!string.IsNullOrEmpty(card.Text) && card.Text.ToLowerInvariant().Contains(w))
					                                                       && (!string.IsNullOrEmpty(card.RaceOrType) && w != card.RaceOrType.ToLowerInvariant())
					                                                       && (!string.IsNullOrEmpty(card.Rarity) && w != card.Rarity.ToLowerInvariant())))
						continue;

					// mana filter
					if(ComboBoxFilterMana.SelectedItem.ToString() == "All"
					   || ((ComboBoxFilterMana.SelectedItem.ToString() == "9+" && card.Cost >= 9)
					       || (ComboBoxFilterMana.SelectedItem.ToString() == card.Cost.ToString())))
					{
						switch(selectedNeutral)
						{
							case "Class + Neutral":
								if(card.GetPlayerClass == selectedClass || card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
							case "Class Only":
								if(card.GetPlayerClass == selectedClass)
									ListViewDB.Items.Add(card);
								break;
							case "Neutral Only":
								if(card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
						}
					}
				}

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private async void SaveDeck(bool overwrite)
		{
			var deckName = TextBoxDeckName.Text;

			if(string.IsNullOrEmpty(deckName))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = deckName};

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if(String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			while(DeckList.DecksList.Any(d => d.Name == deckName) && (!EditingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = deckName};
				var name =
					await
					this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if(String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if(NewDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};

				var result =
					await
					this.ShowMessageAsync("Not 30 cards",
					                      string.Format("Deck contains {0} cards. Is this what you want to save anyway?",
					                                    NewDeck.Cards.Sum(c => c.Count)),
					                      MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result != MessageDialogResult.Affirmative)
					return;
			}

			if(EditingDeck && overwrite)
			{
				DeckList.DecksList.Remove(NewDeck);
				DeckPickerList.RemoveDeck(NewDeck);
			}

			var oldDeckName = NewDeck.Name;

			NewDeck.Name = deckName;
			NewDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			NewDeck.Tags = TagControlNewDeck.GetTags();

			var newDeckClone = (Deck)NewDeck.Clone();
			DeckList.DecksList.Add(newDeckClone);

			newDeckClone.LastEdited = DateTime.Now;

			WriteDecks();
			Logger.WriteLine("Saved Decks");
			BtnSaveDeck.Content = "Save";

			if(EditingDeck)
			{
				TagControlNewDeck.SetSelectedTags(new List<string>());
				if(deckName != oldDeckName)
				{
					var statsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(d => d.Name == oldDeckName);
					if(statsEntry != null)
					{
						if(overwrite)
						{
							statsEntry.Name = deckName;
							Logger.WriteLine("Deck has new name, updated deckstats");
						}
						else
						{
							var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(d => d.Name == deckName);
							if(newStatsEntry == null)
							{
								newStatsEntry = new DeckStats(deckName);
								DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
							}
							foreach(var game in statsEntry.Games)
								newStatsEntry.AddGameResult(game.CloneWithNewId());
							Logger.WriteLine("cloned gamestats for \"Set as new\"");
						}
						DeckStatsList.Save();
					}
				}
			}

			//after cloning the stats, otherwise new stats will be generated
			DeckPickerList.AddAndSelectDeck(newDeckClone);

			TabControlTracker.SelectedIndex = 0;
			EditingDeck = false;

			foreach(var tag in NewDeck.Tags)
				SortFilterDecksFlyout.AddSelectedTag(tag);

			DeckPickerList.UpdateList();
			DeckPickerList.SelectDeck(newDeckClone);

			ClearNewDeckSection();
		}

		private async void ClearNewDeckSection()
		{
			UpdateNewDeckHeader(false);
			ComboBoxSelectClass.SelectedIndex = 0;
			TextBoxDeckName.Text = string.Empty;
			TextBoxDBFilter.Text = string.Empty;
			ComboBoxFilterMana.SelectedIndex = 0;
			NewDeck = new Deck();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;
			ManaCurveNewDeck.SetDeck(NewDeck);
			_newContainsDeck = false;
			EditingDeck = false;

			//wait for animations to finish
			await Task.Delay(1000);
			ManaCurveNewDeck.UpdateValues();
		}

		private void RemoveCardFromDeck(Card card)
		{
			if(card == null)
				return;
			if(card.Count > 1)
			{
				card.Count--;
				ManaCurveNewDeck.UpdateValues();
			}
			else
				NewDeck.Cards.Remove(card);

			ManaCurveNewDeck.SetDeck(NewDeck);
			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		private void UpdateNewDeckHeader(bool show)
		{
			const string headerText = "New Deck";
			var cardCount = NewDeck.Cards.Sum(c => c.Count);
			TabItemNewDeck.Header = show ? string.Format("{0} ({1})", headerText, cardCount) : headerText;
			UpdateTabMarker();
		}

		private void AddCardToDeck(Card card)
		{
			if(card == null)
				return;
			if(NewDeck.Cards.Contains(card))
			{
				var cardInDeck = NewDeck.Cards.First(c => c.Name == card.Name);
				cardInDeck.Count++;
				ManaCurveNewDeck.UpdateValues();
			}
			else
				NewDeck.Cards.Add(card);

			ManaCurveNewDeck.SetDeck(NewDeck);
			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
			try
			{
				TextBoxDBFilter.Focus();
				TextBoxDBFilter.Select(0, TextBoxDBFilter.Text.Length);
			}
			catch
			{
			}
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if(deck != null)
			{
				ClearNewDeckSection();
				_newContainsDeck = true;
				EditingDeck = editing;

				NewDeck = (Deck)deck.Clone();
				ListViewNewDeck.ItemsSource = NewDeck.Cards;
				Helper.SortCardCollection(ListViewNewDeck.ItemsSource, false);

				ManaCurveNewDeck.SetDeck(NewDeck);

				if(ComboBoxSelectClass.Items.Contains(NewDeck.Class))
					ComboBoxSelectClass.SelectedValue = NewDeck.Class;

				TextBoxDeckName.Text = NewDeck.Name;
				UpdateNewDeckHeader(true);
				UpdateDbListView();
			}
		}

		private async void ShowClearNewDeckMessage()
		{
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
			var result = await this.ShowMessageAsync("Clear deck?", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Affirmative)
			{
				ClearNewDeckSection();
				UpdateTabMarker();
			}
		}

		#endregion

		#region OPTIONS

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightCardsInHand = true;
			Game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightCardsInHand = false;
			Game.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInMenu = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInMenu = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmost = true;
			PlayerWindow.Topmost = true;
			OpponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmost = false;
			PlayerWindow.Topmost = false;
			OpponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			PlayerWindow.Topmost = false;
			OpponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if(Config.Instance.WindowsTopmost)
			{
				PlayerWindow.Topmost = true;
				OpponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmost = true;
			TimerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmost = false;
			TimerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			TimerWindow.Show();
			TimerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			TimerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			TimerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if(Config.Instance.TimerWindowTopmost)
				TimerWindow.Topmost = true;
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Overlay.Update(true);
		}


		private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayOpacity = SliderOverlayOpacity.Value;
			SaveConfig(true);
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.KeepDecksVisible = true;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.KeepDecksVisible = false;
			SaveConfig(true);
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.MinimizeToTray = true;
			SaveConfig(false);
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseSameScaling = true;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseSameScaling = false;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoDeckDetection = true;
			CheckboxAutoSelectDeck.IsEnabled = true;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoDeckDetection = false;
			CheckboxAutoSelectDeck.IsChecked = false;
			CheckboxAutoSelectDeck.IsEnabled = false;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = true;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = false;
			SaveConfig(false);
		}

		private void SliderOverlayPlayerScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			var scaling = SliderOverlayPlayerScaling.Value;
			Config.Instance.OverlayPlayerScaling = scaling;
			SaveConfig(false);
			Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && SliderOverlayOpponentScaling.Value != scaling)
				SliderOverlayOpponentScaling.Value = scaling;
		}

		private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized) return;
			var scaling = SliderOverlayOpponentScaling.Value;
			Config.Instance.OverlayOpponentScaling = scaling;
			SaveConfig(false);
			Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
				SliderOverlayPlayerScaling.Value = scaling;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		private void CheckboxHideTimers_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideTimers = true;
			SaveConfig(true);
		}

		private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideTimers = false;
			SaveConfig(true);
		}

		private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			var accent = ComboboxAccent.SelectedItem as Accent;
			if(accent != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.DetectAppStyle().Item1);
				Config.Instance.AccentName = accent.Name;
				SaveConfig(false);
			}
		}

		private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if(theme != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
				Config.Instance.ThemeName = theme.Name;
				//if(ComboboxWindowBackground.SelectedItem.ToString() != "Default")
				UpdateAdditionalWindowsBackground();
				SaveConfig(false);
			}
		}

		private void ComboboxWindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
			Config.Instance.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
			UpdateAdditionalWindowsBackground();
		}

		private void UpdateAdditionalWindowsBackground(Brush brush = null)
		{
			var background = brush;

			switch(ComboboxWindowBackground.SelectedItem.ToString())
			{
				case "Theme":
					background = Background;
					break;
				case "Light":
					background = SystemColors.ControlLightBrush;
					break;
				case "Dark":
					background = SystemColors.ControlDarkDarkBrush;
					break;
			}
			if(background == null)
			{
				var hexBackground = BackgroundFromHex();
				if(hexBackground != null)
				{
					PlayerWindow.Background = hexBackground;
					OpponentWindow.Background = hexBackground;
					TimerWindow.Background = hexBackground;
				}
			}
			else
			{
				PlayerWindow.Background = background;
				OpponentWindow.Background = background;
				TimerWindow.Background = background;
			}
		}

		private SolidColorBrush BackgroundFromHex()
		{
			SolidColorBrush brush = null;
			var hex = TextboxCustomBackground.Text;
			if(hex.StartsWith("#")) hex = hex.Remove(0, 1);
			if(!string.IsNullOrEmpty(hex) && hex.Length == 6 && Helper.IsHex(hex))
			{
				var color = ColorTranslator.FromHtml("#" + hex);
				brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
			}
			return brush;
		}

		private void TextboxCustomBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || ComboboxWindowBackground.SelectedItem.ToString() != "Custom") return;
			var background = BackgroundFromHex();
			if(background != null)
			{
				UpdateAdditionalWindowsBackground(background);
				Config.Instance.WindowsBackgroundHex = TextboxCustomBackground.Text;
				SaveConfig(false);
			}
		}

		private async void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			var language = ComboboxLanguages.SelectedValue.ToString();
			if(!Helper.LanguageDict.ContainsKey(language))
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			if(!File.Exists(string.Format("Files/cardsDB.{0}.json", selectedLanguage)))
				return;

			Config.Instance.SelectedLanguage = selectedLanguage;
			Config.Save();


			await Restart();
		}

		private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = true;
			SaveConfig(false);
		}

		private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExportSetDeckName = false;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = true;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.PrioritizeGolden = false;
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeyPressOnGameStart = ComboboxKeyPressGameStart.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeyPressOnGameEnd = ComboboxKeyPressGameEnd.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = false;
			SaveConfig(true);
		}

		private async void CheckboxAppData_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			await Restart();
		}

		private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			await Restart();
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveMyDecks = true;
			ManaCurveMyDecks.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveMyDecks = false;
			ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveNewDeck = true;
			ManaCurveNewDeck.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ManaCurveNewDeck = false;
			ManaCurveNewDeck.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized) return;
			Config.Instance.TrackerCardToolTips = true;
			SaveConfig(false);
			await Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized) return;
			Config.Instance.TrackerCardToolTips = false;
			SaveConfig(false);
			await Restart();
		}

		private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowCardToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.WindowCardToolTips = false;
			SaveConfig(false);
		}

		private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayCardToolTips = true;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OverlayCardToolTips = false;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = false;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = false;
			SaveConfig(true);
		}

		private void CheckboxDeckSortingClassFirst_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CardSortingClassFirst = true;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, true);
			Helper.SortCardCollection(ListViewNewDeck.Items, true);
		}

		private void CheckboxDeckSortingClassFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CardSortingClassFirst = false;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
			Helper.SortCardCollection(ListViewNewDeck.Items, false);
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.BringHsToForeground = true;
			SaveConfig(false);
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.BringHsToForeground = false;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = false;
			SaveConfig(false);
		}

		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			Overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if(!Game.IsInMenu)
				Overlay.ShowSecrets(Game.PlayingAgainst);
		}

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightDiscarded = true;
			Game.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightDiscarded = false;
			Game.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private async void BtnUnlockOverlay_Click(object sender, RoutedEventArgs e)
		{
			if(User32.GetHearthstoneWindow() == IntPtr.Zero) return;
			BtnUnlockOverlay.Content = await Overlay.UnlockUI() ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Resetting overlay to default",
				                      "Positions of: Player Deck, Opponent deck, Timers and Secrets will be reset to default. Are you sure?",
				                      MessageDialogStyle.AffirmativeAndNegative);
			if(result != MessageDialogResult.Affirmative)
				return;

			if((string)BtnUnlockOverlay.Content == "Lock")
			{
				await Overlay.UnlockUI();
				BtnUnlockOverlay.Content = "Unlock";
			}

			Config.Instance.PlayerDeckTop = Config.Defaults.PlayerDeckTop;
			Config.Instance.PlayerDeckLeft = Config.Defaults.PlayerDeckLeft;
			Config.Instance.PlayerDeckHeight = Config.Defaults.PlayerDeckHeight;

			Config.Instance.OpponentDeckTop = Config.Defaults.OpponentDeckTop;
			Config.Instance.OpponentDeckLeft = Config.Defaults.OpponentDeckLeft;
			Config.Instance.OpponentDeckHeight = Config.Defaults.OpponentDeckHeight;

			Config.Instance.TimersHorizontalPosition = Config.Defaults.TimersHorizontalPosition;
			Config.Instance.TimersHorizontalSpacing = Config.Defaults.TimersHorizontalSpacing;

			Config.Instance.TimersHorizontalSpacing = Config.Defaults.TimersHorizontalSpacing;
			Config.Instance.TimersVerticalSpacing = Config.Defaults.TimersVerticalSpacing;

			Config.Instance.SecretsTop = Config.Defaults.SecretsTop;
			Config.Instance.SecretsLeft = Config.Defaults.SecretsLeft;

			SaveConfig(true);
		}

		private void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			Game.Reset();
			if(DeckPickerList.SelectedDeck != null)
				Game.SetPremadeDeck((Deck)DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Overlay.Update(true);
		}

		private void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			Game.Reset();
			if(DeckPickerList.SelectedDeck != null)
				Game.SetPremadeDeck((Deck)DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Overlay.Update(true);
		}

		private void CheckboxHighlightLastDrawn_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightLastDrawn = true;
			SaveConfig(false);
		}

		private void CheckboxHighlightLastDrawn_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.HighlightLastDrawn = false;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StartMinimized = true;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StartMinimized = false;
			SaveConfig(false);
		}

		private void CheckboxShowPlayerGet_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ShowPlayerGet = true;
			Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ShowPlayerGet = false;
			Overlay.Update(true);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = true;
			SaveConfig(false);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = false;
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExtraFeatures = true;
			Overlay.HookMouse();
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExtraFeatures = false;
			Overlay.UnHookMouse();
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CheckForUpdates = true;
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CheckForUpdates = false;
			SaveConfig(false);
		}

		private void CheckboxRecordRanked_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordRanked = true;
			SaveConfig(false);
		}

		private void CheckboxRecordRanked_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordRanked = false;
			SaveConfig(false);
		}

		private void CheckboxRecordArena_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordArena = true;
			SaveConfig(false);
		}

		private void CheckboxRecordArena_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordArena = false;
			SaveConfig(false);
		}

		private void CheckboxRecordCasual_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordCasual = true;
			SaveConfig(false);
		}

		private void CheckboxRecordCasual_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordCasual = false;
			SaveConfig(false);
		}

		private void CheckboxRecordFriendly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordFriendly = true;
			SaveConfig(false);
		}

		private void CheckboxRecordFriendly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordFriendly = false;
			SaveConfig(false);
		}

		private void CheckboxRecordPractice_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordPractice = true;
			SaveConfig(false);
		}

		private void CheckboxRecordPractice_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordPractice = false;
			SaveConfig(false);
		}

		private void CheckboxRecordOther_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordOther = true;
			SaveConfig(false);
		}

		private void CheckboxRecordOther_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.RecordOther = false;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseFullTextSearch = true;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.UseFullTextSearch = false;
			SaveConfig(false);
		}

		private void CheckboxDiscardGame_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.DiscardGameIfIncorrectDeck = true;
			SaveConfig(false);
		}

		private void CheckboxDiscardGame_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.DiscardGameIfIncorrectDeck = false;
			SaveConfig(false);
		}

		private void ComboboxExportSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			var selected = ComboboxExportSpeed.SelectedValue.ToString();

			switch(selected)
			{
				case "Very Fast (20ms)":
					Config.Instance.DeckExportDelay = 20;
					break;
				case "Fast (40ms)":
					Config.Instance.DeckExportDelay = 40;
					break;
				case "Normal (60ms)":
					Config.Instance.DeckExportDelay = 60;
					break;
				case "Slow (100ms)":
					Config.Instance.DeckExportDelay = 100;
					break;
				case "Very Slow (150ms)":
					Config.Instance.DeckExportDelay = 150;
					break;
				default:
					return;
			}
			SaveConfig(false);
		}

		private void CheckboxExportPasteClipboard_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExportPasteClipboard = true;
			SaveConfig(false);
		}

		private void CheckboxExportPasteClipboard_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.ExportPasteClipboard = false;
			SaveConfig(false);
		}

		private void CheckboxGoldenFeugen_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenFeugen = true;
			SaveConfig(false);
		}

		private void CheckboxGoldenFeugen_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenFeugen = false;
			SaveConfig(false);
		}

		private void CheckboxGoldenStalagg_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenStalagg = true;
			SaveConfig(false);
		}

		private void CheckboxGoldenStalagg_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.OwnsGoldenStalagg = false;
			SaveConfig(false);
		}

		private void CheckboxCloseWithHearthstone_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CloseWithHearthstone = true;
			Config.Save();
		}

		private void CheckboxCloseWithHearthstone_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.CloseWithHearthstone = false;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StatsInWindow = true;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.StatsInWindow = false;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			PlayerWindow.Show();
			PlayerWindow.Activate();
			PlayerWindow.SetCardCount(Game.PlayerHandCount, 30 - Game.PlayerDrawn.Where(c => !c.IsStolen).Sum(card => card.Count));
			Config.Instance.PlayerWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxPlayerWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			PlayerWindow.Hide();
			Config.Instance.PlayerWindowOnStart = false;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			OpponentWindow.Show();
			OpponentWindow.Activate();
			OpponentWindow.SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount, Game.OpponentHasCoin);
			Config.Instance.OpponentWindowOnStart = true;
			Config.Save();
		}

		private void CheckboxOpponentWindowOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			OpponentWindow.Hide();
			Config.Instance.OpponentWindowOnStart = false;
			Config.Save();
		}

		#endregion
	}
}