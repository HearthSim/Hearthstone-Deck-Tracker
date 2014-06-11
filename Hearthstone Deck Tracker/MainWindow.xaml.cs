#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

#endregion

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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
        private readonly TurnTimer _turnTimer;
        private readonly Thread _updateThread;
        private readonly XmlManager<Decks> _xmlManager;
        private readonly XmlManager<Config> _xmlManagerConfig;
        private readonly DeckImporter _deckImporter;
        private bool _editingDeck;
        private bool _newContainsDeck;
        private Deck _newDeck;



        public MainWindow()
        {
            InitializeComponent();
            
            Helper.CheckForUpdates();

            //check for log config and create if not existing
            try
            {
                if (!File.Exists(_logConfigPath))
                {
                    File.Copy("Files/log.config", _logConfigPath);
                }
                else
                {
                    //update log.config if newer
                    var localFile = new FileInfo(_logConfigPath);
                    var file = new FileInfo("Files/log.config");
                    if (file.LastWriteTime > localFile.LastWriteTime)
                    {
                        File.Copy("Files/log.config", _logConfigPath, true);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n Please restart the tracker as administrator",
                    "Error writing log.config");
                Close();
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n What happend here? ",
                    "Error writing log.config");
                Close();
                return;
            }

            //load config
            _config = new Config();
            _xmlManagerConfig = new XmlManager<Config> {Type = typeof (Config)};
            try
            {
                _config = _xmlManagerConfig.Load("config.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    e.Message + "\n\n" + e.InnerException +
                    "\n\n If you don't know how to fix this, please overwrite config with the default one.",
                    "Error loading config.xml");
                Close();
                return;
            }

            //load saved decks
            if (!File.Exists("PlayerDecks.xml"))
            {
                //avoid overwriting decks file with new releases.
                using (var sr = new StreamWriter("PlayerDecks.xml", false))
                {
                    sr.WriteLine("<Decks></Decks>");
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
                Close();
                return;
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


            //hearthstone, loads db etc
            _hearthstone = new Hearthstone();
            _newDeck = new Deck();
            ListViewNewDeck.ItemsSource = _newDeck.Cards;


            //create overlay
            _overlay = new OverlayWindow(_config, _hearthstone) {Topmost = true};
            _overlay.Show();

            _playerWindow = new PlayerWindow(_config, _hearthstone.PlayerDeck);
            _opponentWindow = new OpponentWindow(_config, _hearthstone.EnemyCards);

            LoadConfig();

            //find hs directory
            if (!File.Exists(_config.HearthstoneDirectory + @"\Hearthstone.exe"))
            {
                MessageBox.Show("Please specify your Hearthstone directory", "Hearthstone directory not found",
                                MessageBoxButton.OK);
                var dialog = new OpenFileDialog();
                dialog.Title = "Select Hearthstone.exe";
                dialog.DefaultExt = "Hearthstone.exe";
                dialog.Filter = "Hearthstone.exe|Hearthstone.exe";
                var result = dialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                _config.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
                _xmlManagerConfig.Save("config.xml", _config);
            }

            _deckImporter = new DeckImporter(_hearthstone);

            //this has to happen before reader starts
            var lastDeck = _deckList.DecksList.FirstOrDefault(d => d.Name == _config.LastDeck);
            DeckPickerList.SelectDeck(lastDeck);


            //log reader
            _logReader = new HsLogReader(_config.HearthstoneDirectory, _config.UpdateDelay);
            _logReader.CardMovement += LogReaderOnCardMovement;
            _logReader.GameStateChange += LogReaderOnGameStateChange;
            _logReader.Analyzing += LogReaderOnAnalyzing;
            _logReader.TurnStart += LogReaderOnTurnStart;

            //_turnTimer = new TurnTimer(90);
            //_turnTimer.TimerTick += TurnTimerOnTimerTick;


            UpdateDbListView();

            _updateThread = new Thread(Update);
            _updateThread.Start();
            //ListboxDecks.SelectedItem =
            //    _deckList.DecksList.FirstOrDefault(d => d.Name != null && d.Name == _config.LastDeck);

            _initialized = true;

            if (lastDeck != null)
            {
                UpdateDeckList(lastDeck);
                UseDeck(lastDeck);
            }


            _logReader.Start();

            
        }

        
        #region LogReader Events

        private void TurnTimerOnTimerTick(TurnTimer sender, TimerEventArgs timerEventArgs)
        {
            //_overlay.Dispatcher.BeginInvoke(new Action(() => _overlay.UpdateTurnTimer(timerEventArgs)));
        }

        private void LogReaderOnTurnStart(HsLogReader sender, TurnStartArgs args)
        {
            //doesn't really matter whose turn it is for now, just restart timer
            //maybe add timer to player/opponent windows
            //_turnTimer.Restart();
        }

        private void LogReaderOnAnalyzing(HsLogReader sender, AnalyzingArgs args)
        {
            if (args.State == AnalyzingState.Start)
            {
                //indicate loading maybe
            }
            else if (args.State == AnalyzingState.End)
            {
                //reader done analyzing new stuff, update things
                if (_overlay.CanvasInfo.IsVisible)
                    _overlay.Dispatcher.BeginInvoke(new Action(() => _overlay.Update(false)));
                if (_playerWindow.IsVisible)
                    _playerWindow.Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                            _playerWindow.SetCardCount(_hearthstone.PlayerHandCount,
                                                       _hearthstone.PlayerDeck.Sum(deckcard => deckcard.Count))));
                if (_opponentWindow.IsVisible)
                    _opponentWindow.Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                            _opponentWindow.SetOpponentCardCount(_hearthstone.EnemyHandCount,
                                                                 30 - _hearthstone.EnemyCards.Sum(c => c.Count) -
                                                                 _hearthstone.EnemyHandCount)
                            ));
            }
        }

        private void LogReaderOnGameStateChange(HsLogReader sender, GameStateArgs args)
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

        private void LogReaderOnCardMovement(HsLogReader sender, CardMovementArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
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
                            HandleOpponentPlay(args.CardId);
                            break;
                        case CardMovementType.OpponentMulligan:
                            HandleOpponentMulligan();
                            break;
                        case CardMovementType.OpponentHandDiscard:
                            HandleOpponentHandDiscard();
                            break;
                        case CardMovementType.OpponentDraw:
                            HandleOpponentDraw();
                            break;
                        case CardMovementType.OpponentDeckDiscard:
                            HandleOpponentDeckDiscard(args.CardId);
                            break;
                        case CardMovementType.OpponentPlayToHand:
                            HandleOpponentPlayToHand(args.CardId);
                            break;
                        case CardMovementType.OpponentGet:
                            HandleOpponentGet(args.CardId);
                            break;
                        default:
                            Console.WriteLine("Invalid card movement");
                            break;
                    }
                }));
        }

        #endregion

        #region Handle Events

        private void HandleGameStart()
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!Hearthstone.IsUsingPremade)
                        _hearthstone.PlayerDeck.Clear();
                    else
                    {
                        var deck = DeckPickerList.SelectedDeck;
                        if (deck != null)
                            _hearthstone.SetPremadeDeck(deck.Cards);
                    }
                    _hearthstone.IsInMenu = false;
                    _hearthstone.PlayerHandCount = 0;
                    _hearthstone.EnemyCards.Clear();
                    _hearthstone.EnemyHandCount = 0;
                    _hearthstone.OpponentDeckCount = 30;
                }));
        }

        private void HandleGameEnd()
        {
            //_turnTimer.Stop();
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!_config.KeepDecksVisible)
                    {
                        if (Hearthstone.IsUsingPremade)
                        {
                            var deck = DeckPickerList.SelectedDeck;
                            if (deck != null)
                                _hearthstone.SetPremadeDeck(deck.Cards);
                        }
                        else
                        {
                            _hearthstone.PlayerDeck.Clear();
                        }

                        _hearthstone.EnemyCards.Clear();
                        _hearthstone.EnemyHandCount = 0;
                        _hearthstone.OpponentDeckCount = 30;
                        _hearthstone.PlayerHandCount = 0;
                    }
                    _hearthstone.IsInMenu = true;
                }));
        }

        private void HandleOpponentGet(string cardId)
        {
            _hearthstone.OpponentGet(cardId);
        }

        private void HandleOpponentPlayToHand(string cardId)
        {
            _hearthstone.OpponentBackToHand(cardId);
        }

        private void HandlePlayerGet(string cardId)
        {
            _hearthstone.PlayerGet(cardId);
        }

        private void HandlePlayerDraw(string cardId)
        {
            _hearthstone.PlayerDraw(cardId);
        }

        private void HandlePlayerMulligan(string cardId)
        {
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
            _hearthstone.PlayerDeckDiscard(cardId);
        }

        private void HandleOpponentSecretTrigger(string cardId)
        {
            _hearthstone.EnemySecretTriggered(cardId);
        }

        private void HandleOpponentPlay(string cardId)
        {
            _hearthstone.EnemyPlayed(cardId);
        }

        private void HandleOpponentMulligan()
        {
            _hearthstone.EnemyMulligan();
        }

        private void HandleOpponentHandDiscard()
        {
            _hearthstone.EnemyHandDiscard();
        }

        private void HandleOpponentDraw()
        {
            _hearthstone.EnemyDraw();
        }

        private void HandleOpponentDeckDiscard(string cardId)
        {
            _hearthstone.EnemyDeckDiscard(cardId);
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
                _overlay.Close();
                _logReader.Stop();
                _updateThread.Abort();
                _playerWindow.Shutdown();
                _opponentWindow.Shutdown();
                _xmlManagerConfig.Save("config.xml", _config);
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

        #endregion

        #region GENERAL METHODS

        private void LoadConfig()
        {
            //var deck = _deckList.DecksList.FirstOrDefault(d => d.Name == _config.LastDeck);
            //if (deck != null && ListboxDecks.Items.Contains(deck))
            //{
            //    ListboxDecks.SelectedItem = deck;
            //}

            // Height = _config.WindowHeight;
            Hearthstone.HighlightCardsInHand = _config.HighlightCardsInHand;
            CheckboxHideOverlayInBackground.IsChecked = _config.HideInBackground;
            CheckboxHideDrawChances.IsChecked = _config.HideDrawChances;
            CheckboxHideEnemyCards.IsChecked = _config.HideEnemyCards;
            CheckboxHideEnemyCardCounter.IsChecked = _config.HideEnemyCardCount;
            CheckboxHidePlayerCardCounter.IsChecked = _config.HidePlayerCardCount;
            CheckboxHideOverlayInMenu.IsChecked = _config.HideInMenu;
            CheckboxHighlightCardsInHand.IsChecked = _config.HighlightCardsInHand;
            CheckboxHideOverlay.IsChecked = _config.HideOverlay;
            CheckboxKeepDecksVisible.IsChecked = _config.KeepDecksVisible;
            CheckboxMinimizeTray.IsChecked = _config.MinimizeToTray;

            RangeSliderPlayer.UpperValue = 100 - _config.PlayerDeckTop;
            RangeSliderPlayer.LowerValue = (100 - _config.PlayerDeckTop) - _config.PlayerDeckHeight;
            SliderPlayer.Value = _config.PlayerDeckLeft;

            RangeSliderOpponent.UpperValue = 100 - _config.OpponentDeckTop;
            RangeSliderOpponent.LowerValue = (100 - _config.OpponentDeckTop) - _config.OpponentDeckHeight;
            SliderOpponent.Value = _config.OpponentDeckLeft;

            SliderOverlayOpacity.Value = _config.OverlayOpacity;
            SliderTimerLeft.Value = _config.TimerLeft;
        }

        private void SortCardCollection(ItemCollection collection)
        {
            var view1 = (CollectionView) CollectionViewSource.GetDefaultView(collection);
            view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
            view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
            view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void Update()
        {
            while (true)
            {
                _overlay.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (Process.GetProcessesByName("Hearthstone").Length == 1)
                        {
                             _overlay.UpdatePosition();
                        }
                        else
                        {
                            _overlay.EnableCanvas(false);
                        }
                    }));

            Thread.Sleep(_config.UpdateDelay);
            }
        }

        #endregion

        #region MY DECKS - GUI

        private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
        {
            //ListboxDecks.SelectedIndex = -1;
            DeckPickerList.SelectedDeck = null;
            UpdateDeckList(new Deck());
            UseDeck(new Deck());
            Hearthstone.IsUsingPremade = false;
        }

        private void BtnEditDeck_Click(object sender, RoutedEventArgs e)
        {
            //if (ListboxDecks.SelectedIndex == -1) return;
            //var selectedDeck = ListboxDecks.SelectedItem as Deck;
            var selectedDeck = DeckPickerList.SelectedDeck;
            if (selectedDeck == null) return;
            //move to new deck section with stuff preloaded
            if (_newContainsDeck)
            {
                //still contains deck, discard?
                var result = MessageBox.Show("New Deck Section still contains an unfinished deck. Discard?",
                                             "Found unfinished deck.", MessageBoxButton.YesNo,
                                             MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
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
            TabControlTracker.SelectedIndex = 1;
        }

        private void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            //var deck = ListboxDecks.SelectedItem as Deck;
            var deck = DeckPickerList.SelectedDeck;
            if (deck != null)
            {
                if (
                    MessageBox.Show("Are you Sure?", "Delete " + deck.Name, MessageBoxButton.YesNo,
                                    MessageBoxImage.Asterisk) ==
                    MessageBoxResult.Yes)
                {
                    try
                    {
                        _deckList.DecksList.Remove(deck);
                        _xmlManager.Save("PlayerDecks.xml", _deckList);
                        //ListboxDecks.SelectedIndex = -1;
                        DeckPickerList.RemoveDeck(deck);
                        ListViewDeck.Items.Clear();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error deleting deck");
                    }
                }
            }
        }
        
        private async void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings();

            var clipboard = Clipboard.GetText();
            if (clipboard.Contains("hearthstats") || clipboard.Contains("hearthpwn"))
            {
                settings.DefaultText = clipboard;
            }

            //import dialog
            var url = await this.ShowInputAsync("Import deck\nCurrently works with:\nhearthstats\nhearthpwn", "Url:", settings);
            if (string.IsNullOrEmpty(url))
                return;

            var controller = await this.ShowProgressAsync("Loading Deck...", "please wait");

            var deck = _deckImporter.Import(url);

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

        #endregion

        #region MY DECKS - METHODS

        private void UseDeck(Deck selected)
        {
            if (selected == null)
                return;
            _hearthstone.SetPremadeDeck(selected.Cards);
            _overlay.Dispatcher.BeginInvoke(new Action(_overlay.SortViews));
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    _hearthstone.PlayerHandCount = 0;
                    _hearthstone.EnemyCards.Clear();
                    _hearthstone.EnemyHandCount = 0;
                    _hearthstone.OpponentDeckCount = 30;
                }));
            _logReader.Reset(false);
        }

        private void UpdateDeckList(Deck selected)
        {
            if (selected == null) return;

            ListViewDeck.Items.Clear();
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
                Hearthstone.IsUsingPremade = true;
                UpdateDeckList(deck);
                UseDeck(deck);
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

        private void BtnClearNewDeck_Click(object sender, RoutedEventArgs e)
        {
            ClearNewDeckSection();
        }

        private void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
        {
            SaveDeck();
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
                    AddCardToDeck(card);
                }
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
                AddCardToDeck(card);
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
                AddCardToDeck(card);
            }
        }

        private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var card = (Card) ListViewDB.SelectedItem;
                if (string.IsNullOrEmpty(card.Name)) return;
                AddCardToDeck(card);
            }
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
                    if (!card.Name.ToLower().Contains(TextBoxDBFilter.Text.ToLower()))
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

        private void SaveDeck()
        {
            if (_newDeck.Cards.Sum(c => c.Count) != 30)
            {
                var result =
                    MessageBox.Show(
                        string.Format("Deck contains {0} cards. Is this what you want to save?",
                                      _newDeck.Cards.Sum(c => c.Count)),
                        "Deck does not contain 30 cards.", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            var deckName = TextBoxDeckName.Text;
            if (string.IsNullOrEmpty(deckName))
            {
                MessageBox.Show("Please set a name for the deck.");
                return;
            }
            if (_deckList.DecksList.Any(d => d.Name == deckName) && !_editingDeck)
            {
                MessageBox.Show("You already have a deck with that name!");
                return;
            }
            if (_editingDeck)
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

            //ListboxDecks.SelectedItem = _deckList.DecksList.First(d => d.Equals(_newDeck));

            ClearNewDeckSection();
        }

        private void ClearNewDeckSection()
        {
            UpdateNewDeckHeader(false);
            ComboBoxSelectClass.SelectedIndex = 0;
            TextBoxDeckName.Text = string.Empty;
            TextBoxDBFilter.Text = string.Empty;
            ComboBoxFilterMana.SelectedIndex = 0;
            _newDeck.Cards.Clear();
            _newDeck.Class = string.Empty;
            _newDeck.Name = string.Empty;
            _newContainsDeck = false;
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
           /* if (!_config.KeepDecksVisible)
            {
                _hearthstone.EnemyCards.Clear();
                _hearthstone.EnemyHandCount = 0;
                _hearthstone.OpponentDeckCount = 30;
            } why is this here!?*/
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

        private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDbListView();
        }

        
        #endregion

        #region OPTIONS

        private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HighlightCardsInHand = true;
            Hearthstone.HighlightCardsInHand = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HighlightCardsInHand = false;
            Hearthstone.HighlightCardsInHand = false;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOverlay = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideOverlay = false;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInMenu = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInMenu = false;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideDrawChances_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideDrawChances = true;
            SaveConfigUpdateOverlay();
            _playerWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playerWindow.LblDrawChance1.Visibility = Visibility.Hidden;
                    _playerWindow.LblDrawChance2.Visibility = Visibility.Hidden;
                }));
        }

        private void CheckboxHideDrawChances_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideDrawChances = false;
            SaveConfigUpdateOverlay();
            _playerWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playerWindow.LblDrawChance1.Visibility = Visibility.Visible;
                    _playerWindow.LblDrawChance2.Visibility = Visibility.Visible;
                }));
        }

        private void CheckboxHidePlayerCardCounter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCardCount = true;
            SaveConfigUpdateOverlay();
            _playerWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playerWindow.LblCardCount.Visibility = Visibility.Hidden;
                    _playerWindow.LblDeckCount.Visibility = Visibility.Hidden;
                }));
        }

        private void CheckboxHidePlayerCardCounter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HidePlayerCardCount = false;
            SaveConfigUpdateOverlay();
            _playerWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _playerWindow.LblCardCount.Visibility = Visibility.Visible;
                    _playerWindow.LblDeckCount.Visibility = Visibility.Visible;
                }));
        }

        private void CheckboxHideEnemyCardCounter_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCardCount = true;
            SaveConfigUpdateOverlay();
            _opponentWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Hidden;
                    _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Hidden;
                }));
        }

        private void CheckboxHideEnemyCardCounter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCardCount = false;
            SaveConfigUpdateOverlay();
            _opponentWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _opponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
                    _opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
                }));
        }

        private void CheckboxHideEnemyCards_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCards = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideEnemyCards_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideEnemyCards = false;
            SaveConfigUpdateOverlay();
        }


        private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInBackground = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.HideInBackground = false;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmost = true;
            _playerWindow.Topmost = true;
            _opponentWindow.Topmost = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.WindowsTopmost = false;
            _playerWindow.Topmost = false;
            _opponentWindow.Topmost = false;
            SaveConfigUpdateOverlay();
        }

        private void SaveConfigUpdateOverlay()
        {
            _xmlManagerConfig.Save("config.xml", _config);
            _overlay.Dispatcher.BeginInvoke(new Action(() => _overlay.Update(true)));
        }

        private void BtnShowWindows_Click(object sender, RoutedEventArgs e)
        {
            //show playeroverlay and enemy overlay
            _playerWindow.Show();
            _playerWindow.Activate();
            _opponentWindow.Show();
            _opponentWindow.Activate();
        }

        private void RangeSliderPlayer_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.PlayerDeckTop = 100 - RangeSliderPlayer.UpperValue;
            _config.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
            //SaveConfigUpdateOverlay();
        }

        private void RangeSliderPlayer_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.PlayerDeckHeight = RangeSliderPlayer.UpperValue - RangeSliderPlayer.LowerValue;
            //SaveConfigUpdateOverlay();
        }

        private void SliderPlayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.PlayerDeckLeft = SliderPlayer.Value;
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderOpponent_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.OpponentDeckTop = 100 - RangeSliderOpponent.UpperValue;
            _config.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
            //SaveConfigUpdateOverlay();
        }

        private void RangeSliderOpponent_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            if (!_initialized) return;
            _config.OpponentDeckHeight = RangeSliderOpponent.UpperValue - RangeSliderOpponent.LowerValue;
            //SaveConfigUpdateOverlay();
        }

        private void SliderOpponent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.OpponentDeckLeft = SliderOpponent.Value;
            SaveConfigUpdateOverlay();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.OverlayOpacity = SliderOverlayOpacity.Value;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.KeepDecksVisible = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.KeepDecksVisible = false;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.MinimizeToTray = true;
            SaveConfigUpdateOverlay();
        }

        private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _config.MinimizeToTray = false;
            SaveConfigUpdateOverlay();
        }

        private void SliderTimerLeft_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized) return;
            _config.TimerLeft = SliderTimerLeft.Value;
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderPlayer_CentralThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderPlayer_LowerThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderPlayer_UpperThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderOpponent_UpperThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

            SaveConfigUpdateOverlay();
        }

        private void RangeSliderOpponent_LowerThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveConfigUpdateOverlay();
        }

        private void RangeSliderOpponent_CentralThumbDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveConfigUpdateOverlay();
        }
        #endregion






    }
}