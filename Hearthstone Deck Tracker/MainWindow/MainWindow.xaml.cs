using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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
		//private readonly string _configPath;
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
			new ReadOnlyCollection<string>(new[] { "None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });

		public bool IsShowingIncorrectDeckMessage;
		public bool NeedToIncorrectDeckMessage;
		private bool _canShowDown;
		private bool _doUpdate;
		private DateTime _lastUpdateCheck;
		private Deck _newDeck;
		private bool _newDeckUnsavedChanges;
		private bool _tempUpdateCheckDisabled;
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
			Game.Reset();

			_decksPath = Config.Instance.DataDir + "PlayerDecks.xml";
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

			_notifyIcon = new NotifyIcon { Icon = new Icon(@"Images/HearthstoneDeckTracker16.ico"), Visible = true, ContextMenu = new ContextMenu(), Text = "Hearthstone Deck Tracker v" + versionString };
			_notifyIcon.ContextMenu.MenuItems.Add("Show", (sender, args) => ActivateWindow());
			_notifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, args) => Close());
			_notifyIcon.MouseClick += (sender, args) => { if(args.Button == MouseButtons.Left) ActivateWindow(); };

			//create overlay
			Overlay = new OverlayWindow { Topmost = true };

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

			Options.ComboboxAccent.ItemsSource = ThemeManager.Accents;
			Options.ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			Options.ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			Options.ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			Options.ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

			LoadConfig();

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
			UpdateOverlayAsync();

			_initialized = true;
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
			var settings = new MetroDialogSettings { AffirmativeButtonText = "Download", NegativeButtonText = "Not now" };
			var version = newVersion ?? NewVersion;
			if(version == null) return;
			try
			{
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
			DeselectDeck();
		}

		private void DeselectDeck()
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
			if(!_initialized) return;
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

				//change player deck itemsource
				if(Overlay.ListViewPlayer.ItemsSource != Game.PlayerDeck)
				{
					Overlay.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDeck;
					Logger.WriteLine("Set player itemsource as playerdeck");
				}
				Game.IsUsingPremade = true;
				UpdateDeckList(deck);
				UseDeck(deck);
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
				DeckList.LastDeckClass.Add(new DeckInfo { Class = deck.Class, Name = deck.Name });
				WriteDecks();
				EnableMenuItems(true);
				ManaCurveMyDecks.SetDeck(deck);
				TagControlEdit.SetSelectedTags(deck.Tags);
			}
			else
				EnableMenuItems(false);
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
			ListViewDeck.ItemsSource = selected.Cards;

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			Config.Instance.LastDeck = selected.Name;
			Config.Save();
		}


		private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoDeckDetection = true;
			Config.Save();
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.AutoDeckDetection = false;
			Config.Save();
		}
	}
}