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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
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

		private const bool IS_DEBUG = false;

		//public readonly Config _config;
		public readonly Decks _deckList;
		public readonly Game _game;
		private readonly bool _initialized;

		private readonly string _logConfigPath =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
			@"\Blizzard\Hearthstone\log.config";

		private readonly string _decksPath;
		private readonly string _configPath;

		private readonly HsLogReader _logReader;
		private readonly NotifyIcon _notifyIcon;
		private readonly OpponentWindow _opponentWindow;
		private readonly OverlayWindow _overlay;
		private readonly PlayerWindow _playerWindow;
		private readonly TimerWindow _timerWindow;
		//private readonly XmlManager<Decks> _xmlManager;
		//private readonly XmlManager<Config> _xmlManagerConfig;
		//public readonly XmlManager<Deck> _xmlManagerDeck;
		//internal readonly DeckImporter _deckImporter;
		//internal readonly DeckExporter _deckExporter;
		public bool _editingDeck;
		private bool _newContainsDeck;
		public Deck _newDeck;
		private bool _doUpdate;
		private bool _showingIncorrectDeckMessage;
		private bool _showIncorrectDeckMessage;
		private readonly Version _newVersion;
		//private readonly TurnTimer _turnTimer;


		private readonly bool _updatedLogConfig;


		private readonly bool _foundHsDirectory;
		//private const string EventKeys = "None,F1,F2,F3,F4,F5,F6,F7,F8,F9,F10,F11,F12";
		private ReadOnlyCollection<string> EventKeys = new ReadOnlyCollection<string>(new[] { "None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		#endregion

		public MainWindow()
		{
			InitializeComponent();

			#region Aaron Campf
			DeckOptionsFlyout.Window = this;
			DeckImportFlyout.Window = this;
			TagControlMyDecks.Window = this;
			TagControlNewDeck.Window = this;


			//_xmlManagerConfig = new XmlManager<Config> { Type = typeof(Config) };
			_configPath = Config.Load();

			#endregion

			var version = Helper.CheckForUpdates(out _newVersion);
			if (version != null)
			{
				TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", version.Major, version.Minor, version.Build);
			}

			if (Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

			Config.Instance.Debug = IS_DEBUG;

			if (Config.Instance.GenerateLog)
			{
				Directory.CreateDirectory("Logs");
				var listener = new TextWriterTraceListener(Config.Instance.LogFilePath);
				Trace.Listeners.Add(listener);
				Trace.AutoFlush = true;
			}

			#region find hearthstone dir
			if (string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory) || !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using (var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone"))
				{
					if (hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the installlocation actually is correct (possibly moved?)
						if (File.Exists(hsDir + @"\Hearthstone.exe"))
						{
							Config.Instance.HearthstoneDirectory = hsDir;
							WriteConfig();
							_foundHsDirectory = true;
						}
					}
				}
			}
			else
			{
				_foundHsDirectory = true;
			}

			if (_foundHsDirectory)
			{
				//check for log config and create if not existing
				try
				{
					//always overwrite is true by default. 
					if (!File.Exists(_logConfigPath))
					{
						_updatedLogConfig = true;
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (did not exist)", _configPath));
					}
					else
					{
						//update log.config if newer
						var localFile = new FileInfo(_logConfigPath);
						var file = new FileInfo("Files/log.config");
						if (file.LastWriteTime > localFile.LastWriteTime)
						{
							_updatedLogConfig = true;
							File.Copy("Files/log.config", _logConfigPath, true);
							Logger.WriteLine(string.Format("Copied log.config to {0} (file newer)", _configPath));
						}
						else if (Config.Instance.AlwaysOverwriteLogConfig)
						{
							File.Copy("Files/log.config", _logConfigPath, true);
							Logger.WriteLine(string.Format("Copied log.config to {0} (AlwaysOverwriteLogConfig)", _configPath));
						}
					}
				}
				catch (Exception e)
				{
					if (_updatedLogConfig)
					{
						MessageBox.Show(
							e.Message + "\n\n" + e.InnerException +
							"\n\n Please manually copy the log.config from the Files directory to \"%LocalAppData%/Blizzard/Hearthstone\".",
							"Error writing log.config");
						Application.Current.Shutdown();
					}
				}
			}
			#endregion

			string languageTag = Config.Instance.SelectedLanguage;
			//hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
			_game = Helper.LanguageDict.ContainsValue(languageTag) ? new Game(languageTag) : new Game("enUS");
			_game.Reset();

			#region playerdecks
			_decksPath = Config.Instance.HomeDir + "PlayerDecks.xml";

			if (Config.Instance.SaveInAppData)
			{
				if (File.Exists("PlayerDecks.xml"))
				{
					if (File.Exists(_decksPath))
					{
						//backup in case the file already exists
						File.Move(_decksPath, _decksPath + DateTime.Now.ToFileTime());
					}
					File.Move("PlayerDecks.xml", _decksPath);
					Logger.WriteLine("Moved decks to appdata");
				}
			}
			else
			{
				var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
				if (File.Exists(appDataPath))
				{
					if (File.Exists(_decksPath))
					{
						//backup in case the file already exists
						File.Move(_decksPath, _decksPath + DateTime.Now.ToFileTime());
					}
					File.Move(appDataPath, _decksPath);
					Logger.WriteLine("Moved decks to local");
				}
			}

			//load saved decks
			if (!File.Exists(_decksPath))
			{
				//avoid overwriting decks file with new releases.
				using (var sr = new StreamWriter(_decksPath, false))
				{
					sr.WriteLine("<Decks></Decks>");
				}
			}
			else
			{
				//the new playerdecks.xml wont work with versions below 0.2.19, make copy
				if (!File.Exists(_decksPath + ".old"))
				{
					File.Copy(_decksPath, _decksPath + ".old");
				}
			}

			//_xmlManager = new XmlManager<Decks> { Type = typeof(Decks) };
			try
			{
				_deckList = XmlManager<Decks>.Load(_decksPath);
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + _decksPath + " (this will cause you to lose your decks).",
					"Error loading PlayerDecks.xml");
				Application.Current.Shutdown();
			}
			#endregion

			foreach (var deck in _deckList.DecksList)
			{
				DeckPickerList.AddDeck(deck);
			}
			DeckPickerList.SelectedDeckChanged += DeckPickerListOnSelectedDeckChanged;

			_notifyIcon = new System.Windows.Forms.NotifyIcon();
			_notifyIcon.Icon = new Icon(@"Images/HearthstoneDeckTracker.ico");
			_notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
			_notifyIcon.Visible = false;

			//_xmlManagerDeck = new XmlManager<Deck>();
			//_xmlManagerDeck.Type = typeof(Deck);

			_newDeck = new Deck();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;


			//create overlay
			_overlay = new OverlayWindow(Config.Instance, _game) { Topmost = true };
			if (_foundHsDirectory)
			{
				_overlay.Show();
			}
			_playerWindow = new PlayerWindow(Config.Instance, _game.IsUsingPremade ? _game.PlayerDeck : _game.PlayerDrawn);
			_opponentWindow = new OpponentWindow(Config.Instance, _game.OpponentCards);
			_timerWindow = new TimerWindow(Config.Instance);

			if (Config.Instance.WindowsOnStartup)
			{
				_playerWindow.Show();
				_opponentWindow.Show();
			}
			if (Config.Instance.TimerWindowOnStartup)
			{
				_timerWindow.Show();
			}
			if (!_deckList.AllTags.Contains("All"))
			{
				_deckList.AllTags.Add("All");
				WriteDecks();
			}
			if (!_deckList.AllTags.Contains("Arena"))
			{
				_deckList.AllTags.Add("Arena");
				WriteDecks();
			}
			if (!_deckList.AllTags.Contains("Constructed"))
			{
				_deckList.AllTags.Add("Constructed");
				WriteDecks();
			}

			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

			LoadConfig();

			DeckImporter._game = _game;
			//_deckImporter = new DeckImporter(_game);
			//_deckExporter = new DeckExporter(Config.Instance);

			//this has to happen before reader starts
			var lastDeck = _deckList.DecksList.FirstOrDefault(d => d.Name == Config.Instance.LastDeck);
			DeckPickerList.SelectDeck(lastDeck);

			//deck options flyout button events
			//DeckOptionsFlyout.BtnExportHs.Click += DeckOptionsFlyoutBtnExportHs_Click;
			//DeckOptionsFlyout.BtnNotes.Click += DeckOptionsFlyoutBtnNotes_Click;
			//DeckOptionsFlyout.BtnScreenshot.Click += DeckOptionsFlyoutBtnScreenhot_Click;
			//DeckOptionsFlyout.BtnCloneDeck.Click += DeckOptionsFlyoutCloneDeck_Click;
			//DeckOptionsFlyout.BtnTags.Click += DeckOptionsFlyoutBtnTags_Click;
			//DeckOptionsFlyout.BtnSaveToFile.Click += DeckOptionsFlyoutBtnSaveToFile_Click;
			//DeckOptionsFlyout.BtnClipboard.Click += DeckOptionsFlyoutBtnClipboard_Click;

			DeckOptionsFlyout.DeckOptionsButtonClicked += (DeckOptions sender) => { FlyoutDeckOptions.IsOpen = false; };

			//deck import flyout button events
			//DeckImportFlyout.BtnWeb.Click += DeckImportFlyoutBtnWebClick;
			//DeckImportFlyout.BtnArenavalue.Click += DeckImportFlyoutBtnArenavalue_Click;
			//DeckImportFlyout.BtnFile.Click += DeckImportFlyoutBtnFile_Click;
			//DeckImportFlyout.BtnIdString.Click += DeckImportFlyoutBtnIdString_Click;

			DeckImportFlyout.DeckOptionsButtonClicked += (DeckImport sender) => { FlyoutDeckImport.IsOpen = false; };

			//log reader
			_logReader = new HsLogReader(Config.Instance.HearthstoneDirectory, Config.Instance.UpdateDelay);
			_logReader.CardMovement += LogReaderOnCardMovement;
			_logReader.GameStateChange += LogReaderOnGameStateChange;
			_logReader.Analyzing += LogReaderOnAnalyzing;
			_logReader.TurnStart += LogReaderOnTurnStart;
			_logReader.CardPosChange += LogReaderOnCardPosChange;
			_logReader.SecretPlayed += LogReaderOnSecretPlayed;

			//_turnTimer = new TurnTimer(90);
			//_turnTimer.TimerTick += TurnTimerOnTimerTick;
			TurnTimer.Create(90);
			TurnTimer.Instance.TimerTick += TurnTimerOnTimerTick;

			TagControlFilter.HideStuffToCreateNewTag();
			TagControlNewDeck.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlMyDecks.OperationSwitch.Visibility = Visibility.Collapsed;

			//TagControlNewDeck.NewTag += TagControlOnNewTag;
			//TagControlNewDeck.SelectedTagsChanged += TagControlOnSelectedTagsChanged;
			//TagControlNewDeck.DeleteTag += TagControlOnDeleteTag;

			//TagControlMyDecks.NewTag += TagControlOnNewTag;
			//TagControlMyDecks.SelectedTagsChanged += TagControlOnSelectedTagsChanged;
			//TagControlMyDecks.DeleteTag += TagControlOnDeleteTag;
			TagControlFilter.SelectedTagsChanged += TagControlFilterOnSelectedTagsChanged;
			TagControlFilter.OperationChanged += TagControlFilterOnOperationChanged;


			UpdateDbListView();

			_doUpdate = _foundHsDirectory;
			UpdateOverlayAsync();

			_initialized = true;

			DeckPickerList.UpdateList();
			if (lastDeck != null)
			{
				DeckPickerList.SelectDeck(lastDeck);
				UpdateDeckList(lastDeck);
				UseDeck(lastDeck);
			}

			if (_foundHsDirectory)
			{
				_logReader.Start();
			}

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);

		}

		private void LogReaderOnSecretPlayed(HsLogReader sender)
		{
			_game.OpponentSecretCount++;
			_overlay.ShowSecrets(_game.PlayingAgainst);
		}

		#region LogReader Events

		private void TurnTimerOnTimerTick(TurnTimer sender, TimerEventArgs timerEventArgs)
		{
			_overlay.Dispatcher.BeginInvoke(new Action(() => _overlay.UpdateTurnTimer(timerEventArgs)));
			_timerWindow.Dispatcher.BeginInvoke(new Action(() => _timerWindow.Update(timerEventArgs)));
		}

		private void LogReaderOnCardPosChange(HsLogReader sender, CardPosChangeArgs args)
		{
			Logger.WriteLine(string.Format("Opponent{0} (id:{1} turn:{2} from:{3})", args.Action.ToString(), args.Id, args.Turn, args.From), "LogReader");
			switch (args.Action)
			{
				case OpponentHandMovement.Draw:
					_game.OpponentDraw(args);
					break;
				case OpponentHandMovement.Play:
					_game.OpponentPlay(args);
					break;
				case OpponentHandMovement.Mulligan:
					HandleOpponentMulligan(args.From);
					break;
				case OpponentHandMovement.FromPlayerDeck:
					_game.OpponentGet(args.Turn);
					break;
			}
		}

		private void LogReaderOnTurnStart(HsLogReader sender, TurnStartArgs args)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", args.Turn, sender.GetTurnNumber() + 1), "LogReader");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			TurnTimer.Instance.SetCurrentPlayer(args.Turn);
			TurnTimer.Instance.Restart();
			if (args.Turn == Turn.Player && !_game.IsInMenu)
			{
				if (Config.Instance.FlashHs)
					User32.FlashHs();

				if (Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}

		}

		private void LogReaderOnAnalyzing(HsLogReader sender, AnalyzingArgs args)
		{
			if (args.State == AnalyzingState.Start)
			{

			}
			else if (args.State == AnalyzingState.End)
			{
				//reader done analyzing new stuff, update things
				if (_overlay.IsVisible)
					_overlay.Update(false);

				if (_playerWindow.IsVisible)
					_playerWindow.SetCardCount(_game.PlayerHandCount, 30 - _game.PlayerDrawn.Sum(card => card.Count));

				if (_opponentWindow.IsVisible)
					_opponentWindow.SetOpponentCardCount(_game.OpponentHandCount, _game.OpponentDeckCount, _game.OpponentHasCoin);


				if (_showIncorrectDeckMessage && !_showingIncorrectDeckMessage)
				{
					_showingIncorrectDeckMessage = true;
					ShowIncorrectDeckMessage();
				}

			}
		}

		private void LogReaderOnGameStateChange(HsLogReader sender, GameStateArgs args)
		{
			if (!string.IsNullOrEmpty(args.PlayerHero))
			{
				_game.PlayingAs = args.PlayerHero;
				Logger.WriteLine("Playing as " + args.PlayerHero, "Hearthstone");

			}
			if (!string.IsNullOrEmpty(args.OpponentHero))
			{
				_game.PlayingAgainst = args.OpponentHero;
				Logger.WriteLine("Playing against " + args.OpponentHero, "Hearthstone");
			}

			if (args.State != null)
			{
				switch (args.State)
				{
					case GameState.GameBegin:
						HandleGameStart();
						break;
					case GameState.GameEnd:
						HandleGameEnd();
						break;
				}
			}
		}

		private void LogReaderOnCardMovement(HsLogReader sender, CardMovementArgs args)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", args.MovementType.ToString(), args.CardId, sender.GetTurnNumber(), args.From), "LogReader");

			switch (args.MovementType)
			{
				case CardMovementType.PlayerGet:
					HandlePlayerGet(args.CardId);
					break;
				case CardMovementType.PlayerDraw:
					HandlePlayerDraw(args.CardId);
					break;
				case CardMovementType.PlayerMulligan:
					HandlePlayerMulligan(args.CardId);
					break;
				case CardMovementType.PlayerHandDiscard:
					HandlePlayerHandDiscard(args.CardId);
					break;
				case CardMovementType.PlayerPlay:
					HandlePlayerPlay(args.CardId);
					break;
				case CardMovementType.PlayerDeckDiscard:
					HandlePlayerDeckDiscard(args.CardId);
					break;
				case CardMovementType.OpponentSecretTrigger:
					HandleOpponentSecretTrigger(args.CardId);
					break;
				case CardMovementType.OpponentPlay:
					//moved to CardPosChange
					break;
				case CardMovementType.OpponentHandDiscard:
					//moved to CardPosChange (included in play)
					break;
				case CardMovementType.OpponentDeckDiscard:
					HandleOpponentDeckDiscard(args.CardId);
					break;
				case CardMovementType.OpponentPlayToHand:
					HandleOpponentPlayToHand(args.CardId, sender.GetTurnNumber());
					break;
				default:
					Logger.WriteLine("Invalid card movement");
					break;
			}
		}

		#endregion

		#region Handle Events

		private void HandleGameStart()
		{
			//avoid new game being started when jaraxxus is played
			if (!_game.IsInMenu) return;

			Logger.WriteLine("Game start");

			if (Config.Instance.FlashHs)
				User32.FlashHs();
			if (Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if (Config.Instance.KeyPressOnGameStart != "None" && EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart);
			}

			var selectedDeck = DeckPickerList.SelectedDeck;
			if (selectedDeck != null)
				_game.SetPremadeDeck((Deck)selectedDeck.Clone());

			_game.IsInMenu = false;
			_game.Reset();

			//select deck based on hero
			if (!string.IsNullOrEmpty(_game.PlayingAs))
			{
				if (!_game.IsUsingPremade || !Config.Instance.AutoDeckDetection) return;

				if (selectedDeck == null || selectedDeck.Class != _game.PlayingAs)
				{

					var classDecks = _deckList.DecksList.Where(d => d.Class == _game.PlayingAs).ToList();
					if (classDecks.Count == 0)
					{
						Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
					}
					else if (classDecks.Count == 1)
					{
						DeckPickerList.SelectDeck(classDecks[0]);
						Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
					}
					else if (_deckList.LastDeckClass.Any(ldc => ldc.Class == _game.PlayingAs))
					{
						var lastDeckName = _deckList.LastDeckClass.First(ldc => ldc.Class == _game.PlayingAs).Name;
						Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

						var deck = _deckList.DecksList.FirstOrDefault(d => d.Name == lastDeckName);

						if (deck != null)
						{
							DeckPickerList.SelectDeck(deck);
							UpdateDeckList(deck);
							UseDeck(deck);
						}
					}
				}
			}
		}

		private void HandleGameEnd()
		{
			Logger.WriteLine("Game end");
			if (Config.Instance.KeyPressOnGameEnd != "None" && EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd);
			}
			TurnTimer.Instance.Stop();
			_overlay.HideTimers();
			_overlay.HideSecrets();
			if (Config.Instance.SavePlayedGames && !_game.IsInMenu)
			{
				SavePlayedCards();
			}
			if (!Config.Instance.KeepDecksVisible)
			{
				var deck = DeckPickerList.SelectedDeck;
				if (deck != null)
					_game.SetPremadeDeck((Deck)deck.Clone());

				_game.Reset();
			}
			_game.IsInMenu = true;
		}

		private void HandleOpponentPlayToHand(string cardId, int turn)
		{
			_game.OpponentBackToHand(cardId, turn);
		}

		private void HandlePlayerGet(string cardId)
		{
			_game.PlayerGet(cardId);
		}

		private void HandlePlayerDraw(string cardId)
		{
			var correctDeck = _game.PlayerDraw(cardId);

			if (!correctDeck && Config.Instance.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage && _game.IsUsingPremade)
			{
				_showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck");
			}
		}

		private void HandlePlayerMulligan(string cardId)
		{
			TurnTimer.Instance.MulliganDone(Turn.Player);
			_game.Mulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			_overlay.ListViewPlayer.Items.Refresh();
			_playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerHandDiscard(string cardId)
		{
			_game.PlayerHandDiscard(cardId);
			_overlay.ListViewPlayer.Items.Refresh();
			_playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerPlay(string cardId)
		{
			_game.PlayerPlayed(cardId);
			_overlay.ListViewPlayer.Items.Refresh();
			_playerWindow.ListViewPlayer.Items.Refresh();
		}

		private void HandlePlayerDeckDiscard(string cardId)
		{
			var correctDeck = _game.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if (!correctDeck && Config.Instance.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage && _game.IsUsingPremade)
			{
				_showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
			}
		}

		private void HandleOpponentSecretTrigger(string cardId)
		{
			_game.OpponentSecretTriggered(cardId);
			_game.OpponentSecretCount--;
			if (_game.OpponentSecretCount <= 0)
			{
				_overlay.HideSecrets();
			}
		}

		private void HandleOpponentMulligan(int pos)
		{
			TurnTimer.Instance.MulliganDone(Turn.Opponent);
			_game.OpponentMulligan(pos);
		}

		private void HandleOpponentDeckDiscard(string cardId)
		{
			_game.OpponentDeckDiscard(cardId);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			_overlay.ListViewOpponent.Items.Refresh();
			_opponentWindow.ListViewOpponent.Items.Refresh();
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
			if (!Config.Instance.MinimizeToTray) return;
			if (WindowState == WindowState.Minimized)
			{
				_notifyIcon.Visible = true;
				_notifyIcon.ShowBalloonTip(2000, "Hearthstone Deck Tracker", "Minimized to tray", System.Windows.Forms.ToolTipIcon.Info);
				Hide();
			}
		}

		private void Window_Closing_1(object sender, CancelEventArgs e)
		{
			try
			{
				_doUpdate = false;
				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;
				Config.Instance.WindowHeight = (int)Height;
				_overlay.Close();
				_logReader.Stop();
				_timerWindow.Shutdown();
				_playerWindow.Shutdown();
				_opponentWindow.Shutdown();
				WriteConfig();
				WriteDecks();
			}
			catch (Exception)
			{
				//doesnt matter
			}
		}

		private void NotifyIconOnMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
		{
			_notifyIcon.Visible = false;
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void BtnFilterTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutFilterTags.IsOpen = !FlyoutFilterTags.IsOpen;
		}

		private void TagControlFilterOnSelectedTagsChanged(TagControl sender, List<string> tags)
		{
			DeckPickerList.SetSelectedTags(tags);
			Config.Instance.SelectedTags = tags;
			WriteConfig();
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			PresentationSource presentationsource = PresentationSource.FromVisual(this);
			if (presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			if (!_foundHsDirectory)
			{
				ShowHsNotInstalledMessage();
				return;
			}
			if (_newVersion != null)
			{
				ShowNewUpdateMessage();
			}
			if (_updatedLogConfig)
			{
				ShowMessage("Restart Hearthstone", "This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			//preload the manacurve in new deck
			TabControlTracker.SelectedIndex = 1;
			TabControlTracker.UpdateLayout();
			TabControlTracker.SelectedIndex = 0;

			ManaCurveMyDecks.UpdateValues();

		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized) return;
			Config.Instance.TrackerWindowTop = (int)Top;
			Config.Instance.TrackerWindowLeft = (int)Left;
		}

		#endregion

		#region GENERAL METHODS

		private void ShowIncorrectDeckMessage()
		{
			var decks =
				_deckList.DecksList.Where(
					d => d.Class == _game.PlayingAs && _game.PlayerDrawn.All(c => d.Cards.Contains(c))
				).ToList();
			if (decks.Contains(DeckPickerList.SelectedDeck))
				decks.Remove(DeckPickerList.SelectedDeck);

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			if (decks.Count > 0)
			{
				DeckSelectionDialog dsDialog = new DeckSelectionDialog(decks);

				//todo: System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=ClassColor; DataItem=null; target element is 'GradientStop' (HashCode=7260326); target property is 'Color' (type 'Color')
				//when opened for seconds time. why?
				dsDialog.ShowDialog();



				var selectedDeck = dsDialog.SelectedDeck;

				if (selectedDeck != null)
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

			_showingIncorrectDeckMessage = false;
			_showIncorrectDeckMessage = false;
		}

		private void LoadConfig()
		{
			if (Config.Instance.TrackerWindowTop >= 0)
				Top = Config.Instance.TrackerWindowTop;
			if (Config.Instance.TrackerWindowLeft >= 0)
				Left = Config.Instance.TrackerWindowLeft;

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
			CheckboxHideDrawChances.IsChecked = Config.Instance.HideDrawChances;
			CheckboxHideOpponentDrawChances.IsChecked = Config.Instance.HideOpponentDrawChances;
			CheckboxHideOpponentCards.IsChecked = Config.Instance.HideOpponentCards;
			CheckboxHideOpponentCardCounter.IsChecked = Config.Instance.HideOpponentCardCount;
			CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			CheckboxHidePlayerCardCounter.IsChecked = Config.Instance.HidePlayerCardCount;
			CheckboxHidePlayerCards.IsChecked = Config.Instance.HidePlayerCards;
			CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			CheckboxWindowsOpenAutomatically.IsChecked = Config.Instance.WindowsOnStartup;
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
			CheckboxFlashHs.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;

			RangeSliderPlayer.UpperValue = 100 - Config.Instance.PlayerDeckTop;
			RangeSliderPlayer.LowerValue = (100 - Config.Instance.PlayerDeckTop) - Config.Instance.PlayerDeckHeight;
			SliderPlayer.Value = Config.Instance.PlayerDeckLeft;

			RangeSliderOpponent.UpperValue = 100 - Config.Instance.OpponentDeckTop;
			RangeSliderOpponent.LowerValue = (100 - Config.Instance.OpponentDeckTop) - Config.Instance.OpponentDeckHeight;
			SliderOpponent.Value = Config.Instance.OpponentDeckLeft;

			SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;

			DeckPickerList.ShowAll = Config.Instance.ShowAllDecks;
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;
			SliderTimersHorizontal.Value = Config.Instance.TimersHorizontalPosition;
			SliderTimersHorizontalSpacing.Value = Config.Instance.TimersHorizontalSpacing;
			SliderTimersVertical.Value = Config.Instance.TimersVerticalPosition;
			SliderTimersVerticalSpacing.Value = Config.Instance.TimersVerticalSpacing;

			SliderSecretsHorizontal.Value = Config.Instance.SecretsLeft;
			SliderSecretsVertical.Value = Config.Instance.SecretsTop;


			TagControlFilter.LoadTags(_deckList.AllTags);

			TagControlFilter.SetSelectedTags(Config.Instance.SelectedTags);
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			var tags = new List<string>(_deckList.AllTags);
			tags.Remove("All");
			TagControlNewDeck.LoadTags(tags);
			TagControlMyDecks.LoadTags(tags);
			DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			TagControlFilter.OperationSwitch.IsChecked = Config.Instance.TagOperation == Operation.And;

			ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
											   ? "#696969"
											   : Config.Instance.WindowsBackgroundHex;
			UpdateAdditionalWindowsBackground();

			ComboboxTextLocationPlayer.SelectedIndex = Config.Instance.TextOnTopPlayer ? 0 : 1;
			ComboboxTextLocationOpponent.SelectedIndex = Config.Instance.TextOnTopOpponent ? 0 : 1;
			_overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			_opponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);
			_overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			_playerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);

			if (Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
			{
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;
			}

			if (!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				Config.Instance.KeyPressOnGameStart = "None";
			}
			ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if (!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				Config.Instance.KeyPressOnGameEnd = "None";
			}
			ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
			CheckboxHideManaCurveNewDeck.IsChecked = Config.Instance.ManaCurveNewDeck;
			ManaCurveNewDeck.Visibility = Config.Instance.ManaCurveNewDeck ? Visibility.Visible : Visibility.Collapsed;

			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;

			CheckboxLogGames.IsChecked = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.IsEnabled = Config.Instance.SavePlayedGames;
			BtnLogGamesSelectDir.IsEnabled = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.Text = Config.Instance.SavePlayedGamesPath;

			if (Config.Instance.SavePlayedGames && TextboxLogGamesPath.Text.Length == 0)
				TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);

			CheckboxDeckSortingClassFirst.IsChecked = Config.Instance.CardSortingClassFirst;
		}

		private async void UpdateOverlayAsync()
		{
			bool hsForegroundChanged = false;
			while (_doUpdate)
			{
				if (Process.GetProcessesByName("Hearthstone").Length == 1)
				{
					_overlay.UpdatePosition();

					if (!User32.IsForegroundWindow("Hearthstone") && !hsForegroundChanged)
					{
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							_playerWindow.Topmost = false;
							_opponentWindow.Topmost = false;
							_timerWindow.Topmost = false;
						}
						hsForegroundChanged = true;

					}
					else if (hsForegroundChanged && User32.IsForegroundWindow("Hearthstone"))
					{
						_overlay.Update(true);
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							//if player topmost is set to true before opponent:
							//clicking on the playerwindow and back to hs causes the playerwindow to be behind hs.
							//other way around it works for both windows... what?
							_opponentWindow.Topmost = true;
							_playerWindow.Topmost = true;
							_timerWindow.Topmost = true;
						}
						hsForegroundChanged = false;
					}
				}
				else
				{
					_overlay.ShowOverlay(false);
				}
				await Task.Delay(Config.Instance.UpdateDelay);
			}
		}

		private async void ShowNewUpdateMessage()
		{
			var releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Download";
			settings.NegativeButtonText = "Not now";

			var result = await this.ShowMessageAsync("New Update available!", "Download version " + string.Format("{0}.{1}.{2}", _newVersion.Major, _newVersion.Minor,
													 _newVersion.Build) + " at\n" + releaseDownloadUrl, MessageDialogStyle.AffirmativeAndNegative, settings);

			if (result == MessageDialogResult.Affirmative)
			{
				Process.Start(releaseDownloadUrl);
			}
		}

		public async void ShowMessage(string title, string message)
		{
			await this.ShowMessageAsync(title, message);
		}

		private async void ShowHsNotInstalledMessage()
		{
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Ok";
			settings.NegativeButtonText = "Select manually";
			var result = await this.ShowMessageAsync("Hearthstone install directory not found", "Hearthstone Deck Tracker will not work properly if Hearthstone is not installed on your machine (obviously).", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				var dialog = new OpenFileDialog();
				dialog.Title = "Select Hearthstone.exe";
				dialog.DefaultExt = "Hearthstone.exe";
				dialog.Filter = "Hearthstone.exe|Hearthstone.exe";
				var dialogResult = dialog.ShowDialog();

				if (dialogResult == true)
				{
					Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
					WriteConfig();
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

		private void WriteConfig()
		{
			XmlManager<Config>.Save(_configPath, Config.Instance);
		}

		public void WriteDecks()
		{
			XmlManager<Decks>.Save(_decksPath, _deckList);
		}

		private void SavePlayedCards()
		{
			try
			{
				if (_game.PlayerDrawn != null && _game.PlayerDrawn.Count > 0)
				{
					var serializer = new XmlSerializer(typeof(Card[]));

					if (string.IsNullOrEmpty(Config.Instance.SavePlayedGamesPath))
						return;

					Directory.CreateDirectory(Config.Instance.SavePlayedGamesPath);
					var path = Config.Instance.SavePlayedGamesPath + "\\" + DateTime.Now.ToString("ddMMyyyyHHmmss");
					Directory.CreateDirectory(path);
					Logger.WriteLine("Saving games to: " + path);
					using (var sw = new StreamWriter(path + "\\Player.xml"))
					{
						serializer.Serialize(sw, _game.PlayerDrawn.ToArray());
						Logger.WriteLine("Success saving Player.xml");
					}
					using (var sw = new StreamWriter(path + "\\Opponent.xml"))
					{
						if (_game.OpponentCards != null)
							serializer.Serialize(sw, _game.OpponentCards.ToArray());
						Logger.WriteLine("Success saving Opponent.xml");
					}
				}
			}
			catch (Exception e)
			{
				Logger.WriteLine("Error saving game\n" + e.StackTrace);
			}
		}

		#endregion

		#region MY DECKS - GUI

		private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
		{
			Logger.WriteLine("set player item source as drawn");
			_overlay.ListViewPlayer.ItemsSource = _game.PlayerDrawn;
			_playerWindow.ListViewPlayer.ItemsSource = _game.PlayerDrawn;
			_game.IsUsingPremade = false;

			if (DeckPickerList.SelectedDeck != null)
				DeckPickerList.SelectedDeck.IsSelectedInGui = false;

			DeckPickerList.SelectedDeck = null;
			DeckPickerList.SelectedIndex = -1;
			DeckPickerList.ListboxPicker.Items.Refresh();

			UpdateDeckList(null);
			UseDeck(null);
			EnableDeckButtons(false);
			ManaCurveMyDecks.ClearDeck();
		}

		private void EnableDeckButtons(bool enable)
		{
			DeckOptionsFlyout.EnableButtons(enable);
			BtnEditDeck.IsEnabled = enable;
			BtnDeckOptions.IsEnabled = enable;
		}

		private async void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDeck;
			if (selectedDeck == null) return;

			if (_newContainsDeck)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";
				var result = await this.ShowMessageAsync("Found unfinished deck", "New Deck Section still contains an unfinished deck. Discard?", MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Negative)
				{
					TabControlTracker.SelectedIndex = 1;
					return;
				}
			}

			ClearNewDeckSection();
			_editingDeck = true;
			_newContainsDeck = true;
			_newDeck = (Deck)selectedDeck.Clone();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;

			if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
				ComboBoxSelectClass.SelectedValue = _newDeck.Class;

			TextBoxDeckName.Text = _newDeck.Name;
			UpdateNewDeckHeader(true);
			UpdateDbListView();


			TagControlNewDeck.SetSelectedTags(_newDeck.Tags);

			TabControlTracker.SelectedIndex = 1;
		}

		private void BtnSetTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutNewDeckSetTags.IsOpen = !FlyoutNewDeckSetTags.IsOpen;
		}

		public async Task ShowSavedFileMessage(string fileName, string dir)
		{
			var settings = new MetroDialogSettings();
			settings.NegativeButtonText = "Open folder";
			var result =
				await
				this.ShowMessageAsync("", "Saved to\n\"" + fileName + "\"", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				Process.Start(Path.GetDirectoryName(Application.ResourceAssembly.Location) + "\\" + dir);
			}
		}

		private void TagControlFilterOnOperationChanged(TagControl sender, Operation operation)
		{
			Config.Instance.TagOperation = operation;
			DeckPickerList.SetTagOperation(operation);
			DeckPickerList.UpdateList();
		}

		private void BtnDeckOptions_Click(object sender, RoutedEventArgs e)
		{
			FlyoutDeckOptions.IsOpen = true;
		}

		#endregion

		#region MY DECKS - METHODS

		private void UseDeck(Deck selected)
		{
			_game.Reset();

			if (selected != null)
				_game.SetPremadeDeck((Deck)selected.Clone());

			_logReader.Reset(true);

			_overlay.SortViews();

		}

		private void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if (selected == null)
			{

				Config.Instance.LastDeck = string.Empty;
				WriteConfig();
				return;
			}
			ListViewDeck.ItemsSource = selected.Cards;

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			Config.Instance.LastDeck = selected.Name;
			WriteConfig();
		}

		private void DeckPickerListOnSelectedDeckChanged(DeckPicker sender, Deck deck)
		{
			if (!_initialized) return;
			if (deck != null)
			{
				//set up notes
				DeckNotesEditor.SetDeck(deck);
				var flyoutHeader = deck.Name.Length >= 20 ? string.Join("", deck.Name.Take(17)) + "..." : deck.Name;
				FlyoutNotes.Header = flyoutHeader;
				FlyoutDeckOptions.Header = flyoutHeader;

				//change player deck itemsource
				if (_overlay.ListViewPlayer.ItemsSource != _game.PlayerDeck)
				{
					_overlay.ListViewPlayer.ItemsSource = _game.PlayerDeck;
					_playerWindow.ListViewPlayer.ItemsSource = _game.PlayerDeck;
					Logger.WriteLine("Set player itemsource as playerdeck");
				}
				_game.IsUsingPremade = true;
				UpdateDeckList(deck);
				UseDeck(deck);
				Logger.WriteLine("Switched to deck: " + deck.Name);

				//set and save last used deck for class
				while (_deckList.LastDeckClass.Any(ldc => ldc.Class == deck.Class))
				{
					var lastSelected = _deckList.LastDeckClass.FirstOrDefault(ldc => ldc.Class == deck.Class);
					if (lastSelected != null)
					{
						_deckList.LastDeckClass.Remove(lastSelected);
					}
					else
					{
						break;
					}
				}
				_deckList.LastDeckClass.Add(new DeckInfo() { Class = deck.Class, Name = deck.Name });
				WriteDecks();
				EnableDeckButtons(true);
				ManaCurveMyDecks.SetDeck(deck);
				TagControlMyDecks.SetSelectedTags(deck.Tags);
			}
			else
			{
				EnableDeckButtons(false);
			}
		}

		#endregion

		#region NEW DECK GUI

		private void ComboBoxFilterClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			_newDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newContainsDeck = true;
			UpdateDbListView();

			ManaCurveNewDeck.UpdateValues();
		}

		private async void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
		{
			_newDeck.Cards = new ObservableCollection<Card>(_newDeck.Cards.OrderBy(c => c.Cost).ThenByDescending(c => c.Type).ThenBy(c => c.Name).ToList());
			ListViewNewDeck.ItemsSource = _newDeck.Cards;

			if (_editingDeck)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Overwrite";
				settings.NegativeButtonText = "Save as new";
				var result =
					await
					this.ShowMessageAsync("Saving deck", "How do you wish to save the deck?",
										  MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
				{
					SaveDeck(true);
				}
				else if (result == MessageDialogResult.Negative)
				{
					SaveDeck(false);
				}
			}
			else
			{
				SaveDeck(false);
			}

			FlyoutNewDeckSetTags.IsOpen = false;
		}

		private void ComboBoxFilterMana_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			UpdateDbListView();
		}

		private void ComboboxNeutral_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			UpdateDbListView();
		}

		private void TextBoxDBFilter_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (ListViewDB.Items.Count == 1)
				{
					var card = (Card)ListViewDB.Items[0];
					AddCardToDeck((Card)card.Clone());
				}
			}
		}

		private void BtnImport_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutDeckImport.IsOpen = true;

		}

		private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewDB.SelectedItem;
				AddCardToDeck((Card)card.Clone());
				_newContainsDeck = true;
			}
		}

		private void ListViewNewDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{

			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				RemoveCardFromDeck(card);
			}
		}

		private void ListViewNewDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{

			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if (string.IsNullOrEmpty(card.Name)) return;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void Grid_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
			var info = new FileInfo(file);

			if (info.Extension != ".txt") return;

		}

		private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDbListView();
		}

		#endregion

		#region NEW DECK METHODS

		private void UpdateDbListView()
		{
			var selectedClass = ComboBoxSelectClass.SelectedValue.ToString();
			var selectedNeutral = ComboboxNeutral.SelectedValue.ToString();
			if (selectedClass == "Select a Class")
			{
				ListViewDB.Items.Clear();
			}
			else
			{
				ListViewDB.Items.Clear();

				foreach (var card in _game.GetActualCards())
				{
					if (!card.LocalizedName.ToLowerInvariant().Contains(TextBoxDBFilter.Text.ToLowerInvariant()))
						continue;
					// mana filter
					if (ComboBoxFilterMana.SelectedItem.ToString() == "All"
						|| ((ComboBoxFilterMana.SelectedItem.ToString() == "9+" && card.Cost >= 9)
						|| (ComboBoxFilterMana.SelectedItem.ToString() == card.Cost.ToString())))
					{
						switch (selectedNeutral)
						{
							case "Class + Neutral":
								if (card.GetPlayerClass == selectedClass || card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
							case "Class Only":
								if (card.GetPlayerClass == selectedClass)
									ListViewDB.Items.Add(card);
								break;
							case "Neutral Only":
								if (card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
						}
					}
				}
				if (_newDeck != null)
					ManaCurveNewDeck.SetDeck(_newDeck);

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private async void SaveDeck(bool overwrite)
		{
			var deckName = TextBoxDeckName.Text;

			if (string.IsNullOrEmpty(deckName))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = deckName;

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;

			}

			while (_deckList.DecksList.Any(d => d.Name == deckName) && (!_editingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = deckName;
				string name = await this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if (_newDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";

				var result =
					await this.ShowMessageAsync("Not 30 cards", string.Format("Deck contains {0} cards. Is this what you want to save anyway?",
										  _newDeck.Cards.Sum(c => c.Count)), MessageDialogStyle.AffirmativeAndNegative,
												settings);
				if (result != MessageDialogResult.Affirmative)
				{
					return;
				}
			}

			if (_editingDeck && overwrite)
			{
				_deckList.DecksList.Remove(_newDeck);
				DeckPickerList.RemoveDeck(_newDeck);
			}
			_newDeck.Name = deckName;
			_newDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newDeck.Tags = TagControlNewDeck.GetTags();

			var newDeckClone = (Deck)_newDeck.Clone();
			_deckList.DecksList.Add(newDeckClone);
			DeckPickerList.AddAndSelectDeck(newDeckClone);

			WriteDecks();
			BtnSaveDeck.Content = "Save";

			if (_editingDeck)
			{
				TagControlNewDeck.SetSelectedTags(new List<string>());
			}

			TabControlTracker.SelectedIndex = 0;
			_editingDeck = false;

			foreach (var tag in _newDeck.Tags)
			{
				TagControlFilter.AddSelectedTag(tag);
			}

			DeckPickerList.UpdateList();
			DeckPickerList.SelectDeck(newDeckClone);

			ClearNewDeckSection();
		}

		private void ClearNewDeckSection()
		{
			UpdateNewDeckHeader(false);
			ComboBoxSelectClass.SelectedIndex = 0;
			TextBoxDeckName.Text = string.Empty;
			TextBoxDBFilter.Text = string.Empty;
			ComboBoxFilterMana.SelectedIndex = 0;
			_newDeck = new Deck();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;
			_newContainsDeck = false;
			_editingDeck = false;
			ManaCurveNewDeck.ClearDeck();

		}

		private void RemoveCardFromDeck(Card card)
		{
			if (card == null)
				return;
			if (card.Count > 1)
			{
				_newDeck.Cards.Remove(card);
				card.Count--;
				_newDeck.Cards.Add(card);
			}
			else
				_newDeck.Cards.Remove(card);

			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		private void UpdateNewDeckHeader(bool show)
		{
			var headerText = "New Deck";
			var cardCount = _newDeck.Cards.Sum(c => c.Count);
			TabItemNewDeck.Header = show ? string.Format("{0} ({1})", headerText, cardCount) : headerText;
		}

		private void AddCardToDeck(Card card)
		{
			if (card == null)
				return;
			if (_newDeck.Cards.Contains(card))
			{
				var cardInDeck = _newDeck.Cards.First(c => c.Name == card.Name);
				cardInDeck.Count++;
			}
			else
			{
				_newDeck.Cards.Add(card);
			}

			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if (deck != null)
			{
				ClearNewDeckSection();
				_newContainsDeck = true;
				_editingDeck = editing;

				_newDeck = (Deck)deck.Clone();
				ListViewNewDeck.ItemsSource = _newDeck.Cards;

				if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
					ComboBoxSelectClass.SelectedValue = _newDeck.Class;

				TextBoxDeckName.Text = _newDeck.Name;
				UpdateNewDeckHeader(true);
				UpdateDbListView();
			}
		}

		private async void ShowClearNewDeckMessage()
		{
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Yes";
			settings.NegativeButtonText = "No";
			var result = await this.ShowMessageAsync("Clear deck?", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Affirmative)
			{
				ClearNewDeckSection();
			}
		}

		#endregion

		#region OPTIONS

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightCardsInHand = true;
			Game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightCardsInHand = false;
			Game.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInMenu = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInMenu = false;
			SaveConfig(true);
		}

		private void CheckboxHideDrawChances_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideDrawChances = true;
			SaveConfig(true);
			_playerWindow.LblDrawChance1.Visibility = Visibility.Collapsed;
			_playerWindow.LblDrawChance2.Visibility = Visibility.Collapsed;

		}

		private void CheckboxHideDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideDrawChances = false;
			SaveConfig(true);
			_playerWindow.LblDrawChance1.Visibility = Visibility.Visible;
			_playerWindow.LblDrawChance2.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentDrawChances_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = true;
			SaveConfig(true);
			_opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Collapsed;
			_opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = false;
			SaveConfig(true);
			_opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Visible;
			_opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Visible;

		}

		private void CheckboxHidePlayerCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = true;
			SaveConfig(true);
			_playerWindow.LblCardCount.Visibility = Visibility.Collapsed;
			_playerWindow.LblDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = false;
			SaveConfig(true);
			_playerWindow.LblCardCount.Visibility = Visibility.Visible;
			_playerWindow.LblDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHidePlayerCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = true;
			SaveConfig(true);
			_playerWindow.ListViewPlayer.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = false;
			SaveConfig(true);
			_playerWindow.ListViewPlayer.Visibility = Visibility.Visible;
		}


		private void CheckboxHideOpponentCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = true;
			SaveConfig(true);
			_opponentWindow.LblOpponentCardCount.Visibility = Visibility.Collapsed;
			_opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = false;
			SaveConfig(true);
			_opponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
			_opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = true;
			SaveConfig(true);
			_opponentWindow.ListViewOpponent.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = false;
			SaveConfig(true);
			_opponentWindow.ListViewOpponent.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmost = true;
			_playerWindow.Topmost = true;
			_opponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmost = false;
			_playerWindow.Topmost = false;
			_opponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_playerWindow.Show();
			_playerWindow.Activate();
			_opponentWindow.Show();
			_opponentWindow.Activate();

			_playerWindow.SetCardCount(_game.PlayerHandCount,
									   30 - _game.PlayerDrawn.Sum(card => card.Count));

			_opponentWindow.SetOpponentCardCount(_game.OpponentHandCount,
												 _game.OpponentDeckCount, _game.OpponentHasCoin);

			Config.Instance.WindowsOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_playerWindow.Hide();
			_opponentWindow.Hide();
			Config.Instance.WindowsOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			_playerWindow.Topmost = false;
			_opponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if (Config.Instance.WindowsTopmost)
			{
				_playerWindow.Topmost = true;
				_opponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = true;
			_timerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = false;
			_timerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_timerWindow.Show();
			_timerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_timerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			_timerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if (Config.Instance.TimerWindowTopmost)
			{
				_timerWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			WriteConfig();
			if (updateOverlay)
				_overlay.Update(true);
		}

		private void RangeSliderPlayer_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.PlayerDeckTop = 100 - RangeSliderPlayer.UpperValue;
			Config.Instance.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
		}

		private void RangeSliderPlayer_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
		}

		private void SliderPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.PlayerDeckLeft = SliderPlayer.Value;
			SaveConfig(true);
		}

		private void RangeSliderOpponent_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OpponentDeckTop = 100 - RangeSliderOpponent.UpperValue;
			Config.Instance.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
		}

		private void RangeSliderOpponent_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
		}

		private void SliderOpponent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.OpponentDeckLeft = SliderOpponent.Value;
			SaveConfig(true);
		}

		private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayOpacity = SliderOverlayOpacity.Value;
			SaveConfig(true);
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.KeepDecksVisible = true;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.KeepDecksVisible = false;
			SaveConfig(true);
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.MinimizeToTray = true;
			SaveConfig(false);
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.UseSameScaling = true;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.UseSameScaling = false;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoDeckDetection = true;
			CheckboxAutoSelectDeck.IsEnabled = true;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoDeckDetection = false;
			CheckboxAutoSelectDeck.IsChecked = false;
			CheckboxAutoSelectDeck.IsEnabled = false;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = true;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = false;
			SaveConfig(false);
		}

		private void RangeSliderPlayer_CentralThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveConfig(true);
		}

		private void RangeSliderPlayer_LowerThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveConfig(true);
		}

		private void RangeSliderPlayer_UpperThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveConfig(true);
		}

		private void RangeSliderOpponent_UpperThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{

			SaveConfig(true);
		}

		private void RangeSliderOpponent_LowerThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveConfig(true);
		}

		private void RangeSliderOpponent_CentralThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveConfig(true);
		}

		private void SliderOverlayPlayerScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			var scaling = SliderOverlayPlayerScaling.Value;
			Config.Instance.OverlayPlayerScaling = scaling;
			SaveConfig(false);
			_overlay.UpdateScaling();

			if (Config.Instance.UseSameScaling && SliderOverlayOpponentScaling.Value != scaling)
			{
				SliderOverlayOpponentScaling.Value = scaling;
			}
		}

		private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			var scaling = SliderOverlayOpponentScaling.Value;
			Config.Instance.OverlayOpponentScaling = scaling;
			SaveConfig(false);
			_overlay.UpdateScaling();

			if (Config.Instance.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
			{
				SliderOverlayPlayerScaling.Value = scaling;
			}
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		private void CheckboxHideTimers_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideTimers = true;
			SaveConfig(true);
		}

		private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideTimers = false;
			SaveConfig(true);
		}

		private void SliderTimersHorizontal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersHorizontalPosition = SliderTimersHorizontal.Value;
			SaveConfig(true);
		}

		private void SliderTimersHorizontalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersHorizontalSpacing = SliderTimersHorizontalSpacing.Value;
			SaveConfig(true);
		}

		private void SliderTimersVertical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersVerticalPosition = SliderTimersVertical.Value;
			SaveConfig(true);
		}

		private void SliderTimersVerticalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersVerticalSpacing = SliderTimersVerticalSpacing.Value;
			SaveConfig(true);
		}

		private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var accent = ComboboxAccent.SelectedItem as Accent;
			if (accent != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.DetectAppStyle().Item1);
				Config.Instance.AccentName = accent.Name;
				SaveConfig(false);
			}
		}

		private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if (theme != null)
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
			if (!_initialized) return;
			TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
			Config.Instance.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
			UpdateAdditionalWindowsBackground();
		}

		private void UpdateAdditionalWindowsBackground(Brush brush = null)
		{
			Brush background = brush;

			switch (ComboboxWindowBackground.SelectedItem.ToString())
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
			if (background == null)
			{
				var hexBackground = BackgroundFromHex();
				if (hexBackground != null)
				{
					_playerWindow.Background = hexBackground;
					_opponentWindow.Background = hexBackground;
					_timerWindow.Background = hexBackground;
				}
			}
			else
			{
				_playerWindow.Background = background;
				_opponentWindow.Background = background;
				_timerWindow.Background = background;
			}
		}

		private SolidColorBrush BackgroundFromHex()
		{
			SolidColorBrush brush = null;
			var hex = TextboxCustomBackground.Text;
			if (hex.StartsWith("#")) hex = hex.Remove(0, 1);
			if (!string.IsNullOrEmpty(hex) && hex.Length == 6 && Helper.IsHex(hex))
			{
				var color = ColorTranslator.FromHtml("#" + hex);
				brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
			}
			return brush;
		}

		private void TextboxCustomBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!_initialized || ComboboxWindowBackground.SelectedItem.ToString() != "Custom") return;
			var background = BackgroundFromHex();
			if (background != null)
			{
				UpdateAdditionalWindowsBackground(background);
				Config.Instance.WindowsBackgroundHex = TextboxCustomBackground.Text;
				SaveConfig(false);
			}
		}

		private void ComboboxTextLocationOpponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TextOnTopOpponent = ComboboxTextLocationOpponent.SelectedItem.ToString() == "Top";

			SaveConfig(false);
			_overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			_opponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);

		}

		private void ComboboxTextLocationPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;

			Config.Instance.TextOnTopPlayer = ComboboxTextLocationPlayer.SelectedItem.ToString() == "Top";
			SaveConfig(false);

			_overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			_playerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);
		}

		private async void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var language = ComboboxLanguages.SelectedValue.ToString();
			if (!Helper.LanguageDict.ContainsKey(language))
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			if (!File.Exists(string.Format("Files/cardsDB.{0}.json", selectedLanguage)))
			{
				return;
			}

			Config.Instance.SelectedLanguage = selectedLanguage;


			await Restart();
		}

		private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ExportSetDeckName = true;
			SaveConfig(false);
		}

		private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ExportSetDeckName = false;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PrioritizeGolden = true;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PrioritizeGolden = false;
			SaveConfig(false);
		}
		private void ComboboxKeyPressGameStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.KeyPressOnGameStart = ComboboxKeyPressGameStart.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.KeyPressOnGameEnd = ComboboxKeyPressGameEnd.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = false;
			SaveConfig(true);
		}

		private async void CheckboxAppData_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.SaveInAppData = true;
			SaveConfig(false);
			await Restart();
		}

		private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.SaveInAppData = false;
			SaveConfig(false);
			await Restart();
		}
		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveMyDecks = true;
			ManaCurveMyDecks.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveMyDecks = false;
			ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveNewDeck = true;
			ManaCurveNewDeck.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveNewDeck = false;
			ManaCurveNewDeck.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if (!_initialized) return;
			Config.Instance.TrackerCardToolTips = true;
			SaveConfig(false);
			await Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if (!_initialized) return;
			Config.Instance.TrackerCardToolTips = false;
			SaveConfig(false);
			await Restart();
		}

		private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowCardToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowCardToolTips = false;
			SaveConfig(false);
		}

		private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayCardToolTips = true;
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayCardToolTips = false;
			SaveConfig(true);
		}

		private void CheckboxLogGames_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TextboxLogGamesPath.IsEnabled = true;
			BtnLogGamesSelectDir.IsEnabled = true;
			Config.Instance.SavePlayedGames = true;
			if (TextboxLogGamesPath.Text.Length == 0)
				TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);
			SaveConfig(false);
		}

		private void CheckboxLogGames_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TextboxLogGamesPath.IsEnabled = false;
			BtnLogGamesSelectDir.IsEnabled = false;
			Config.Instance.SavePlayedGames = false;
			SaveConfig(false);
		}

		private void BtnLogGamesSelectDir_Click(object sender, RoutedEventArgs e)
		{
			var folderDialog = new FolderBrowserDialog();
			var result = folderDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				TextboxLogGamesPath.Text = folderDialog.SelectedPath;
				Config.Instance.SavePlayedGamesPath = folderDialog.SelectedPath;

				TextboxLogGamesPath.BorderBrush =
					new SolidColorBrush(TextboxLogGamesPath.Text.Length == 0
											? Colors.Red
											: SystemColors.ActiveBorderColor);
				SaveConfig(false);
			}
		}

		private void CheckboxDeckSortingClassFirst_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CardSortingClassFirst = true;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, true);
			Helper.SortCardCollection(ListViewNewDeck.Items, true);
		}

		private void CheckboxDeckSortingClassFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CardSortingClassFirst = false;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
			Helper.SortCardCollection(ListViewNewDeck.Items, false);
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.BringHsToForeground = true;
			SaveConfig(false);
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.BringHsToForeground = false;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.FlashHs = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.FlashHs = false;
			SaveConfig(false);
		}

		private void SliderSecretsHorizontal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.SecretsLeft = SliderSecretsHorizontal.Value;
			SaveConfig(true);
		}

		private void SliderSecretsVertical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.SecretsTop = SliderSecretsVertical.Value;
			SaveConfig(true);
		}

		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			_overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if (!_game.IsInMenu)
				_overlay.ShowSecrets(_game.PlayingAgainst);
		}

		private void BtnShowSecrets_Click(object sender, RoutedEventArgs e)
		{
			if (BtnShowSecrets.Content.Equals("Show"))
			{
				_overlay.ShowSecrets("Mage");
				BtnShowSecrets.Content = "Hide";
			}
			else
			{
				_overlay.HideSecrets();
				BtnShowSecrets.Content = "Show";
			}
		}
		#endregion

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightDiscarded = true;
			Game.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightDiscarded = false;
			Game.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private void BtnClear_Click(object sender, RoutedEventArgs e)
		{
			ShowClearNewDeckMessage();
		}
	}
}