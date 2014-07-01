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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
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
        private readonly Hearthstone _hearthstone;
        private readonly bool _initialized;

        private readonly string _logConfigPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
            @"\Blizzard\Hearthstone\log.config";

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
        
        
        public MainWindow()
        {
            InitializeComponent();
            var version = Helper.CheckForUpdates(out _newVersion);
            if (version != null)
            {
                TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", version.Major, version.Minor,
                                                     version.Build);
            }
            
            //load config
            _config = new Config();
            _xmlManagerConfig = new XmlManager<Config> {Type = typeof (Config)};
            if (!File.Exists(_config.ConfigPath))
            {
                using (var sr = new StreamWriter(_config.ConfigPath, false))
                {
                    sr.WriteLine("<Config></Config>");
                }
            }
            try
            {
                _config = _xmlManagerConfig.Load("config.xml");
                if(_config.SelectedTags.Count == 0)
                    _config.SelectedTags.Add("All");
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n If you don't know how to fix this, please delete config.xml",
                    "Error loading config.xml");
                Application.Current.Shutdown();
            }
            _config.Debug = IS_DEBUG;

            //find hs directory
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
                            _xmlManagerConfig.Save("config.xml", _config);
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
                    if (!File.Exists(_logConfigPath))
                    {
                        File.Copy("Files/log.config", _logConfigPath);
                        _updatedLogConfig = true;
                    }
                    else
                    {
                        //update log.config if newer
                        var localFile = new FileInfo(_logConfigPath);
                        var file = new FileInfo("Files/log.config");
                        if (file.LastWriteTime > localFile.LastWriteTime)
                        {
                            File.Copy("Files/log.config", _logConfigPath, true);
                            _updatedLogConfig = true;
                        }

                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MessageBox.Show(
                        e.Message + "\n\n" + e.InnerException +
                        "\n\n Please restart the tracker as administrator",
                        "Error writing log.config");
                    Application.Current.Shutdown();
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        e.Message + "\n\n" + e.InnerException +
                        "\n\n What happend here? ",
                        "Error writing log.config");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                BtnExport.IsEnabled = false;
            }


            string languageTag = _config.SelectedLanguage;
            //hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
            _hearthstone = Helper.LanguageDict.ContainsValue(languageTag) ? new Hearthstone(languageTag) : new Hearthstone("enUS");
            _hearthstone.Reset();

            //load saved decks
            if (!File.Exists("PlayerDecks.xml"))
            {
                //avoid overwriting decks file with new releases.
                using (var sr = new StreamWriter("PlayerDecks.xml", false))
                {
                    sr.WriteLine("<Decks></Decks>");
                }
            }
            else
            {
                //the new playerdecks.xml wont work with versions below 0.2.19, make copy
                if (!File.Exists("PlayerDecks_old.xml"))
                {
                    File.Copy("PlayerDecks.xml", "PlayerDecks_old.xml");
                }
            }
            _xmlManager = new XmlManager<Decks> {Type = typeof (Decks)};
            try
            {
                _deckList = _xmlManager.Load("PlayerDecks.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n If you don't know how to fix this, please delete PlayerDecks.xml (this will cause you to lose your decks).",
                    "Error loading PlayerDecks.xml");
                Application.Current.Shutdown();
            }
            foreach (var deck in _deckList.DecksList)
            {
                DeckPickerList.AddDeck(deck);
            }
            DeckPickerList.SelectedDeckChanged += DeckPickerListOnSelectedDeckChanged;
            //ListboxDecks.ItemsSource = _deckList.DecksList;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = new Icon(@"Images/HearthstoneDeckTracker.ico");
            _notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
            _notifyIcon.Visible = false;


            _newDeck = new Deck();
            ListViewNewDeck.ItemsSource = _newDeck.Cards;

            //create overlay
            _overlay = new OverlayWindow(_config, _hearthstone) { Topmost = true };
            if (_foundHsDirectory)
            {
                _overlay.Show();
            }
            _playerWindow = new PlayerWindow(_config, _hearthstone.IsUsingPremade ? _hearthstone.PlayerDeck : _hearthstone.PlayerDrawn);
            _opponentWindow = new OpponentWindow(_config, _hearthstone.EnemyCards);
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
                _xmlManager.Save("PlayerDecks.xml", _deckList);
            }
            if (!_deckList.AllTags.Contains("Arena"))
            {
                _deckList.AllTags.Add("Arena");
                _xmlManager.Save("PlayerDecks.xml", _deckList);
            }
            if (!_deckList.AllTags.Contains("Constructed"))
            {
                _deckList.AllTags.Add("Constructed");
                _xmlManager.Save("PlayerDecks.xml", _deckList);
            }

            ComboboxAccent.ItemsSource = ThemeManager.Accents;
            ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
            ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

            LoadConfig();

            _deckImporter = new DeckImporter(_hearthstone);
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
            //why does this need invoke?
            _overlay.Dispatcher.BeginInvoke(new Action(() => _overlay.UpdateTurnTimer(timerEventArgs)));
            _timerWindow.Dispatcher.BeginInvoke(new Action(() => _timerWindow.Update(timerEventArgs)));

        }

        private void LogReaderOnCardPosChange(HsLogReader sender, CardPosChangeArgs args)
        {
            switch (args.Action)
            {
                case OpponentHandMovement.Draw:
                    _hearthstone.OpponentDraw(args);
                    break;
                case OpponentHandMovement.Play:
                    _hearthstone.OpponentPlay(args);
                    break;
                case OpponentHandMovement.Mulligan:
                    HandleOpponentMulligan(args.From);
                    break;
                case OpponentHandMovement.FromPlayerDeck:
                    _hearthstone.OpponentGet(args.Turn);
                    break;
            }
        }

        private void LogReaderOnTurnStart(HsLogReader sender, TurnStartArgs args)
        {
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
                    _playerWindow.SetCardCount(_hearthstone.PlayerHandCount,
                                               30 - _hearthstone.PlayerDrawn.Sum(card => card.Count));

                if (_opponentWindow.IsVisible)
                    _opponentWindow.SetOpponentCardCount(_hearthstone.EnemyHandCount,
                                                         _hearthstone.OpponentDeckCount, _hearthstone.OpponentHasCoin);


                if (_showIncorrectDeckMessage && !_showingIncorrectDeckMessage)
                {
                    _showingIncorrectDeckMessage = true;

                    ShowIncorrectDeckMessage();
                    //stuff
                }
                
            }
        }

        private void LogReaderOnGameStateChange(HsLogReader sender, GameStateArgs args)
        {
            if (!string.IsNullOrEmpty(args.PlayerHero))
            {
                _hearthstone.PlayingAs = args.PlayerHero;

            }
            if (!string.IsNullOrEmpty(args.OpponentHero))
            {
                _hearthstone.PlayingAgainst = args.OpponentHero;
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
                    Console.WriteLine("Invalid card movement");
                    break;
            }
        }

        #endregion

        #region Handle Events

        private void HandleGameStart()
        {
            //avoid new game being started when jaraxxus is played
            if (!_hearthstone.IsInMenu) return;


            var selectedDeck = DeckPickerList.SelectedDeck;
            if (selectedDeck != null)
                _hearthstone.SetPremadeDeck((Deck)selectedDeck.Clone());

            _hearthstone.IsInMenu = false;
            _hearthstone.Reset();

            //select deck based on hero
            if (!string.IsNullOrEmpty(_hearthstone.PlayingAs))
            {
                if (!_hearthstone.IsUsingPremade || !_config.AutoDeckDetection) return;
                
                if (selectedDeck == null || selectedDeck.Class != _hearthstone.PlayingAs)
                {

                    var classDecks = _deckList.DecksList.Where(d => d.Class == _hearthstone.PlayingAs).ToList();
                    if (classDecks.Count == 0)
                    {
                        Debug.WriteLine("Found no deck to switch to", "HandleGameStart");
                        return;
                    }
                    if (classDecks.Count == 1)
                    {
                        DeckPickerList.SelectDeck(classDecks[0]);
                        Debug.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
                    }
                    else if (_deckList.LastDeckClass.Any(ldc => ldc.Class == _hearthstone.PlayingAs))
                    {
                        var lastDeckName = _deckList.LastDeckClass.First(ldc => ldc.Class == _hearthstone.PlayingAs).Name;
                        Debug.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

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
            _turnTimer.Stop();
            _overlay.HideTimers();
            if (!_config.KeepDecksVisible)
            {
                var deck = DeckPickerList.SelectedDeck;
                if (deck != null)
                    _hearthstone.SetPremadeDeck((Deck)deck.Clone());

                _hearthstone.Reset();
            }
            _hearthstone.IsInMenu = true;
        }

        private void HandleOpponentPlayToHand(string cardId, int turn)
        {
            _hearthstone.OpponentBackToHand(cardId, turn);
        }

        private void HandlePlayerGet(string cardId)
        {
            _hearthstone.PlayerGet(cardId);
        }

        private void HandlePlayerDraw(string cardId)
        {
           var correctDeck = _hearthstone.PlayerDraw(cardId);

            if (!correctDeck && _config.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage && _hearthstone.IsUsingPremade)
            {
                _showIncorrectDeckMessage = true;
                Debug.WriteLine("Found incorrect deck", "HandlePlayerDraw");
            }
        }


        private void HandlePlayerMulligan(string cardId)
        {
            Debug.WriteLine("HandlePlayerMulligan");
            _turnTimer.MulliganDone(Turn.Player);
            _hearthstone.Mulligan(cardId);
        }

        private void HandlePlayerHandDiscard(string cardId)
        {
            _hearthstone.PlayerHandDiscard(cardId);
        }

        private void HandlePlayerPlay(string cardId)
        {
            _hearthstone.PlayerPlayed(cardId);
        }

        private void HandlePlayerDeckDiscard(string cardId)
        {
            var correctDeck = _hearthstone.PlayerDeckDiscard(cardId);
            
            //don't think this will ever detect an incorrect deck but who knows...
            if (!correctDeck && _config.AutoDeckDetection && !_showIncorrectDeckMessage && !_showingIncorrectDeckMessage && _hearthstone.IsUsingPremade)
            {
                _showIncorrectDeckMessage = true;
                Debug.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
            }
        }

        private void HandleOpponentSecretTrigger(string cardId)
        {
            _hearthstone.OpponentSecretTriggered(cardId);
        }

        private void HandleOpponentMulligan(int pos)
        {
            _turnTimer.MulliganDone(Turn.Opponent);
            _hearthstone.OpponentMulligan(pos);
        }
        
        private void HandleOpponentDeckDiscard(string cardId)
        {
            _hearthstone.OpponentDeckDiscard(cardId);
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
                _xmlManagerConfig.Save("config.xml", _config);
                _xmlManager.Save("PlayerDecks.xml", _deckList);
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
            _xmlManagerConfig.Save("config.xml", _config);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        private void MetroWindow_LocationChanged(object sender, EventArgs e)
        {
            _config.TrackerWindowTop = (int)Top;
            _config.TrackerWindowLeft = (int)Left;
        }

        #endregion

        #region GENERAL METHODS

        private void ShowIncorrectDeckMessage()
        {

            var decks =
                _deckList.DecksList.Where(
                    d => d.Class == _hearthstone.PlayingAs && _hearthstone.PlayerDrawn.All(c => d.Cards.Contains(c)))
                         .ToList();
            if (decks.Contains(DeckPickerList.SelectedDeck))
                decks.Remove(DeckPickerList.SelectedDeck);

            Debug.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
            if (decks.Count > 0)
            {

                DeckSelectionDialog dsDialog = new DeckSelectionDialog(decks);

                //todo: System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=ClassColor; DataItem=null; target element is 'GradientStop' (HashCode=7260326); target property is 'Color' (type 'Color')
                //when opened for seconds time. why?
                dsDialog.ShowDialog();
                
                    

                var selectedDeck = dsDialog.SelectedDeck;

                if (selectedDeck != null)
                {
                    Debug.WriteLine("Selected deck: " + selectedDeck.Name);
                    DeckPickerList.SelectDeck(selectedDeck);
                    UpdateDeckList(selectedDeck);
                    UseDeck(selectedDeck);
                }
                else
                {
                    Debug.WriteLine("No deck selected. disabled deck detection.");
                    CheckboxDeckDetection.IsChecked = false;
                    SaveConfig(false);
                }
            }

            _showingIncorrectDeckMessage = false;
            _showIncorrectDeckMessage = false;
        }

        private void LoadConfig()
        {
            if (_config.TrackerWindowTop != -32000 && _config.TrackerWindowTop != -1)
                Top = _config.TrackerWindowTop;
            if (_config.TrackerWindowLeft != -32000 && _config.TrackerWindowLeft != -1)
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

            Height = _config.WindowHeight;
            Hearthstone.HighlightCardsInHand = _config.HighlightCardsInHand;
            CheckboxHideOverlayInBackground.IsChecked = _config.HideInBackground;
            CheckboxHideDrawChances.IsChecked = _config.HideDrawChances;
            CheckboxHideOpponentDrawChances.IsChecked = _config.HideOpponentDrawChances;
            CheckboxHideEnemyCards.IsChecked = _config.HideEnemyCards;
            CheckboxHideEnemyCardCounter.IsChecked = _config.HideEnemyCardCount;
            CheckboxHideOpponentCardAge.IsChecked = _config.HideOpponentCardAge;
            CheckboxHidePlayerCardCounter.IsChecked = _config.HidePlayerCardCount;
            CheckboxHideOverlayInMenu.IsChecked = _config.HideInMenu;
            CheckboxHighlightCardsInHand.IsChecked = _config.HighlightCardsInHand;
            CheckboxHideOverlay.IsChecked = _config.HideOverlay;
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
                    _xmlManagerConfig.Save("config.xml", _config);
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
        #endregion

        #region MY DECKS - GUI

        private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("set player item source as drawn");
            _overlay.ListViewPlayer.ItemsSource = _hearthstone.PlayerDrawn;
            _playerWindow.ListViewPlayer.ItemsSource = _hearthstone.PlayerDrawn;
            _hearthstone.IsUsingPremade = false;
            DeckPickerList.SelectedDeck = null;
            DeckPickerList.SelectedIndex = -1;
            UpdateDeckList(null);
            UseDeck(null);
        }

        private async void BtnEditDeck_Click(object sender, RoutedEventArgs e)
        {
            //if (ListboxDecks.SelectedIndex == -1) return;
            //var selectedDeck = ListboxDecks.SelectedItem as Deck;
            var selectedDeck = DeckPickerList.SelectedDeck;
            if (selectedDeck == null) return;
            //move to new deck section with stuff preloaded
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
            //var deck = ListboxDecks.SelectedItem as Deck;
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
                        _xmlManager.Save("PlayerDecks.xml", _deckList);
                        DeckPickerList.RemoveDeck(deck);
                        ListViewDeck.Items.Clear();
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Error deleting deck");
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
                _xmlManager.Save("PlayerDecks.xml", _deckList);
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

                _xmlManager.Save("PlayerDecks.xml", _deckList);
                TagControlFilter.LoadTags(_deckList.AllTags);
                DeckPickerList.UpdateList();
            }
        }

        private void TagControlSetOnSelectedTagsChanged(TagControl sender, List<string> tags)
        {
            if (_newDeck == null) return;
            _newDeck.Tags = new List<string>(tags);
            BtnSaveDeck.Content = "Save*";
        }

        private void BtnNotes_Click(object sender, RoutedEventArgs e)
        {
            FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
        }

        private async void ListViewDeck_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source == null) return;

            double dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            double dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

            //increase height to get full list rendered
            var previousHeight = Height;
            Height = 3000;
            var success = Helper.ScreenshotDeck(ListViewDeck, dpiX, dpiY, DeckPickerList.SelectedDeck.Name);
            Height = previousHeight;

            if (!success)
            {
                await
                    this.ShowMessageAsync("Error saving screenshot", "");
            }
        }
        #endregion

        #region MY DECKS - METHODS

        private void UseDeck(Deck selected)
        {
            _hearthstone.Reset();

            if (selected != null)
                _hearthstone.SetPremadeDeck((Deck)selected.Clone());

            _logReader.Reset(true);

            _overlay.SortViews();

        }

        private void UpdateDeckList(Deck selected)
        {
            ListViewDeck.Items.Clear();
            if (selected == null)
            {

                _config.LastDeck = string.Empty;
                _xmlManagerConfig.Save("config.xml", _config);
                return;
            }
            foreach (var card in selected.Cards)
            {
                ListViewDeck.Items.Add(card);
            }

            SortCardCollection(ListViewDeck.Items);
            _config.LastDeck = selected.Name;
            _xmlManagerConfig.Save("config.xml", _config);
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
                if (_overlay.ListViewPlayer.ItemsSource != _hearthstone.PlayerDeck)
                {
                    _overlay.ListViewPlayer.ItemsSource = _hearthstone.PlayerDeck;
                    _playerWindow.ListViewPlayer.ItemsSource = _hearthstone.PlayerDeck;
                    Debug.WriteLine("Set player itemsource as playerdeck");
                }
                _hearthstone.IsUsingPremade = true;
                UpdateDeckList(deck);
                UseDeck(deck);

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
                _deckList.LastDeckClass.Add(new DeckInfo(){Class = deck.Class, Name = deck.Name});
                _xmlManager.Save("PlayerDecks.xml", _deckList);
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

            var clipboard = Clipboard.GetText();
            if (clipboard.Contains("hearthstats") || clipboard.Contains("hearthpwn") || 
                clipboard.Contains("hearthhead") || clipboard.Contains("hearthstoneplayers") ||
                clipboard.Contains("tempostorm") || clipboard.Contains("hearthstonetopdeck"))
            {
                settings.DefaultText = clipboard;
            }

            //import dialog
            var url = await this.ShowInputAsync("Import deck", "Currently supported:\nhearthstats, hearthpwn, hearthhead*, hearthstoneplayers, hearthstonetopdeck and tempostorm\n\n*doesn't works 100%, just retry a few times\n\nUrl:", settings);
            if (string.IsNullOrEmpty(url))
                return;

            var controller = await this.ShowProgressAsync("Loading Deck...", "please wait");

            var deck = await _deckImporter.Import(url);

            await controller.CloseAsync();

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
                    var card = _hearthstone.GetCardFromName(line.Trim());
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

                foreach (var card in _hearthstone.GetActualCards())
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
            
            var newDeckClone = (Deck) _newDeck.Clone();
            _deckList.DecksList.Add(newDeckClone);
            DeckPickerList.AddAndSelectDeck(newDeckClone);
            
            _xmlManager.Save("PlayerDecks.xml", _deckList);
            BtnSaveDeck.Content = "Save";

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
        }

        private void RemoveCardFromDeck(Card card)
        {
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
            Hearthstone.HighlightCardsInHand = true;
            SaveConfig(true);
        }

        private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HighlightCardsInHand = false;
            Hearthstone.HighlightCardsInHand = false;
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

        private void CheckboxHideEnemyCardCounter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCardCount = true;
            SaveConfig(true);
                    _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Collapsed;
                    _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Collapsed;
        }

        private void CheckboxHideEnemyCardCounter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCardCount = false;
            SaveConfig(true);
                    _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
                    _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
        }

        private void CheckboxHideEnemyCards_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCards = true;
            SaveConfig(true);
        }

        private void CheckboxHideEnemyCards_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCards = false;
            SaveConfig(true);
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

            _playerWindow.SetCardCount(_hearthstone.PlayerHandCount,
                                       30 - _hearthstone.PlayerDrawn.Sum(card => card.Count));

            _opponentWindow.SetOpponentCardCount(_hearthstone.EnemyHandCount,
                                                 _hearthstone.OpponentDeckCount, _hearthstone.OpponentHasCoin);

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
            _xmlManagerConfig.Save("config.xml", _config);
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
        #endregion


    }
}