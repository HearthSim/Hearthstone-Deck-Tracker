#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
    public class GameEventHandler : IGameHandler
    {
        private readonly GameV2 _game;
        private const int MaxCardsOnCollectionPage = 8;
        private DateTime _lastGameStart;
        private Deck _assignedDeck;
        private GameStats _lastGame;
        private bool _showedNoteDialog;
        private bool _doneImportingConstructed;
        private List<string> _ignoreCachedIds;
        private DateTime _lastArenaReward = DateTime.MinValue;
        private int _lastCachedManaCost;
        private int _lastManaCost;
        private bool _startImportingCached;
        private int _unloadedCardCount;
        private bool _handledGameEnd;
        private bool _handledGameResult;

        public GameEventHandler(GameV2 game)
        {
            _game = game;
        }

        public bool RecordCurrentGameMode
        {
            get
            {
                return _game.CurrentGameMode == GameMode.None && Config.Instance.RecordOther
                       || _game.CurrentGameMode == GameMode.Practice && Config.Instance.RecordPractice
                       || _game.CurrentGameMode == GameMode.Arena && Config.Instance.RecordArena
                       || _game.CurrentGameMode == GameMode.Brawl && Config.Instance.RecordBrawl
                       || _game.CurrentGameMode == GameMode.Ranked && Config.Instance.RecordRanked
                       || _game.CurrentGameMode == GameMode.Friendly && Config.Instance.RecordFriendly
                       || _game.CurrentGameMode == GameMode.Casual && Config.Instance.RecordCasual
                       || _game.CurrentGameMode == GameMode.Spectator && Config.Instance.RecordSpectator;
            }
        }

        public void ResetConstructedImporting()
        {
            Logger.WriteLine("Reset constructed importing", "GameEventHandler");
            _doneImportingConstructed = false;
            _lastManaCost = 0;
            _unloadedCardCount = 0;
            _ignoreCachedIds = new List<string>(Config.Instance.ConstructedImportingIgnoreCachedIds);
            _game.ResetConstructedCards();
        }

        public void HandlePossibleConstructedCard(string id, bool canBeDoneImporting)
        {
            if (_doneImportingConstructed)
                return;
            var card = GameV2.GetCardFromId(id);
            if (card == null || !GameV2.IsActualCard(card))
                return;
            if (canBeDoneImporting)
            {
                _unloadedCardCount++;
                var containsOtherThanDruid =
                    _game.PossibleConstructedCards.Any(c => !string.IsNullOrEmpty(c.PlayerClass) && c.PlayerClass != "Druid");
                var cardCount =
                    _game.PossibleConstructedCards.Where(c => !Config.Instance.ConstructedImportingIgnoreCachedIds.Contains(c.Id))
                        .Count(c => (!containsOtherThanDruid || c.PlayerClass != "Druid"));
                if (_unloadedCardCount > MaxCardsOnCollectionPage && card.Cost < _lastManaCost && cardCount > 10)
                {
                    _doneImportingConstructed = true;
                    return;
                }
                _lastManaCost = card.Cost;
            }
            else
            {
                if (Helper.SettingUpConstructedImporting)
                {
                    if (!_game.PossibleConstructedCards.Contains(card))
                        _game.PossibleConstructedCards.Add(card);
                    return;
                }
                if (_ignoreCachedIds.Contains(card.Id))
                {
                    _ignoreCachedIds.Remove(card.Id);
                    return;
                }
            }
            if (!_game.PossibleConstructedCards.Contains(card))
                _game.PossibleConstructedCards.Add(card);
        }

        public void HandlePossibleArenaCard(string id)
        {
            var card = GameV2.GetCardFromId(id);
            if (!GameV2.IsActualCard(card))
                return;
            if (!_game.PossibleArenaCards.Contains(card))
                _game.PossibleArenaCards.Add(card);
        }

        public void HandleInMenu()
        {
            if (_game.IsInMenu)
                return;

            if (Config.Instance.RecordReplays && _game.Entities.Count > 0 && !_game.SavedReplay && _game.CurrentGameStats != null
               && _game.CurrentGameStats.ReplayFile == null && RecordCurrentGameMode)
                _game.CurrentGameStats.ReplayFile = ReplayMaker.SaveToDisk();

            SaveAndUpdateStats();

            _game.IsInMenu = true;
            TurnTimer.Instance.Stop();
            Helper.MainWindow.Overlay.HideTimers();
            Helper.MainWindow.Overlay.HideSecrets();
            if (Config.Instance.KeyPressOnGameEnd != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
            {
                SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
                Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd, "GameEventHandler");
            }
            if (!Config.Instance.KeepDecksVisible)
            {
                var deck = DeckList.Instance.ActiveDeckVersion;
                if (deck != null)
                    _game.SetPremadeDeck((Deck)deck.Clone());
            }
            if (!_game.IsUsingPremade)
                _game.DrawnLastGame = new List<Card>(_game.PlayerDrawn);
            HsLogReaderV2.Instance.ClearLog();
            if (!Config.Instance.KeepDecksVisible)
                _game.Reset(false);
            if (_game.CurrentGameStats != null && _game.CurrentGameStats.Result != GameResult.None)
                _game.CurrentGameStats = null;
            if (_game.CurrentGameMode == GameMode.Spectator)
                SetGameMode(GameMode.None);
            GameEvents.OnInMenu.Execute();
        }

        public void HandleConcede()
        {
            if (_game.CurrentGameStats == null)
                return;
            _game.CurrentGameStats.WasConceded = true;
        }

        public void SetOpponentHero(string hero)
        {
            if (string.IsNullOrEmpty(hero))
                return;
            _game.PlayingAgainst = hero;

            if (_game.CurrentGameStats != null)
                _game.CurrentGameStats.OpponentHero = hero;
            Logger.WriteLine("Playing against " + hero, "GameEventHandler");

            HeroClass heroClass;
            if (Enum.TryParse(hero, true, out heroClass))
                _game.OpponentSecrets.HeroClass = heroClass;
        }

        public void SetPlayerHero(string hero)
        {
            try
            {
                if (!string.IsNullOrEmpty(hero))
                {
                    //_game.PlayingAs = hero;
                    if (_game.CurrentGameStats != null)
                        _game.CurrentGameStats.PlayerHero = hero;
                    var selectedDeck = DeckList.Instance.ActiveDeckVersion;
                    if (!_game.IsUsingPremade || !Config.Instance.AutoDeckDetection)
                        return;

                    if (selectedDeck == null || selectedDeck.Class != _game.PlayingAs)
                    {
                        var classDecks = DeckList.Instance.Decks.Where(d => d.Class == _game.PlayingAs && !d.Archived).ToList();
                        if (classDecks.Count == 0)
                            Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
                        else if (classDecks.Count == 1)
                        {
                            Helper.MainWindow.DeckPickerList.SelectDeck(classDecks[0]);
                            Helper.MainWindow.DeckPickerList.RefreshDisplayedDecks();
                            Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
                        }
                        else if (DeckList.Instance.LastDeckClass.Any(ldc => ldc.Class == _game.PlayingAs))
                        {
                            var lastDeck = DeckList.Instance.LastDeckClass.First(ldc => ldc.Class == _game.PlayingAs);

                            var deck = lastDeck.Id == Guid.Empty
                                           ? DeckList.Instance.Decks.FirstOrDefault(d => d.Name == lastDeck.Name)
                                           : DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == lastDeck.Id);
							if( deck != null && _game.PlayerDrawnIdsTotal.Distinct().All(id => deck.GetSelectedDeckVersion().Cards.Any(c => id == c.Id)))
							{
								Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeck.Name, "HandleGameStart");
								if (deck.Archived)
                                {
                                    Logger.WriteLine("Deck " + deck.Name + " is archived - waiting for deck selection dialog", "HandleGameStart");
                                    return;
                                }

                                Helper.MainWindow.NeedToIncorrectDeckMessage = false;
                                Helper.MainWindow.DeckPickerList.SelectDeck(deck);
                                Helper.MainWindow.UpdateDeckList(deck);
                                Helper.MainWindow.UseDeck(deck);
                                Helper.MainWindow.DeckPickerList.RefreshDisplayedDecks();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.WriteLine("Error setting player hero: " + exception, "GameEventHandler");
            }
        }

        public void TurnStart(ActivePlayer player, int turnNumber)
        {
            Logger.WriteLine(string.Format("{0}-turn ({1})", player, turnNumber + 1), "GameEventHandler");
            //doesn't really matter whose turn it is for now, just restart timer
            //maybe add timer to player/opponent windows
            TurnTimer.Instance.SetCurrentPlayer(player);
            TurnTimer.Instance.Restart();
            if (player == ActivePlayer.Player && !_game.IsInMenu)
            {
                if (Config.Instance.FlashHsOnTurnStart)
                    User32.FlashHs();

                if (Config.Instance.BringHsToForeground)
                    User32.BringHsToForeground();
            }
            GameEvents.OnTurnStart.Execute(player);
        }

        public void HandleGameStart()
        {
            if (DateTime.Now - _lastGameStart < new TimeSpan(0, 0, 0, 5)) //game already started
                return;
            _handledGameEnd = false;
            _handledGameResult = false;
            _lastGameStart = DateTime.Now;
            Logger.WriteLine("Game start", "GameEventHandler");

            if (Config.Instance.FlashHsOnTurnStart)
                User32.FlashHs();
            if (Config.Instance.BringHsToForeground)
                User32.BringHsToForeground();

            if (Config.Instance.KeyPressOnGameStart != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
            {
                SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
                Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart, "GameEventHandler");
            }
            _showedNoteDialog = false;
            _game.IsInMenu = false;
            _game.Reset();

            var selectedDeck = DeckList.Instance.ActiveDeckVersion;
            if (selectedDeck != null)
                _game.SetPremadeDeck((Deck)selectedDeck.Clone());
            GameEvents.OnGameStart.Execute();
        }
#pragma warning disable 4014
        public async void HandleGameEnd()
        {
            Helper.MainWindow.Overlay.HideTimers();
            if (_game.CurrentGameStats == null || _handledGameEnd)
                return;
            if (_game.CurrentGameMode == GameMode.Spectator && !Config.Instance.RecordSpectator)
            {
                Logger.WriteLine("Game is in Spectator mode, discarded. (Record Spectator disabled)", "GameEventHandler");
                _assignedDeck = null;
                return;
            }
            var player = _game.Entities.FirstOrDefault(e => e.Value.IsPlayer);
            var opponent = _game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && !e.Value.IsPlayer);
            if (player.Value != null)
                _game.CurrentGameStats.PlayerName = player.Value.Name;
            if (opponent.Value != null && CardIds.HeroIdDict.ContainsValue(_game.CurrentGameStats.OpponentHero))
                _game.CurrentGameStats.OpponentName = opponent.Value.Name;
            else
                _game.CurrentGameStats.OpponentName = _game.CurrentGameStats.OpponentHero;

            _game.CurrentGameStats.Turns = HsLogReaderV2.Instance.GetTurnNumber();
            if (Config.Instance.DiscardZeroTurnGame && _game.CurrentGameStats.Turns < 1)
            {
                Logger.WriteLine("Game has 0 turns, discarded. (DiscardZeroTurnGame)", "GameEventHandler");
                _assignedDeck = null;
                GameEvents.OnGameEnd.Execute();
                return;
            }
            _game.CurrentGameStats.GameEnd();
            GameEvents.OnGameEnd.Execute();
            var selectedDeck = DeckList.Instance.ActiveDeck;
            if (selectedDeck != null)
            {
                if (Config.Instance.DiscardGameIfIncorrectDeck
                   && !_game.PlayerDrawn.All(
                                            c =>
                                            c.IsStolen
                                            || selectedDeck.GetSelectedDeckVersion().Cards.Any(c2 => c.Id == c2.Id && c.Count <= c2.Count)))
                {
                    if (Config.Instance.AskBeforeDiscardingGame)
                    {
                        var discardDialog = new DiscardGameDialog(_game.CurrentGameStats);
                        discardDialog.Topmost = true;
                        discardDialog.ShowDialog();
                        if (discardDialog.Result == DiscardGameDialogResult.Discard)
                        {
                            Logger.WriteLine("Assigned current game to NO deck - selected deck does not match cards played (dialogresult: discard)",
                                             "GameEventHandler");
                            _game.CurrentGameStats.DeleteGameFile();
                            _assignedDeck = null;
                            return;
                        }
                        if (discardDialog.Result == DiscardGameDialogResult.MoveToOther)
                        {
                            var moveDialog = new MoveGameDialog(DeckList.Instance.Decks.Where(d => d.Class == _game.CurrentGameStats.PlayerHero));
                            moveDialog.Topmost = true;
                            moveDialog.ShowDialog();
                            var targetDeck = moveDialog.SelectedDeck;
                            if (targetDeck != null)
                            {
                                selectedDeck = targetDeck;
                                _game.CurrentGameStats.PlayerDeckVersion = moveDialog.SelectedVersion;
                                _game.CurrentGameStats.HearthStatsDeckVersionId = targetDeck.GetVersion(moveDialog.SelectedVersion).HearthStatsDeckVersionId;
                                //...continue as normal
                            }
                            else
                            {
                                Logger.WriteLine("No deck selected in move game dialog after discard dialog, discarding game", "GameEventHandler");
                                _game.CurrentGameStats.DeleteGameFile();
                                _assignedDeck = null;
                                return;
                            }
                        }
                    }
                    else
                    {
                        Logger.WriteLine("Assigned current game to NO deck - selected deck does not match cards played (no dialog)", "GameEventHandler");
                        _game.CurrentGameStats.DeleteGameFile();
                        _assignedDeck = null;
                        return;
                    }
                }
                else
                {
                    _game.CurrentGameStats.PlayerDeckVersion = DeckList.Instance.ActiveDeckVersion.Version;
                    _game.CurrentGameStats.HearthStatsDeckVersionId = DeckList.Instance.ActiveDeckVersion.HearthStatsDeckVersionId;
                }

                _lastGame = _game.CurrentGameStats;
                selectedDeck.DeckStats.AddGameResult(_lastGame);
                selectedDeck.StatsUpdated();
                if (Config.Instance.ShowNoteDialogAfterGame && !Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
                {
                    _showedNoteDialog = true;
                    new NoteDialog(_game.CurrentGameStats);
                }
                Logger.WriteLine("Assigned current game to deck: " + selectedDeck.Name, "GameStats");
                _assignedDeck = selectedDeck;

                // Unarchive the active deck after we have played a game with it
                if (_assignedDeck.Archived)
                {
                    Logger.WriteLine("Automatically unarchiving deck " + selectedDeck.Name + " after assigning current game", "GameEventHandler");
                    Helper.MainWindow.ArchiveDeck(_assignedDeck, false);
                }

                if (HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewGames)
                {
                    if (_game.CurrentGameMode == GameMode.None)
                        await GameModeDetection(300); //give the user 5 minutes to get out of the victory/defeat screen
                    if (_game.CurrentGameMode == GameMode.Casual)
                        await HsLogReaderV2.Instance.RankedDetection();
                    if (_game.CurrentGameMode == GameMode.Ranked && !_lastGame.HasRank)
                        await RankDetection(5);
                    await GameModeSaved(15);
                    if (_game.CurrentGameMode == GameMode.Arena)
                        HearthStatsManager.UploadArenaMatchAsync(_lastGame, selectedDeck, background: true);
                    if (_game.CurrentGameMode == GameMode.Brawl)
                    { /* do nothing */ }
                    else
                        HearthStatsManager.UploadMatchAsync(_lastGame, selectedDeck, background: true);
                }
                _lastGame = null;
            }
            else
            {
                DefaultDeckStats.Instance.GetDeckStats(_game.PlayingAs).AddGameResult(_game.CurrentGameStats);
                Logger.WriteLine(string.Format("Assigned current deck to default {0} deck.", _game.PlayingAs), "GameStats");
                _assignedDeck = null;
            }
        }
#pragma warning restore 4014
        private async Task RankDetection(int timeoutInSeconds)
        {
            Logger.WriteLine("waiting for rank detection", "GameEventHandler");
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            while (_lastGame != null && !_lastGame.HasRank && (DateTime.Now - startTime) < timeout)
                await Task.Delay(100);
        }

        private async Task GameModeDetection(int timeoutInSeconds)
        {
            Logger.WriteLine("waiting for game mode detection", "GameEventHandler");
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            while (_game.CurrentGameMode == GameMode.None && (DateTime.Now - startTime) < timeout)
                await Task.Delay(100);
        }

        private async Task GameModeSaved(int timeoutInSeconds)
        {
            Logger.WriteLine("waiting for game mode to be saved to game", "GameEventHandler");
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            while (_lastGame != null && _lastGame.GameMode == GameMode.None && (DateTime.Now - startTime) < timeout)
                await Task.Delay(100);
        }

        private void LogEvent(string type, string id = "", int turn = 0, int from = -1)
        {
            Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", type, id, turn, from), "GameEventHandler");
        }

        public void PlayerSetAside(string id)
        {
            _game.SetAsideCards.Add(id);
            Logger.WriteLine("set aside: " + id, "GameEventHandler");
        }

        public void HandleWin()
        {
            if (_game.CurrentGameStats == null || _handledGameResult)
                return;
            Logger.WriteLine("Game was won!", "GameEventHandler");
            _game.CurrentGameStats.Result = GameResult.Win;
            GameEvents.OnGameWon.Execute();
        }

        public void HandleLoss()
        {
            if (_game.CurrentGameStats == null || _handledGameResult)
                return;
            Logger.WriteLine("Game was lost!", "GameEventHandler");
            _game.CurrentGameStats.Result = GameResult.Loss;
            GameEvents.OnGameLost.Execute();
        }

        public void HandleTied()
        {
            if (_game.CurrentGameStats == null || _handledGameResult)
                return;
            Logger.WriteLine("Game was a tie!", "GameEventHandler");
            _game.CurrentGameStats.Result = GameResult.Draw;
            GameEvents.OnGameTied.Execute();
        }

        public void SetGameMode(GameMode mode)
        {
            _game.CurrentGameMode = mode;
        }

        private void SaveAndUpdateStats()
        {
            var statsControl = Config.Instance.StatsInWindow ? Helper.MainWindow.StatsWindow.StatsControl : Helper.MainWindow.DeckStatsFlyout;
            if (RecordCurrentGameMode)
            {
                if (Config.Instance.ShowNoteDialogAfterGame && Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
                {
                    _showedNoteDialog = true;
                    new NoteDialog(_game.CurrentGameStats);
                }

                if (_game.CurrentGameStats != null)
                {
                    _game.CurrentGameStats.Turns = HsLogReaderV2.Instance.GetTurnNumber();
                    if (Config.Instance.DiscardZeroTurnGame && _game.CurrentGameStats.Turns < 1)
                    {
                        Logger.WriteLine("Game has 0 turns, discarded. (DiscardZeroTurnGame)", "GameEventHandler");
                        return;
                    }
                    _game.CurrentGameStats.GameMode = _game.CurrentGameMode;
                    Logger.WriteLine("Set CurrentGameStats.GameMode to " + _game.CurrentGameMode, "GameEventHandler");
                    _game.CurrentGameStats = null;
                }

                if (_assignedDeck == null)
                {
                    Logger.WriteLine("Saving DefaultDeckStats", "GameEventHandler");
                    DefaultDeckStats.Save();
                }
                else
                {
                    Logger.WriteLine("Saving DeckStats", "GameEventHandler");
                    DeckStatsList.Save();
                }

                Helper.MainWindow.DeckPickerList.UpdateDecks();
                statsControl.Refresh();
            }
            else if (_assignedDeck != null && _assignedDeck.DeckStats.Games.Contains(_game.CurrentGameStats))
            {
                //game was not supposed to be recorded, remove from deck again.
                _assignedDeck.DeckStats.Games.Remove(_game.CurrentGameStats);
                statsControl.Refresh();
            }
        }

        public void HandlePlayerHeroPower(string cardId, int turn)
        {
            LogEvent("PlayerHeroPower", cardId, turn);
            _game.AddPlayToCurrentGame(PlayType.PlayerHeroPower, turn, cardId);
            GameEvents.OnPlayerHeroPower.Execute();
        }

        public void HandleOpponentHeroPower(string cardId, int turn)
        {
            LogEvent("OpponentHeroPower", cardId, turn);
            _game.AddPlayToCurrentGame(PlayType.OpponentHeroPower, turn, cardId);
            GameEvents.OnOpponentHeroPower.Execute();
        }

        public void HandlePlayerFatigue(int currentDamage)
        {
            LogEvent("PlayerFatigue", "", currentDamage);
            _game.PlayerFatigueCount = currentDamage;
            GameEvents.OnPlayerFatigue.Execute(currentDamage);
        }

        public void HandleOpponentFatigue(int currentDamage)
        {
            LogEvent("OpponentFatigue", "", currentDamage);
            _game.OpponentFatigueCount = currentDamage;
            GameEvents.OnOpponentFatigue.Execute(currentDamage);
        }

        #region Player

        public void HandlePlayerName(string name)
        {
            _game.PlayerName = name;
        }

        public void HandlePlayerGetToDeck(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerGetToDeck", cardId);
            _game.PlayerGetToDeck(cardId, turn);
            _game.AddPlayToCurrentGame(PlayType.PlayerGetToDeck, turn, cardId);
        }

        public void HandlePlayerGet(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerGet", cardId);
            _game.PlayerGet(cardId, false, turn);

            if (cardId == "GAME_005" && _game.CurrentGameStats != null)
            {
                _game.CurrentGameStats.Coin = true;
                Logger.WriteLine("Got coin", "GameStats");
            }

            _game.AddPlayToCurrentGame(PlayType.PlayerGet, turn, cardId);
            GameEvents.OnPlayerGet.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerBackToHand(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerBackToHand", cardId);
            _game.PlayerGet(cardId, true, turn);
            _game.AddPlayToCurrentGame(PlayType.PlayerBackToHand, turn, cardId);
            GameEvents.OnPlayerPlayToHand.Execute(GameV2.GetCardFromId(cardId));
        }

        public async void HandlePlayerDraw(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            if (cardId == "GAME_005")
                HandlePlayerGet(cardId, turn);
            else
            {
                if (_game.SetAsideCards.Contains(cardId))
                    _game.SetAsideCards.Remove(cardId);
                LogEvent("PlayerDraw", cardId);
                var correctDeck = _game.PlayerDraw(cardId);

                if (!(await correctDeck) && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage
                   && !Helper.MainWindow.IsShowingIncorrectDeckMessage && _game.IsUsingPremade && _game.CurrentGameMode != GameMode.Spectator)
                {
                    Helper.MainWindow.NeedToIncorrectDeckMessage = true;
                    Logger.WriteLine("Found incorrect deck on PlayerDraw", "GameEventHandler");
                }
                Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
                Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
                _game.AddPlayToCurrentGame(PlayType.PlayerDraw, turn, cardId);
            }
            GameEvents.OnPlayerDraw.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerMulligan(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerMulligan", cardId);
            TurnTimer.Instance.MulliganDone(ActivePlayer.Player);
            _game.PlayerMulligan(cardId);

            //without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

            _game.AddPlayToCurrentGame(PlayType.PlayerMulligan, 0, cardId);
            GameEvents.OnPlayerMulligan.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerSecretPlayed(string cardId, int turn, bool fromDeck)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerSecretPlayed", cardId);
            if (fromDeck)
                _game.PlayerDeckDiscard(cardId);
            else
                _game.PlayerHandDiscard(cardId);
            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.PlayerSecretPlayed, turn, cardId);
            GameEvents.OnPlayerPlay.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerHandDiscard(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerHandDiscard", cardId);
            if (_game.SetAsideCards.Contains(cardId))
                _game.SetAsideCards.Remove(cardId);
            _game.PlayerHandDiscard(cardId);
            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.PlayerHandDiscard, turn, cardId);
            GameEvents.OnPlayerHandDiscard.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerPlay(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerPlay", cardId);
            _game.PlayerPlayed(cardId);
            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

            _game.AddPlayToCurrentGame(PlayType.PlayerPlay, turn, cardId);
            GameEvents.OnPlayerPlay.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerDeckDiscard(string cardId, int turn)
        {
            LogEvent("PlayerDeckDiscard", cardId);
            var correctDeck = _game.PlayerDeckDiscard(cardId);

            if (!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage
               && !Helper.MainWindow.IsShowingIncorrectDeckMessage && _game.IsUsingPremade && _game.CurrentGameMode != GameMode.Spectator)
            {
                Helper.MainWindow.NeedToIncorrectDeckMessage = true;
                Logger.WriteLine("Found incorrect deck on PlayerDeckDiscard", "GameEventHandler");
            }
            _game.AddPlayToCurrentGame(PlayType.PlayerDeckDiscard, turn, cardId);

            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
            GameEvents.OnPlayerDeckDiscard.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlePlayerPlayToDeck(string cardId, int turn)
        {
            if (string.IsNullOrEmpty(cardId))
                return;
            LogEvent("PlayerPlayToDeck", cardId);
            _game.PlayerPlayToDeck(cardId);

            //without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
            Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
            Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

            _game.AddPlayToCurrentGame(PlayType.PlayerPlayToDeck, turn, cardId);
            GameEvents.OnPlayerPlayToDeck.Execute(GameV2.GetCardFromId(cardId));
        }

        #endregion

        #region Opponent

        public void HandleOpponentName(string name)
        {
            _game.OpponentName = name;
        }

        public void HandleOpponentGetToDeck(int turn)
        {
            LogEvent("OpponentGetToDeck", turn: turn);
            _game.OpponentGetToDeck(turn);
            _game.AddPlayToCurrentGame(PlayType.OpponentGetToDeck, turn, string.Empty);
        }

        public void HandleDustReward(int amount)
        {
            if (DeckList.Instance.ActiveDeck != null && DeckList.Instance.ActiveDeck.IsArenaDeck)
            {
                if (!DeckList.Instance.ActiveDeck.DustReward.HasValue)
                {
                    DeckList.Instance.ActiveDeck.DustReward = amount;
                    _lastArenaReward = DateTime.Now;
                }
                //All rewards are logged as soon as the run is over.
                //This makes sure no "old" data is added (in case hdt is restarted after an arena run)
                else if ((DateTime.Now - _lastArenaReward).TotalSeconds < 5)
                {
                    DeckList.Instance.ActiveDeck.DustReward += amount;
                    _lastArenaReward = DateTime.Now;
                }
            }
        }

        public void HandleGoldReward(int amount)
        {
            if (DeckList.Instance.ActiveDeck != null && DeckList.Instance.ActiveDeck.IsArenaDeck)
            {
                if (!DeckList.Instance.ActiveDeck.GoldReward.HasValue)
                {
                    DeckList.Instance.ActiveDeck.GoldReward = amount;
                    _lastArenaReward = DateTime.Now;
                }
                //All rewards are logged as soon as the run is over.
                //This makes sure no "old" data is added (in case hdt is restarted after an arena run)
                else if ((DateTime.Now - _lastArenaReward).TotalSeconds < 5)
                {
                    DeckList.Instance.ActiveDeck.GoldReward += amount;
                    _lastArenaReward = DateTime.Now;
                }
            }
        }

        public void SetRank(int rank)
        {
            if (_game.CurrentGameStats != null)
            {
                _game.CurrentGameStats.Rank = rank;
                Logger.WriteLine("set rank to " + rank, "GameEventHandler");
            }
            else if (_lastGame != null)
            {
                _lastGame.Rank = rank;
                Logger.WriteLine("set rank to " + rank, "GameEventHandler");
            }
        }

        public void HandleOpponentPlay(string cardId, int from, int turn)
        {
            LogEvent("OpponentPlay", cardId, turn, from);
            _game.OpponentPlay(cardId, from, turn);
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.OpponentPlay, turn, cardId);
            GameEvents.OnOpponentPlay.Execute(GameV2.GetCardFromId(cardId));
        }


        public void HandleOpponentJoust(string cardId)
        {
            _game.OpponentJoustReveal(cardId);
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
        }

        public void HandleOpponentHandDiscard(string cardId, int from, int turn)
        {
            LogEvent("OpponentHandDiscard", cardId, turn, from);
            try
            {
                _game.OpponentPlay(cardId, from, turn);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.ToString(), "OpponentHandDiscard");
            }
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.OpponentHandDiscard, turn, cardId);
            GameEvents.OnOpponentHandDiscard.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandlOpponentDraw(int turn)
        {
            LogEvent("OpponentDraw", turn: turn);
            _game.OpponentDraw(turn);
            _game.AddPlayToCurrentGame(PlayType.OpponentDraw, turn, string.Empty);
            GameEvents.OnOpponentDraw.Execute();
        }

        public void HandleOpponentMulligan(int from)
        {
            LogEvent("OpponentMulligan", from: from);
            _game.OpponentMulligan(from);
            TurnTimer.Instance.MulliganDone(ActivePlayer.Opponent);
            _game.AddPlayToCurrentGame(PlayType.OpponentMulligan, 0, string.Empty);
            GameEvents.OnOpponentMulligan.Execute();
        }

        public void HandleOpponentGet(int turn, int id)
        {
            LogEvent("OpponentGet", turn: turn);
            _game.OpponentGet(turn, id);
            _game.AddPlayToCurrentGame(PlayType.OpponentGet, turn, string.Empty);
            GameEvents.OnOpponentGet.Execute();
        }

        public void HandleOpponentSecretPlayed(string cardId, int from, int turn, bool fromDeck, int otherId)
        {
            LogEvent("OpponentSecretPlayed");
            _game.OpponentSecretCount++;
            if (fromDeck)
                _game.OpponentDeckDiscard(cardId);
            else
                _game.OpponentPlay(cardId, from, turn);
            _game.AddPlayToCurrentGame(PlayType.OpponentSecretPlayed, turn, cardId);

            var isStolenCard = from > 0 && _game.OpponentHandMarks[from - 1] == CardMark.Stolen;
            _game.OpponentSecrets.NewSecretPlayed(otherId, isStolenCard);

            Helper.MainWindow.Overlay.ShowSecrets();
            GameEvents.OnOpponentPlay.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandleOpponentPlayToHand(string cardId, int turn, int id)
        {
            LogEvent("OpponentBackToHand", cardId, turn);
            _game.OpponentBackToHand(cardId, turn, id);
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.OpponentBackToHand, turn, cardId);
            GameEvents.OnOpponentPlayToHand.Execute(GameV2.GetCardFromId(cardId));
        }


        public void HandleOpponentPlayToDeck(string cardId, int turn)
        {
            LogEvent("OpponentPlayToDeck", cardId, turn);
            _game.OpponentPlayToDeck(cardId, turn);
            _game.AddPlayToCurrentGame(PlayType.OpponentPlayToDeck, turn, cardId);
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
            GameEvents.OnOpponentPlayToDeck.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandleOpponentSecretTrigger(string cardId, int turn, int otherId)
        {
            LogEvent("OpponentSecretTrigger", cardId);
            _game.OpponentSecretTriggered(cardId);
            _game.OpponentSecretCount--;
            _game.OpponentSecrets.SecretRemoved(otherId);
            if (_game.OpponentSecretCount <= 0)
                Helper.MainWindow.Overlay.HideSecrets();
            else
            {
                if (Config.Instance.AutoGrayoutSecrets)
                    _game.OpponentSecrets.SetZero(cardId);
                Helper.MainWindow.Overlay.ShowSecrets();
            }
            _game.AddPlayToCurrentGame(PlayType.OpponentSecretTriggered, turn, cardId);
            GameEvents.OnOpponentSecretTriggered.Execute(GameV2.GetCardFromId(cardId));
        }

        public void HandleOpponentDeckDiscard(string cardId, int turn)
        {
            LogEvent("OpponentDeckDiscard", cardId);
            _game.OpponentDeckDiscard(cardId);

            //there seems to be an issue with the overlay not updating here.
            //possibly a problem with order of logs?
            Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
            Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
            _game.AddPlayToCurrentGame(PlayType.OpponentDeckDiscard, turn, cardId);
            GameEvents.OnOpponentDeckDiscard.Execute(GameV2.GetCardFromId(cardId));
        }

        #endregion

        #region IGameHandlerImplementation

        void IGameHandler.HandlePlayerBackToHand(string cardId, int turn)
        {
            HandlePlayerBackToHand(cardId, turn);
        }

        void IGameHandler.HandlePlayerDraw(string cardId, int turn)
        {
            HandlePlayerDraw(cardId, turn);
        }

        void IGameHandler.HandlePlayerMulligan(string cardId)
        {
            HandlePlayerMulligan(cardId);
        }

        void IGameHandler.HandlePlayerSecretPlayed(string cardId, int turn, bool fromDeck)
        {
            HandlePlayerSecretPlayed(cardId, turn, fromDeck);
        }

        void IGameHandler.HandlePlayerHandDiscard(string cardId, int turn)
        {
            HandlePlayerHandDiscard(cardId, turn);
        }

        void IGameHandler.HandlePlayerPlay(string cardId, int turn)
        {
            HandlePlayerPlay(cardId, turn);
        }

        void IGameHandler.HandlePlayerDeckDiscard(string cardId, int turn)
        {
            HandlePlayerDeckDiscard(cardId, turn);
        }

        void IGameHandler.HandlePlayerHeroPower(string cardId, int turn)
        {
            HandlePlayerHeroPower(cardId, turn);
        }

        void IGameHandler.HandleOpponentPlay(string cardId, int @from, int turn)
        {
            HandleOpponentPlay(cardId, @from, turn);
        }

        void IGameHandler.HandleOpponentHandDiscard(string cardId, int @from, int turn)
        {
            HandleOpponentHandDiscard(cardId, @from, turn);
        }

        void IGameHandler.HandleOpponentDraw(int turn)
        {
            HandlOpponentDraw(turn);
        }

        void IGameHandler.HandleOpponentMulligan(int @from)
        {
            HandleOpponentMulligan(@from);
        }

        void IGameHandler.HandleOpponentGet(int turn, int id)
        {
            HandleOpponentGet(turn, id);
        }

        void IGameHandler.HandleOpponentSecretPlayed(string cardId, int @from, int turn, bool fromDeck, int otherId)
        {
            HandleOpponentSecretPlayed(cardId, @from, turn, fromDeck, otherId);
        }

        void IGameHandler.HandleOpponentPlayToHand(string cardId, int turn, int id)
        {
            HandleOpponentPlayToHand(cardId, turn, id);
        }

        void IGameHandler.HandleOpponentSecretTrigger(string cardId, int turn, int otherId)
        {
            HandleOpponentSecretTrigger(cardId, turn, otherId);
        }

        void IGameHandler.HandleOpponentDeckDiscard(string cardId, int turn)
        {
            HandleOpponentDeckDiscard(cardId, turn);
        }

        void IGameHandler.SetOpponentHero(string hero)
        {
            SetOpponentHero(hero);
        }

        void IGameHandler.SetPlayerHero(string hero)
        {
            SetPlayerHero(hero);
        }

        void IGameHandler.HandleOpponentHeroPower(string cardId, int turn)
        {
            HandleOpponentHeroPower(cardId, turn);
        }

        void IGameHandler.TurnStart(ActivePlayer player, int turnNumber)
        {
            TurnStart(player, turnNumber);
        }

        void IGameHandler.HandleGameStart()
        {
            HandleGameStart();
        }

        void IGameHandler.HandleGameEnd()
        {
            HandleGameEnd();
        }

        void IGameHandler.HandleLoss()
        {
            HandleLoss();
        }

        void IGameHandler.HandleWin()
        {
            HandleWin();
        }

        void IGameHandler.HandleTied()
        {
            HandleTied();
        }

        void IGameHandler.PlayerSetAside(string id)
        {
            PlayerSetAside(id);
        }

        void IGameHandler.HandlePlayerGet(string cardId, int turn)
        {
            HandlePlayerGet(cardId, turn);
        }

        void IGameHandler.HandlePlayerPlayToDeck(string cardId, int turn)
        {
            HandlePlayerPlayToDeck(cardId, turn);
        }

        void IGameHandler.HandleOpponentPlayToDeck(string cardId, int turn)
        {
            HandleOpponentPlayToDeck(cardId, turn);
        }

        void IGameHandler.SetGameMode(GameMode mode)
        {
            SetGameMode(mode);
        }

        void IGameHandler.HandlePlayerFatigue(int currentDamage)
        {
            HandlePlayerFatigue(currentDamage);
        }

        void IGameHandler.HandleOpponentFatigue(int currentDamage)
        {
            HandleOpponentFatigue(currentDamage);
        }

        #endregion IGameHandlerImplementation
    }
}