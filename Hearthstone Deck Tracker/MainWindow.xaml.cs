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
        private const bool IS_DEBUG = false;

        private readonly Config _config;
        private readonly Decks _deckList;
        private readonly Game _game;
        private readonly bool _initialized;

        private readonly string _logConfigPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
            @"\Blizzard\Hearthstone\log.config";

        private readonly string _decksPath;
        private readonly string _configPath;

        private readonly HsLogReader _logReader;
        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        private readonly OpponentWindow _opponentWindow;
        private readonly OverlayWindow _overlay;
        private readonly PlayerWindow _playerWindow;
        private readonly TimerWindow _timerWindow;
        private readonly XmlManager<Decks> _xmlManager;
        private readonly XmlManager<Config> _xmlManagerConfig;
        private readonly DeckImporter _deckImporter;
        private readonly DeckExporter _deckExporter;
        private bool _editingDeck;
        private bool _newContainsDeck;
        private Deck _newDeck;
        private bool _doUpdate;
        private bool _showingIncorrectDeckMessage;
        private bool _showIncorrectDeckMessage;
        private readonly Version _newVersion;
        private readonly TurnTimer _turnTimer;
        private readonly bool _updatedLogConfig;
        private readonly bool _foundHsDirectory;
        private const string EventKeys = "None,F1,F2,F3,F4,F5,F6,F7,F8,F9,F10,F11,F12";

        public bool ShowToolTip 
        {
            get { return _config.TrackerCardToolTips; }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            
            var version = Helper.CheckForUpdates(out _newVersion);
            if (version != null)
            {
                TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", version.Major, version.Minor,
                                                     version.Build);
            }

            #region load config
            _config = new Config();
            _xmlManagerConfig = new XmlManager<Config> {Type = typeof (Config)};

            bool foundConfig = false;
            try
            {
                if(File.Exists("config.xml"))
                {
                    _config = _xmlManagerConfig.Load("config.xml");
                    foundConfig = true;
                }
                else if (File.Exists(_config.AppDataPath + @"\config.xml"))
                {
                    _config = _xmlManagerConfig.Load(_config.AppDataPath + @"\config.xml");
                    foundConfig = true;
                }
                else
                {
                    //save locally if appdata doesn't exist (when e.g. not on C)
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
                    {
                        _config.SaveInAppData = false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n If you don't know how to fix this, please delete " + _config.ConfigPath,
                    "Error loading config.xml");
                Application.Current.Shutdown();
            }
            _configPath = _config.ConfigPath;
            if (!foundConfig)
            {
                if(_config.HomeDir != string.Empty)
                    Directory.CreateDirectory(_config.HomeDir);
                using (var sr = new StreamWriter(_config.ConfigPath, false))
                {
                    sr.WriteLine("<Config></Config>");
                }
            }
            else
            {
                //check if config needs to be moved
                if (_config.SaveInAppData)
                {
                    if (File.Exists("config.xml"))
                    {
                        Directory.CreateDirectory(_config.HomeDir);
                        if (File.Exists(_config.ConfigPath))
                        {
                            //backup in case the file already exists
                            File.Move(_configPath, _configPath + DateTime.Now.ToFileTime());
                        }
                        File.Move("config.xml", _config.ConfigPath);
                        Logger.WriteLine("Moved config to appdata");
                    }
                }
                else
                {
                    if (File.Exists(_config.AppDataPath + @"\config.xml"))
                    {
                        if (File.Exists(_config.ConfigPath))
                        {
                            //backup in case the file already exists
                            File.Move(_configPath, _configPath + DateTime.Now.ToFileTime());
                        }
                        File.Move(_config.AppDataPath + @"\config.xml", _config.ConfigPath);
                        Logger.WriteLine("Moved config to local");
                    }
                }
            }
            #endregion

            if (_config.SelectedTags.Count == 0)
                _config.SelectedTags.Add("All");

            _config.Debug = IS_DEBUG;

            if (_config.GenerateLog)
            {
                Directory.CreateDirectory("Logs");
                var listener = new TextWriterTraceListener(_config.LogFilePath);
                Trace.Listeners.Add(listener);
                Trace.AutoFlush = true;
            }

            #region find hearthstone dir
            if (string.IsNullOrEmpty(_config.HearthstoneDirectory) || !File.Exists(_config.HearthstoneDirectory + @"\Hearthstone.exe"))
            {
                using (var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone"))
                {
                    if (hsDirKey != null)
                    {
                        var hsDir = (string)hsDirKey.GetValue("InstallLocation");

                        //verify the installlocation actually is correct (possibly moved?)
                        if (File.Exists(hsDir + @"\Hearthstone.exe"))
                        {
                            _config.HearthstoneDirectory = hsDir;
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
                        else if (_config.AlwaysOverwriteLogConfig)
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
            else
            {
                BtnExport.IsEnabled = false;
            }
            #endregion

            string languageTag = _config.SelectedLanguage;
            //hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
            _game = Helper.LanguageDict.ContainsValue(languageTag) ? new Game(languageTag) : new Game("enUS");
            _game.Reset();

            #region playerdecks
            _decksPath = _config.HomeDir + "PlayerDecks.xml";

            if (_config.SaveInAppData)
            {
                if(File.Exists("PlayerDecks.xml"))
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
                var appDataPath = _config.AppDataPath + @"\PlayerDecks.xml";
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

            _xmlManager = new XmlManager<Decks> {Type = typeof (Decks)};
            try
            {
                _deckList = _xmlManager.Load(_decksPath);
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


            _newDeck = new Deck();
            ListViewNewDeck.ItemsSource = _newDeck.Cards;


            //create overlay
            _overlay = new OverlayWindow(_config, _game) { Topmost = true };
            if (_foundHsDirectory)
            {
                _overlay.Show();
            }
            _playerWindow = new PlayerWindow(_config, _game.IsUsingPremade ? _game.PlayerDeck : _game.PlayerDrawn);
            _opponentWindow = new OpponentWindow(_config, _game.OpponentCards);
            _timerWindow = new TimerWindow(_config);

            if (_config.WindowsOnStartup)
            {
                _playerWindow.Show();
                _opponentWindow.Show();
            }
            if (_config.TimerWindowOnStartup)
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

            ComboboxKeyPressGameStart.ItemsSource = EventKeys.Split(',');
            ComboboxKeyPressGameEnd.ItemsSource = EventKeys.Split(',');
            

            LoadConfig();

            _deckImporter = new DeckImporter(_game);
            _deckExporter = new DeckExporter(_config);

            //this has to happen before reader starts
            var lastDeck = _deckList.DecksList.FirstOrDefault(d => d.Name == _config.LastDeck);
            DeckPickerList.SelectDeck(lastDeck);
            


            //log reader
            _logReader = new HsLogReader(_config.HearthstoneDirectory, _config.UpdateDelay);
            _logReader.CardMovement += LogReaderOnCardMovement;
            _logReader.GameStateChange += LogReaderOnGameStateChange;
            _logReader.Analyzing += LogReaderOnAnalyzing;
            _logReader.TurnStart += LogReaderOnTurnStart;
            _logReader.CardPosChange += LogReaderOnCardPosChange;

            _turnTimer = new TurnTimer(90);
            _turnTimer.TimerTick += TurnTimerOnTimerTick;

            TagControlFilter.HideStuffToCreateNewTag();
            TagControlSet.NewTag += TagControlSetOnNewTag;
            TagControlSet.SelectedTagsChanged += TagControlSetOnSelectedTagsChanged;
            TagControlSet.DeleteTag += TagControlSetOnDeleteTag;
            TagControlFilter.SelectedTagsChanged += TagControlFilterOnSelectedTagsChanged;


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
            _turnTimer.SetCurrentPlayer(args.Turn);
            _turnTimer.Restart();
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
                    _playerWindow.SetCardCount(_game.PlayerHandCount,
                                               30 - _game.PlayerDrawn.Sum(card => card.Count));

                if (_opponentWindow.IsVisible)
                    _opponentWindow.SetOpponentCardCount(_game.OpponentHandCount,
                                                         _game.OpponentDeckCount, _game.OpponentHasCoin);


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

            if (_config.KeyPressOnGameStart != "None" && EventKeys.Split(',').Contains(_config.KeyPressOnGameStart))
            {
                SendKeys.SendWait("{" + _config.KeyPressOnGameStart + "}");
                Logger.WriteLine("Sent keypress: " + _config.KeyPressOnGameStart);
            }

            var selectedDeck = DeckPickerList.SelectedDeck;
            if (selectedDeck != null)
                _game.SetPremadeDeck((Deck)selectedDeck.Clone());

            _game.IsInMenu = false;
            _game.Reset();

            //select deck based on hero
            if (!string.IsNullOrEmpty(_game.PlayingAs))
            {
                if (!_game.IsUsingPremade || !_config.AutoDeckDetection) return;
                
                if (selectedDeck == null || selectedDeck.Class != _game.PlayingAs)
                {

                    var classDecks = _deckList.DecksList.Where(d => d.Class == _game.PlayingAs).ToList();
                    if (classDecks.Count == 0)
                    {
                        Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
                        return;
                    }
                    if (classDecks.Count == 1)
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
            if (_config.KeyPressOnGameEnd != "None" && EventKeys.Split(',').Contains(_config.KeyPressOnGameEnd))
            {
                SendKeys.SendWait("{" + _config.KeyPressOnGameEnd + "}");
                Logger.WriteLine("Sent keypress: " + _config.KeyPressOnGameEnd);
            }
            _turnTimer.Stop();
            _overlay.HideTimers();
            if (_config.SavePlayedGames)
            {
                SavePlayedCards();
            }
            if (!_config.KeepDecksVisible)
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

            if (!correctDeck && _config.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage &&
                _game.IsUsingPremade)
            {
                _showIncorrectDeckMessage = true;
                Logger.WriteLine("Found incorrect deck");
            }
        }

        private void HandlePlayerMulligan(string cardId)
        {
            _turnTimer.MulliganDone(Turn.Player);
            _game.Mulligan(cardId);
        }

        private void HandlePlayerHandDiscard(string cardId)
        {
            _game.PlayerHandDiscard(cardId);
        }

        private void HandlePlayerPlay(string cardId)
        {
            _game.PlayerPlayed(cardId);
        }

        private void HandlePlayerDeckDiscard(string cardId)
        {
            var correctDeck = _game.PlayerDeckDiscard(cardId);
            
            //don't think this will ever detect an incorrect deck but who knows...
            if (!correctDeck && _config.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage && _game.IsUsingPremade)
            {
                _showIncorrectDeckMessage = true;
                Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
            }
        }

        private void HandleOpponentSecretTrigger(string cardId)
        {
            _game.OpponentSecretTriggered(cardId);
        }

        private void HandleOpponentMulligan(int pos)
        {
            _turnTimer.MulliganDone(Turn.Opponent);
            _game.OpponentMulligan(pos);
        }
        
        private void HandleOpponentDeckDiscard(string cardId)
        {
            _game.OpponentDeckDiscard(cardId);
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
            if (!_config.MinimizeToTray) return;
            if (WindowState == WindowState.Minimized)
            {
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(2000, "Hearthstone Deck Tracker", "Minimized to tray",
                                           System.Windows.Forms.ToolTipIcon.Info);
                Hide();
            }
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            try
            {
                _doUpdate = false;
                _config.SelectedTags = _config.SelectedTags.Distinct().ToList();
                _config.ShowAllDecks = DeckPickerList.ShowAll;
                _config.WindowHeight = (int)Height;
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
            _config.SelectedTags = tags;
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
                ShowUpdatedLogConfigMessage();
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
            _config.TrackerWindowTop = (int)Top;
            _config.TrackerWindowLeft = (int)Left;
        }

        #endregion

        #region GENERAL METHODS

        private void ShowIncorrectDeckMessage()
        {

            var decks =
                _deckList.DecksList.Where(
                    d => d.Class == _game.PlayingAs && _game.PlayerDrawn.All(c => d.Cards.Contains(c)))
                         .ToList();
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
            if (_config.TrackerWindowTop >= 0)
                Top = _config.TrackerWindowTop;
            if (_config.TrackerWindowLeft >= 0)
                Left = _config.TrackerWindowLeft;

            var theme = string.IsNullOrEmpty(_config.ThemeName)
                            ? ThemeManager.DetectAppStyle().Item1
                            : ThemeManager.AppThemes.First(t => t.Name == _config.ThemeName);
            var accent = string.IsNullOrEmpty(_config.AccentName)
                             ? ThemeManager.DetectAppStyle().Item2
                             : ThemeManager.Accents.First(a => a.Name == _config.AccentName);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            ComboboxTheme.SelectedItem = theme;
            ComboboxAccent.SelectedItem = accent;

            CheckboxSaveAppData.IsChecked = _config.SaveInAppData;

            Height = _config.WindowHeight;
            Game.HighlightCardsInHand = _config.HighlightCardsInHand;
            CheckboxHideOverlayInBackground.IsChecked = _config.HideInBackground;
            CheckboxHideDrawChances.IsChecked = _config.HideDrawChances;
            CheckboxHideOpponentDrawChances.IsChecked = _config.HideOpponentDrawChances;
            CheckboxHideOpponentCards.IsChecked = _config.HideOpponentCards;
            CheckboxHideOpponentCardCounter.IsChecked = _config.HideOpponentCardCount;
            CheckboxHideOpponentCardAge.IsChecked = _config.HideOpponentCardAge;
            CheckboxHidePlayerCardCounter.IsChecked = _config.HidePlayerCardCount;
            CheckboxHidePlayerCards.IsChecked = _config.HidePlayerCards;
            CheckboxHideOverlayInMenu.IsChecked = _config.HideInMenu;
            CheckboxHighlightCardsInHand.IsChecked = _config.HighlightCardsInHand;
            CheckboxHideOverlay.IsChecked = _config.HideOverlay;
            CheckboxHideDecksInOverlay.IsChecked = _config.HideDecksInOverlay;
            CheckboxKeepDecksVisible.IsChecked = _config.KeepDecksVisible;
            CheckboxMinimizeTray.IsChecked = _config.MinimizeToTray;
            CheckboxWindowsTopmost.IsChecked = _config.WindowsTopmost;
            CheckboxWindowsOpenAutomatically.IsChecked = _config.WindowsOnStartup;
            CheckboxTimerTopmost.IsChecked = _config.TimerWindowTopmost;
            CheckboxTimerWindow.IsChecked = _config.TimerWindowOnStartup;
            CheckboxTimerTopmostHsForeground.IsChecked = _config.TimerWindowTopmostIfHsForeground;
            CheckboxTimerTopmostHsForeground.IsEnabled = _config.TimerWindowTopmost;
            CheckboxSameScaling.IsChecked = _config.UseSameScaling;
            CheckboxDeckDetection.IsChecked = _config.AutoDeckDetection;
            CheckboxWinTopmostHsForeground.IsChecked = _config.WindowsTopmostIfHsForeground;
            CheckboxWinTopmostHsForeground.IsEnabled = _config.WindowsTopmost;
            CheckboxAutoSelectDeck.IsEnabled = _config.AutoDeckDetection;
            CheckboxAutoSelectDeck.IsChecked = _config.AutoSelectDetectedDeck;
            CheckboxExportName.IsChecked = _config.ExportSetDeckName;
            CheckboxPrioGolden.IsChecked = _config.PrioritizeGolden;

            RangeSliderPlayer.UpperValue = 100 - _config.PlayerDeckTop;
            RangeSliderPlayer.LowerValue = (100 - _config.PlayerDeckTop) - _config.PlayerDeckHeight;
            SliderPlayer.Value = _config.PlayerDeckLeft;

            RangeSliderOpponent.UpperValue = 100 - _config.OpponentDeckTop;
            RangeSliderOpponent.LowerValue = (100 - _config.OpponentDeckTop) - _config.OpponentDeckHeight;
            SliderOpponent.Value = _config.OpponentDeckLeft;

            SliderOverlayOpacity.Value = _config.OverlayOpacity;
            SliderOpponentOpacity.Value = _config.OpponentOpacity;
            SliderPlayerOpacity.Value = _config.PlayerOpacity;
            SliderOverlayPlayerScaling.Value = _config.OverlayPlayerScaling;
            SliderOverlayOpponentScaling.Value = _config.OverlayOpponentScaling;

            DeckPickerList.ShowAll = _config.ShowAllDecks;
            DeckPickerList.SetSelectedTags(_config.SelectedTags);

            CheckboxHideTimers.IsChecked = _config.HideTimers;
            SliderTimersHorizontal.Value = _config.TimersHorizontalPosition;
            SliderTimersHorizontalSpacing.Value = _config.TimersHorizontalSpacing;
            SliderTimersVertical.Value = _config.TimersVerticalPosition;
            SliderTimersVerticalSpacing.Value = _config.TimersVerticalSpacing;

            TagControlFilter.LoadTags(_deckList.AllTags);

            TagControlFilter.SetSelectedTags(_config.SelectedTags);
            DeckPickerList.SetSelectedTags(_config.SelectedTags);

            var tags = new List<string>(_deckList.AllTags);
            tags.Remove("All");
            TagControlSet.LoadTags(tags);

            ComboboxWindowBackground.SelectedItem = _config.SelectedWindowBackground;
            TextboxCustomBackground.IsEnabled = _config.SelectedWindowBackground == "Custom";
            TextboxCustomBackground.Text = string.IsNullOrEmpty(_config.WindowsBackgroundHex)
                                               ? "#696969"
                                               : _config.WindowsBackgroundHex;
            UpdateAdditionalWindowsBackground();
            
            ComboboxTextLocationPlayer.SelectedIndex = _config.TextOnTopPlayer ? 0 : 1;
            ComboboxTextLocationOpponent.SelectedIndex = _config.TextOnTopOpponent ? 0 : 1;
            _overlay.SetOpponentTextLocation(_config.TextOnTopOpponent);
            _opponentWindow.SetTextLocation(_config.TextOnTopOpponent);
            _overlay.SetPlayerTextLocation(_config.TextOnTopPlayer);
            _playerWindow.SetTextLocation(_config.TextOnTopPlayer);

            if (Helper.LanguageDict.Values.Contains(_config.SelectedLanguage))
            {
                ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == _config.SelectedLanguage).Key;
            }

            if (!EventKeys.Split(',').Contains(_config.KeyPressOnGameStart))
            {
                _config.KeyPressOnGameStart = "None";
            }
            ComboboxKeyPressGameStart.SelectedValue = _config.KeyPressOnGameStart;

            if (!EventKeys.Split(',').Contains(_config.KeyPressOnGameEnd))
            {
                _config.KeyPressOnGameEnd = "None";
            }
            ComboboxKeyPressGameEnd.SelectedValue = _config.KeyPressOnGameEnd;

            CheckboxHideManaCurveMyDecks.IsChecked = _config.ManaCurveMyDecks;
            ManaCurveMyDecks.Visibility = _config.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
            CheckboxHideManaCurveNewDeck.IsChecked = _config.ManaCurveNewDeck;
            ManaCurveNewDeck.Visibility = _config.ManaCurveNewDeck ? Visibility.Visible : Visibility.Collapsed;

            CheckboxTrackerCardToolTips.IsChecked = _config.TrackerCardToolTips;
            CheckboxWindowCardToolTips.IsChecked = _config.WindowCardToolTips;
            CheckboxOverlayCardToolTips.IsChecked = _config.OverlayCardToolTips;

            CheckboxLogGames.IsChecked = _config.SavePlayedGames;
            TextboxLogGamesPath.IsEnabled = _config.SavePlayedGames;
            BtnLogGamesSelectDir.IsEnabled = _config.SavePlayedGames;
            TextboxLogGamesPath.Text = _config.SavePlayedGamesPath;

            if (_config.SavePlayedGames && TextboxLogGamesPath.Text.Length == 0)
                TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void SortCardCollection(ItemCollection collection)
        {
            var view1 = (CollectionView) CollectionViewSource.GetDefaultView(collection);
            view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
            view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
            view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
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
                        if(_config.WindowsTopmostIfHsForeground && _config.WindowsTopmost)
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
                        if (_config.WindowsTopmostIfHsForeground && _config.WindowsTopmost)
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
                await Task.Delay(_config.UpdateDelay);
            }
        }

        private async void ShowNewUpdateMessage()
        {

            var releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
            var settings = new MetroDialogSettings();
            settings.AffirmativeButtonText = "Download";
            settings.NegativeButtonText = "Not now";

            var result =
                await this.ShowMessageAsync("New Update available!", "Download version " + string.Format("{0}.{1}.{2}", _newVersion.Major, _newVersion.Minor,
                                                     _newVersion.Build) + " at\n" + releaseDownloadUrl, MessageDialogStyle.AffirmativeAndNegative,
                                            settings);
            if (result == MessageDialogResult.Affirmative)
            {
                Process.Start(releaseDownloadUrl);
            }

        }

        private async void ShowUpdatedLogConfigMessage()
        {
            await this.ShowMessageAsync("Restart Hearthstone", "This is either your first time starting the tracker or the log.config file has been updated. Please restart heartstone once, for the tracker to work properly.");
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
                    _config.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
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
            _xmlManagerConfig.Save(_configPath, _config);
        }

        private void WriteDecks()
        {
            _xmlManager.Save(_decksPath, _deckList);
        }

        private void SavePlayedCards()
        {
            try
            {
                if (_game.PlayerDrawn != null && _game.PlayerDrawn.Count > 0)
                {
                    var serializer = new XmlSerializer(typeof(Card[]);

                    if (string.IsNullOrEmpty(_config.SavePlayedGamesPath))
                        return;

                    Directory.CreateDirectory(_config.SavePlayedGamesPath);
                    var dateString = string.Format("{0}{1}{2}{3}{4}{5}", DateTime.Now.Day, DateTime.Now.Month,
                                                   DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute,
                                                   DateTime.Now.Second);
                    var path = _config.SavePlayedGamesPath + "\\" + dateString;
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

        private void EnableDeckButtons(bool enable)
        {
            BtnScreenshot.IsEnabled = enable;
            BtnNotes.IsEnabled = enable;
            BtnExport.IsEnabled = enable;
            BtnDeleteDeck.IsEnabled = enable;
            BtnEditDeck.IsEnabled = enable;
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
            _newDeck = (Deck) selectedDeck.Clone();
            ListViewNewDeck.ItemsSource = _newDeck.Cards;

            if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
                ComboBoxSelectClass.SelectedValue = _newDeck.Class;

            TextBoxDeckName.Text = _newDeck.Name;
            UpdateNewDeckHeader(true);
            UpdateDbListView();


            TagControlSet.SetSelectedTags(_newDeck.Tags);

            TabControlTracker.SelectedIndex = 1;
        }

        private async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            var deck = DeckPickerList.SelectedDeck;
            if (deck != null)
            {
                var settings = new MetroDialogSettings();
                settings.AffirmativeButtonText = "Yes";
                settings.NegativeButtonText = "No";
                var result = await this.ShowMessageAsync("Deleting " + deck.Name, "Are you Sure?", MessageDialogStyle.AffirmativeAndNegative, settings);
                if (result == MessageDialogResult.Affirmative)
                {
                    try
                    {
                        _deckList.DecksList.Remove(deck);
                        WriteDecks();
                        DeckPickerList.RemoveDeck(deck);
                        ListViewDeck.Items.Clear();
                    }
                    catch (Exception)
                    {
                        Logger.WriteLine("Error deleting deck");
                    }
                }
            }
        }
        
        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var deck = DeckPickerList.SelectedDeck;
            if (deck == null) return;

            var result = await this.ShowMessageAsync("Export " + deck.Name + " to Hearthstone",
                                               "Please create a new, empty " + deck.Class + "-Deck in Hearthstone before continuing (leave the deck creation screen open).\nDo not move your mouse after clicking OK!",
                                               MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                var controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
                Topmost = false;
                await Task.Delay(500);
                await _deckExporter.Export(DeckPickerList.SelectedDeck);
                await controller.CloseAsync();
            }


        }

        private void BtnSetTag_Click(object sender, RoutedEventArgs e)
        {
            FlyoutSetTags.IsOpen = !FlyoutSetTags.IsOpen;
        }

        private void TagControlSetOnNewTag(TagControl sender, string tag)
        {
            if (!_deckList.AllTags.Contains(tag))
            {
                _deckList.AllTags.Add(tag);
                WriteDecks();
                TagControlFilter.LoadTags(_deckList.AllTags);
            }
        }

        private void TagControlSetOnDeleteTag(TagControl sender, string tag)
        {
            if (_deckList.AllTags.Contains(tag))
            {
                _deckList.AllTags.Remove(tag);
                foreach (var deck in _deckList.DecksList)
                {
                    if (deck.Tags.Contains(tag))
                    {
                        deck.Tags.Remove(tag);
                    }
                }
                if (_newDeck.Tags.Contains(tag))
                    _newDeck.Tags.Remove(tag);

                WriteDecks();
                TagControlFilter.LoadTags(_deckList.AllTags);
                DeckPickerList.UpdateList();
            }
        }

        private void TagControlSetOnSelectedTagsChanged(TagControl sender, List<string> tags)
        {
            if (_newDeck == null) return;
            BtnSaveDeck.Content = "Save*";
        }

        private void BtnNotes_Click(object sender, RoutedEventArgs e)
        {
            if (DeckPickerList.SelectedDeck == null) return;
            FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
        }

        private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
        {
            if (DeckPickerList.SelectedDeck == null) return;
            PlayerWindow screenShotWindow = new PlayerWindow(_config, DeckPickerList.SelectedDeck.Cards, true);
            screenShotWindow.Show();
            screenShotWindow.Top = 0;
            screenShotWindow.Left = 0;
            await Task.Delay(100);
            PresentationSource source = PresentationSource.FromVisual(screenShotWindow);
            if (source == null) return;

            double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

            var fileName = Helper.ScreenshotDeck(screenShotWindow.ListViewPlayer, dpiX, dpiY, DeckPickerList.SelectedDeck.Name);

            screenShotWindow.Shutdown();
            if (fileName == null)
            {
                await this.ShowMessageAsync("","Error saving screenshot");
            }
            else
            {
                var settings = new MetroDialogSettings();
                settings.NegativeButtonText = "Open folder";
                var result = await this.ShowMessageAsync("", "Saved to " + fileName, MessageDialogStyle.AffirmativeAndNegative, settings);
                if (result == MessageDialogResult.Negative)
                {
                    Process.Start(Path.GetDirectoryName(Application.ResourceAssembly.Location) + "/Screenshots");
                }
            }
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
            ListViewDeck.Items.Clear();
            if (selected == null)
            {

                _config.LastDeck = string.Empty;
                WriteConfig();
                return;
            }
            foreach (var card in selected.Cards)
            {
                ListViewDeck.Items.Add(card);
            }

            SortCardCollection(ListViewDeck.Items);
            _config.LastDeck = selected.Name;
            WriteConfig();
        }

        private void DeckPickerListOnSelectedDeckChanged(DeckPicker sender, Deck deck)
        {
            if (!_initialized) return;
            if (deck != null)
            {
                //set up notes
                DeckNotesEditor.SetDeck(deck);
                FlyoutNotes.Header = deck.Name.Length >= 20 ? string.Join("", deck.Name.Take(17)) + "..." : deck.Name;

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
                _deckList.LastDeckClass.Add(new DeckInfo() {Class = deck.Class, Name = deck.Name});
                WriteDecks();
                EnableDeckButtons(true);
                ManaCurveMyDecks.SetDeck(deck);
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
            FlyoutSetTags.IsOpen = false;
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
                    var card = (Card) ListViewDB.Items[0];
                    AddCardToDeck((Card)card.Clone());
                }
            }
        }

        private async void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings();
            Deck deck = null;
            var clipboard = Clipboard.GetText();
            var clipboardLines = clipboard.Split('\n');
            var validUrls = new[]
                {
                    "hearthstats", "hss.io", "hearthpwn", "hearthhead", "hearthstoneplayers", "tempostorm",
                    "hearthstonetopdeck"
                };
            if (validUrls.Any(clipboard.Contains))
            {
                settings.DefaultText = clipboard;
            }
            else if (clipboardLines.Length >= 1 && clipboardLines.Length <= 100)
            {
                try
                {
                    foreach (var line in clipboardLines)
                    {
                        var parts = line.Split(new[] {" x "}, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;
                        var name = parts[0].Trim();
                        while (name.Length > 0 && Helper.IsNumeric(name[0]))
                            name = name.Remove(0, 1);

                        var card = _game.GetCardFromName(name);
                        if (card.Id == "UNKNOWN")
                            continue;

                        if (parts.Length > 1)
                            int.TryParse(parts[1], out card.Count);

                        if (deck == null)
                            deck = new Deck();

                        if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                            deck.Class = card.PlayerClass;

                        deck.Cards.Add(card);
                    }

                    if (deck != null)
                    {
                        ClearNewDeckSection();
                        _newContainsDeck = true;

                        _newDeck = (Deck)deck.Clone();
                        ListViewNewDeck.ItemsSource = _newDeck.Cards;

                        if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
                            ComboBoxSelectClass.SelectedValue = _newDeck.Class;

                        TextBoxDeckName.Text = _newDeck.Name;
                        UpdateNewDeckHeader(true);
                        UpdateDbListView();
                        return;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }

            //import dialog
            var url = await this.ShowInputAsync("Import deck", "Currently supported:\nhearthstats, hearthpwn, hearthhead, hearthstoneplayers, hearthstonetopdeck and tempostorm\n\nUrl:", settings);
            if (string.IsNullOrEmpty(url))
                return;

            var controller = await this.ShowProgressAsync("Loading Deck...", "please wait");

            deck = await _deckImporter.Import(url);

            await controller.CloseAsync();

            if (deck != null)
            {
                var reimport = _editingDeck && _newDeck != null && _newDeck.Url == url;

                deck.Url = url;

                if(reimport) //keep old notes
                    deck.Note = _newDeck.Note;

                if(!deck.Note.Contains(url))
                    deck.Note = url + "\n" + deck.Note;

                ClearNewDeckSection();
                _newContainsDeck = true;
                _editingDeck = reimport;

                _newDeck = (Deck)deck.Clone();
                ListViewNewDeck.ItemsSource = _newDeck.Cards;

                if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
                    ComboBoxSelectClass.SelectedValue = _newDeck.Class;

                TextBoxDeckName.Text = _newDeck.Name;
                UpdateNewDeckHeader(true);
                UpdateDbListView();
            }
            else
            {
                await this.ShowMessageAsync("Error", "Could not load deck from specified url");
            }


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
                var card = (Card) ListViewDB.SelectedItem;
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
            using (var sr = new StreamReader(file))
            {
                var lines = sr.ReadToEnd().Split('\n');
                var deck = new Deck();
                foreach (var line in lines)
                {
                    var card = _game.GetCardFromName(line.Trim());
                    if (card.Name == "") continue;

                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }

                    if (deck.Cards.Contains(card))
                    {
                        var deckCard = deck.Cards.First(c => c.Equals(card));
                        deck.Cards.Remove(deckCard);
                        deckCard.Count++;
                        deck.Cards.Add(deckCard);
                    }
                    else
                    {
                        deck.Cards.Add(card);
                    }
                }
                ClearNewDeckSection();
                _newContainsDeck = true;

                _newDeck = (Deck)deck.Clone();
                ListViewNewDeck.ItemsSource = _newDeck.Cards;

                if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
                    ComboBoxSelectClass.SelectedValue = _newDeck.Class;

                TextBoxDeckName.Text = _newDeck.Name;
                UpdateNewDeckHeader(true);
                UpdateDbListView();
            }
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
                    if (!card.LocalizedName.ToLower().Contains(TextBoxDBFilter.Text.ToLower()))
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
                                {
                                    ListViewDB.Items.Add(card);
                                }
                                break;
                            case "Neutral Only":
                                if (card.GetPlayerClass == "Neutral")
                                {
                                    ListViewDB.Items.Add(card);
                                }
                                break;
                        }
                    }
                }
                if(_newDeck != null)
                    ManaCurveNewDeck.SetDeck(_newDeck);

                var view1 = (CollectionView) CollectionViewSource.GetDefaultView(ListViewDB.Items);
                view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
                view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
                view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

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
            _newDeck.Tags = TagControlSet.GetTags();
            
            var newDeckClone = (Deck) _newDeck.Clone();
            _deckList.DecksList.Add(newDeckClone);
            DeckPickerList.AddAndSelectDeck(newDeckClone);

            WriteDecks();
            BtnSaveDeck.Content = "Save";

            if (_editingDeck)
            {
                TagControlSet.SetSelectedTags(new List<string>());
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

            SortCardCollection(ListViewNewDeck.Items);
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
                _newDeck.Cards.Remove(cardInDeck);
                cardInDeck.Count++;
                _newDeck.Cards.Add(cardInDeck);
            }
            else
            {
                _newDeck.Cards.Add(card);
            }

            SortCardCollection(ListViewNewDeck.Items);
            BtnSaveDeck.Content = "Save*";
            UpdateNewDeckHeader(true);
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
            _config.HighlightCardsInHand = true;
            Game.HighlightCardsInHand = true;
            SaveConfig(true);
        }

        private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HighlightCardsInHand = false;
            Game.HighlightCardsInHand = false;
            SaveConfig(true);
        }

        private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOverlay = true;
            SaveConfig(true);
        }

        private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOverlay = false;
            SaveConfig(true);
        }

        private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInMenu = true;
            SaveConfig(true);
        }

        private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInMenu = false;
            SaveConfig(true);
        }

        private void CheckboxHideDrawChances_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideDrawChances = true;
            SaveConfig(true);
            _playerWindow.LblDrawChance1.Visibility = Visibility.Collapsed;
            _playerWindow.LblDrawChance2.Visibility = Visibility.Collapsed;

        }

        private void CheckboxHideDrawChances_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideDrawChances = false;
            SaveConfig(true);
            _playerWindow.LblDrawChance1.Visibility = Visibility.Visible;
            _playerWindow.LblDrawChance2.Visibility = Visibility.Visible;
        }

        private void CheckboxHideOpponentDrawChances_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentDrawChances = true;
            SaveConfig(true);
            _opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Collapsed;
            _opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHideOpponentDrawChances_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentDrawChances = false;
            SaveConfig(true);
            _opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Visible;
            _opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Visible;

        }

        private void CheckboxHidePlayerCardCounter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCardCount = true;
            SaveConfig(true);
            _playerWindow.LblCardCount.Visibility = Visibility.Collapsed;
            _playerWindow.LblDeckCount.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHidePlayerCardCounter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCardCount = false;
            SaveConfig(true);
            _playerWindow.LblCardCount.Visibility = Visibility.Visible;
            _playerWindow.LblDeckCount.Visibility = Visibility.Visible;
        }

        private void CheckboxHidePlayerCards_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCards = true;
            SaveConfig(true);
            _playerWindow.ListViewPlayer.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHidePlayerCards_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCards = false;
            SaveConfig(true); 
            _playerWindow.ListViewPlayer.Visibility = Visibility.Visible;
        }


        private void CheckboxHideOpponentCardCounter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardCount = true;
            SaveConfig(true);
            _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Collapsed;
            _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHideOpponentCardCounter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardCount = false;
            SaveConfig(true);
            _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
            _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
        }

        private void CheckboxHideOpponentCards_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCards = true;
            SaveConfig(true);
            _opponentWindow.ListViewOpponent.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHideOpponentCards_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCards = false;
            SaveConfig(true);
            _opponentWindow.ListViewOpponent.Visibility = Visibility.Visible;
        }

        private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardAge = false;
            SaveConfig(true);
        }

        private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardAge = true;
            SaveConfig(true);
        }

        private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardMarks = false;
            SaveConfig(true);
        }

        private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOpponentCardMarks = true;
            SaveConfig(true);
        }

        private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInBackground = true;
            SaveConfig(true);
        }

        private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInBackground = false;
            SaveConfig(true);
        }

        private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmost = true;
            _playerWindow.Topmost = true;
            _opponentWindow.Topmost = true;
            CheckboxWinTopmostHsForeground.IsEnabled = true;
            SaveConfig(true);
        }

        private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmost = false;
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

            _config.WindowsOnStartup = true;
            SaveConfig(true);
        }

        private void CheckboxWindowsOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _playerWindow.Hide();
            _opponentWindow.Hide();
            _config.WindowsOnStartup = false;
            SaveConfig(true);
        }

        private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmostIfHsForeground = true;
            _playerWindow.Topmost = false;
            _opponentWindow.Topmost = false;
            SaveConfig(false);
        }

        private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmostIfHsForeground = false;
            if (_config.WindowsTopmost)
            {
                _playerWindow.Topmost = true;
                _opponentWindow.Topmost = true;
            }
            SaveConfig(false);
        }

        private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.TimerWindowTopmost = true;
            _timerWindow.Topmost = true;
            CheckboxTimerTopmostHsForeground.IsEnabled = true;
            SaveConfig(true);
        }

        private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.TimerWindowTopmost = false;
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
            _config.TimerWindowOnStartup = true;
            SaveConfig(true);
        }

        private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _timerWindow.Hide();
            _config.TimerWindowOnStartup = false;
            SaveConfig(true);
        }

        private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.TimerWindowTopmostIfHsForeground = true;
            _timerWindow.Topmost = false;
            SaveConfig(false);
        }

        private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.TimerWindowTopmostIfHsForeground = false;
            if (_config.TimerWindowTopmost)
            {
                _timerWindow.Topmost = true;
            }
            SaveConfig(false);
        }

        private void SaveConfig(bool updateOverlay)
        {
            WriteConfig();
            if(updateOverlay)
                _overlay.Update(true);
        }

        private void RangeSliderPlayer_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.PlayerDeckTop = 100 - RangeSliderPlayer.UpperValue;
            _config.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
        }

        private void RangeSliderPlayer_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
        }

        private void SliderPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.PlayerDeckLeft = SliderPlayer.Value;
            SaveConfig(true);
        }

        private void RangeSliderOpponent_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.OpponentDeckTop = 100 - RangeSliderOpponent.UpperValue;
            _config.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
        }

        private void RangeSliderOpponent_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
        }

        private void SliderOpponent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.OpponentDeckLeft = SliderOpponent.Value;
            SaveConfig(true);
        }

        private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.OverlayOpacity = SliderOverlayOpacity.Value;
            SaveConfig(true);
        }

        private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.OpponentOpacity = SliderOpponentOpacity.Value;
            SaveConfig(true);
        }

        private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.PlayerOpacity = SliderPlayerOpacity.Value;
            SaveConfig(true);
        }

        private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.KeepDecksVisible = true;
            SaveConfig(true);
        }

        private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.KeepDecksVisible = false;
            SaveConfig(true);
        }

        private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.MinimizeToTray = true;
            SaveConfig(false);
        }

        private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.MinimizeToTray = false;
            SaveConfig(false);
        }

        private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.UseSameScaling = true;
            SaveConfig(false);
        }

        private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.UseSameScaling = false;
            SaveConfig(false);
        }

        private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.AutoDeckDetection = true;
            CheckboxAutoSelectDeck.IsEnabled = true;
            SaveConfig(false);
        }

        private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.AutoDeckDetection = false;
            CheckboxAutoSelectDeck.IsChecked = false;
            CheckboxAutoSelectDeck.IsEnabled = false;
            SaveConfig(false);
        }

        private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.AutoSelectDetectedDeck = true;
            SaveConfig(false);
        }

        private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.AutoSelectDetectedDeck = false;
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
            _config.OverlayPlayerScaling = scaling;
            SaveConfig(false);
             _overlay.UpdateScaling();

            if (_config.UseSameScaling && SliderOverlayOpponentScaling.Value != scaling)
            {
                SliderOverlayOpponentScaling.Value = scaling;
            }
        }

        private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            var scaling = SliderOverlayOpponentScaling.Value;
            _config.OverlayOpponentScaling = scaling;
            SaveConfig(false);
             _overlay.UpdateScaling();

            if (_config.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
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
            _config.HideTimers = true;
            SaveConfig(true);
        }
        
        private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideTimers = false;
            SaveConfig(true);
        }

        private void SliderTimersHorizontal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.TimersHorizontalPosition = SliderTimersHorizontal.Value;
            SaveConfig(true);
        }

        private void SliderTimersHorizontalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.TimersHorizontalSpacing = SliderTimersHorizontalSpacing.Value;
            SaveConfig(true);
        }

        private void SliderTimersVertical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.TimersVerticalPosition = SliderTimersVertical.Value;
            SaveConfig(true);
        }

        private void SliderTimersVerticalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.TimersVerticalSpacing = SliderTimersVerticalSpacing.Value;
            SaveConfig(true);
        }

        private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            var accent = ComboboxAccent.SelectedItem as Accent;
            if (accent != null)
            {
                ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.DetectAppStyle().Item1);
                _config.AccentName = accent.Name;
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
                _config.ThemeName = theme.Name;
                //if(ComboboxWindowBackground.SelectedItem.ToString() != "Default")
                UpdateAdditionalWindowsBackground();
                SaveConfig(false);
            }
        }

        private void ComboboxWindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
            _config.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
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
                _config.WindowsBackgroundHex = TextboxCustomBackground.Text;
                SaveConfig(false);
            }
        }

        private void ComboboxTextLocationOpponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.TextOnTopOpponent = ComboboxTextLocationOpponent.SelectedItem.ToString() == "Top";

            SaveConfig(false);
            _overlay.SetOpponentTextLocation(_config.TextOnTopOpponent);
            _opponentWindow.SetTextLocation(_config.TextOnTopOpponent);

        }

        private void ComboboxTextLocationPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;

            _config.TextOnTopPlayer = ComboboxTextLocationPlayer.SelectedItem.ToString() == "Top";
            SaveConfig(false);

            _overlay.SetPlayerTextLocation(_config.TextOnTopPlayer);
            _playerWindow.SetTextLocation(_config.TextOnTopPlayer);
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

            _config.SelectedLanguage = selectedLanguage;


            await Restart();
        }

        private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.ExportSetDeckName = true;
            SaveConfig(false);
        }

        private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.ExportSetDeckName = false;
            SaveConfig(false);
        }

        private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.PrioritizeGolden = true;
            SaveConfig(false);
        }

        private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.PrioritizeGolden = false;
            SaveConfig(false);
        }
        private void ComboboxKeyPressGameStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.KeyPressOnGameStart = ComboboxKeyPressGameStart.SelectedValue.ToString();
            SaveConfig(false);
        }

        private void ComboboxKeyPressGameEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.KeyPressOnGameEnd = ComboboxKeyPressGameEnd.SelectedValue.ToString();
            SaveConfig(false);
        }

        private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.HideDecksInOverlay = true;
            SaveConfig(true);
        }

        private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            _config.HideDecksInOverlay = false;
            SaveConfig(true);
        }

        private async void CheckboxAppData_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.SaveInAppData = true;
            SaveConfig(false);
            await Restart();
        }

        private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.SaveInAppData = false;
            SaveConfig(false);
            await Restart();
        }
        private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.ManaCurveMyDecks = true;
            ManaCurveMyDecks.Visibility = Visibility.Visible;
            SaveConfig(false);
        }

        private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.ManaCurveMyDecks = false;
            ManaCurveMyDecks.Visibility = Visibility.Collapsed;
            SaveConfig(false);
        }

        private void CheckboxManaCurveNewDeck_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.ManaCurveNewDeck = true;
            ManaCurveNewDeck.Visibility = Visibility.Visible;
            SaveConfig(false);
        }

        private void CheckboxManaCurveNewDeck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.ManaCurveNewDeck = false;
            ManaCurveNewDeck.Visibility = Visibility.Collapsed;
            SaveConfig(false);
        }

        private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
        {
            //this is probably somehow possible without restarting
            if (!_initialized) return;
            _config.TrackerCardToolTips = true;
            SaveConfig(false);
            await Restart();
        }

        private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
        {
            //this is probably somehow possible without restarting
            if (!_initialized) return;
            _config.TrackerCardToolTips = false;
            SaveConfig(false);
            await Restart();
        }

        private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowCardToolTips = true;
            SaveConfig(false);
        }

        private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowCardToolTips = false;
            SaveConfig(false);
        }

        private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.OverlayCardToolTips = true;
            SaveConfig(true);
        }

        private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.OverlayCardToolTips = false;
            SaveConfig(true);
        }
        #endregion

        private void CheckboxLogGames_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            TextboxLogGamesPath.IsEnabled = true;
            BtnLogGamesSelectDir.IsEnabled = true;
            _config.SavePlayedGames = true;
            if(TextboxLogGamesPath.Text.Length == 0)
                TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);
            SaveConfig(false);
        }

        private void CheckboxLogGames_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            TextboxLogGamesPath.IsEnabled = false;
            BtnLogGamesSelectDir.IsEnabled = false;
            _config.SavePlayedGames = false;
            SaveConfig(false);
        }

        private void BtnLogGamesSelectDir_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TextboxLogGamesPath.Text = folderDialog.SelectedPath;
                _config.SavePlayedGamesPath = folderDialog.SelectedPath;

                TextboxLogGamesPath.BorderBrush =
                    new SolidColorBrush(TextboxLogGamesPath.Text.Length == 0
                                            ? Colors.Red
                                            : SystemColors.ActiveBorderColor);
                SaveConfig(false);
            }
        }


    }
}