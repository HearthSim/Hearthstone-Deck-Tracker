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
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class GameEventHandler : IGameHandler
	{
		private const int MaxCardsOnCollectionPage = 8;
		private static DateTime _lastGameStart;
		private static Deck _assignedDeck;
		private static GameStats _lastGame;
		private static bool _showedNoteDialog;
		private bool _doneImportingConstructed;
		private List<string> _ignoreCachedIds;
		private int _lastCachedManaCost;
		private int _lastManaCost;
		private bool _startImportingCached;
		private int _unloadedCardCount;

		public static bool RecordCurrentGameMode
		{
			get
			{
				return Game.CurrentGameMode == GameMode.None && Config.Instance.RecordOther
				       || Game.CurrentGameMode == GameMode.Practice && Config.Instance.RecordPractice
				       || Game.CurrentGameMode == GameMode.Arena && Config.Instance.RecordArena
				       || Game.CurrentGameMode == GameMode.Ranked && Config.Instance.RecordRanked
				       || Game.CurrentGameMode == GameMode.Friendly && Config.Instance.RecordFriendly
				       || Game.CurrentGameMode == GameMode.Casual && Config.Instance.RecordCasual
				       || Game.CurrentGameMode == GameMode.Spectator && Config.Instance.RecordSpectator;
			}
		}

		public void ResetConstructedImporting()
		{
			Logger.WriteLine("Reset constructed importing", "GameEventHandler");
			_doneImportingConstructed = false;
			_lastManaCost = 0;
			_unloadedCardCount = 0;
			_ignoreCachedIds = new List<string>(Config.Instance.ConstructedImportingIgnoreCachedIds);
			Game.ResetConstructedCards();
		}

		public void HandlePossibleConstructedCard(string id, bool canBeDoneImporting)
		{
			if(_doneImportingConstructed)
				return;
			var card = Game.GetCardFromId(id);
			if(card == null || !Game.IsActualCard(card))
				return;
			if(canBeDoneImporting)
			{
				_unloadedCardCount++;
				var containsOtherThanDruid =
					Game.PossibleConstructedCards.Any(c => !string.IsNullOrEmpty(c.PlayerClass) && c.PlayerClass != "Druid");
				var cardCount =
					Game.PossibleConstructedCards.Where(c => !Config.Instance.ConstructedImportingIgnoreCachedIds.Contains(c.Id))
					    .Count(c => (!containsOtherThanDruid || c.PlayerClass != "Druid"));
				if(_unloadedCardCount > MaxCardsOnCollectionPage && card.Cost < _lastManaCost && cardCount > 10)
				{
					_doneImportingConstructed = true;
					return;
				}
				_lastManaCost = card.Cost;
			}
			else
			{
				if(Helper.SettingUpConstructedImporting)
				{
					if(!Game.PossibleConstructedCards.Contains(card))
						Game.PossibleConstructedCards.Add(card);
					return;
				}
				if(_ignoreCachedIds.Contains(card.Id))
				{
					_ignoreCachedIds.Remove(card.Id);
					return;
				}
			}
			if(!Game.PossibleConstructedCards.Contains(card))
				Game.PossibleConstructedCards.Add(card);
		}

		public void HandlePossibleArenaCard(string id)
		{
			var card = Game.GetCardFromId(id);
			if(!Game.IsActualCard(card))
				return;
			if(!Game.PossibleArenaCards.Contains(card))
				Game.PossibleArenaCards.Add(card);
		}

		public void HandleInMenu()
		{
			if(Game.IsInMenu)
				return;

			if(Config.Instance.RecordReplays && Game.Entities.Count > 0 && !Game.SavedReplay && Game.CurrentGameStats != null
			   && Game.CurrentGameStats.ReplayFile == null && RecordCurrentGameMode)
				Game.CurrentGameStats.ReplayFile = ReplayMaker.SaveToDisk();

			SaveAndUpdateStats();

			Game.IsInMenu = true;
			TurnTimer.Instance.Stop();
			Helper.MainWindow.Overlay.HideTimers();
			Helper.MainWindow.Overlay.HideSecrets();
			if(Config.Instance.KeyPressOnGameEnd != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd, "GameEventHandler");
			}
			if(!Config.Instance.KeepDecksVisible)
			{
				var deck = DeckList.Instance.ActiveDeckVersion;
				if(deck != null)
					Game.SetPremadeDeck((Deck)deck.Clone());
			}
			if(!Game.IsUsingPremade)
				Game.DrawnLastGame = new List<Card>(Game.PlayerDrawn);
			HsLogReader.Instance.ClearLog();
			if(!Config.Instance.KeepDecksVisible)
				Game.Reset(false);
			if(Game.CurrentGameStats != null && Game.CurrentGameStats.Result != GameResult.None)
				Game.CurrentGameStats = null;
			if(Game.CurrentGameMode == GameMode.Spectator)
				SetGameMode(GameMode.None);
			GameEvents.OnInMenu.Execute();
		}

		public void HandleConcede()
		{
			if(Game.CurrentGameStats == null)
				return;
			Game.CurrentGameStats.WasConceded = true;
		}

		public static void SetOpponentHero(string hero)
		{
			if(string.IsNullOrEmpty(hero))
				return;
			Game.PlayingAgainst = hero;

			if(Game.CurrentGameStats != null)
				Game.CurrentGameStats.OpponentHero = hero;
			Logger.WriteLine("Playing against " + hero, "GameEventHandler");

			HeroClass heroClass;
			if(Enum.TryParse(hero, true, out heroClass))
				Game.OpponentSecrets.HeroClass = heroClass;
		}

		public static void SetPlayerHero(string hero)
		{
			try
			{
				if(!string.IsNullOrEmpty(hero))
				{
					Game.PlayingAs = hero;
					if(Game.CurrentGameStats != null)
						Game.CurrentGameStats.PlayerHero = hero;
					var selectedDeck = DeckList.Instance.ActiveDeckVersion;
					if(!Game.IsUsingPremade || !Config.Instance.AutoDeckDetection)
						return;

					if(selectedDeck == null || selectedDeck.Class != Game.PlayingAs)
					{
						var classDecks = DeckList.Instance.Decks.Where(d => d.Class == Game.PlayingAs && !d.Archived).ToList();
						if(classDecks.Count == 0)
							Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
						else if(classDecks.Count == 1)
						{
							Helper.MainWindow.DeckPickerList.SelectDeck(classDecks[0]);
							Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
						}
						else if(DeckList.Instance.LastDeckClass.Any(ldc => ldc.Class == Game.PlayingAs))
						{
							var lastDeck = DeckList.Instance.LastDeckClass.First(ldc => ldc.Class == Game.PlayingAs);
							Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeck.Name, "HandleGameStart");

							var deck = lastDeck.Id == Guid.Empty
								           ? DeckList.Instance.Decks.FirstOrDefault(d => d.Name == lastDeck.Name)
								           : DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == lastDeck.Id);

							if(deck != null)
							{
								if(deck.Archived)
								{
									Logger.WriteLine("Deck " + deck.Name + " is archived - not switching", "HandleGameStart");
									return;
								}

								Helper.MainWindow.NeedToIncorrectDeckMessage = false;
								Helper.MainWindow.DeckPickerList.SelectDeck(deck);
								Helper.MainWindow.UpdateDeckList(deck);
								Helper.MainWindow.UseDeck(deck);
							}
						}
					}
				}
			}
			catch(Exception exception)
			{
				Logger.WriteLine("Error setting player hero: " + exception, "GameEventHandler");
			}
		}

		public static void TurnStart(ActivePlayer player, int turnNumber)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", player, turnNumber + 1), "GameEventHandler");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			TurnTimer.Instance.SetCurrentPlayer(player);
			TurnTimer.Instance.Restart();
			if(player == ActivePlayer.Player && !Game.IsInMenu)
			{
				if(Config.Instance.FlashHsOnTurnStart)
					User32.FlashHs();

				if(Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}
			GameEvents.OnTurnStart.Execute(player);
		}

		public static void HandleGameStart()
		{
			if(DateTime.Now - _lastGameStart < new TimeSpan(0, 0, 0, 5)) //game already started
				return;
			_lastGameStart = DateTime.Now;
			Logger.WriteLine("Game start", "GameEventHandler");

			if(Config.Instance.FlashHsOnTurnStart)
				User32.FlashHs();
			if(Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if(Config.Instance.KeyPressOnGameStart != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart, "GameEventHandler");
			}
			_showedNoteDialog = false;
			Game.IsInMenu = false;
			Game.Reset();

			var selectedDeck = DeckList.Instance.ActiveDeckVersion;
			if(selectedDeck != null)
				Game.SetPremadeDeck((Deck)selectedDeck.Clone());
			GameEvents.OnGameStart.Execute();
		}
#pragma warning disable 4014
		public static async void HandleGameEnd()
		{
			Helper.MainWindow.Overlay.HideTimers();
			if(Game.CurrentGameStats == null)
				return;
			if(Game.CurrentGameMode == GameMode.Spectator && !Config.Instance.RecordSpectator)
			{
				Logger.WriteLine("Game is in Spectator mode, discarded. (Record Spectator disabled)", "GameEventHandler");
				_assignedDeck = null;
				return;
			}
			var player = Game.Entities.FirstOrDefault(e => e.Value.IsPlayer);
			var opponent = Game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && !e.Value.IsPlayer);
			if(player.Value != null)
				Game.CurrentGameStats.PlayerName = player.Value.Name;
			if(opponent.Value != null && CardIds.HeroIdDict.ContainsValue(Game.CurrentGameStats.OpponentHero))
				Game.CurrentGameStats.OpponentName = opponent.Value.Name;
			else
				Game.CurrentGameStats.OpponentName = Game.CurrentGameStats.OpponentHero;

			Game.CurrentGameStats.Turns = HsLogReader.Instance.GetTurnNumber();
			if(Config.Instance.DiscardZeroTurnGame && Game.CurrentGameStats.Turns < 1)
			{
				Logger.WriteLine("Game has 0 turns, discarded. (DiscardZeroTurnGame)", "GameEventHandler");
				_assignedDeck = null;
				GameEvents.OnGameEnd.Execute();
				return;
			}
			Game.CurrentGameStats.GameEnd();
			GameEvents.OnGameEnd.Execute();
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck != null)
			{
				if(Config.Instance.DiscardGameIfIncorrectDeck
				   && !Game.PlayerDrawn.All(
				                            c =>
				                            c.IsStolen
				                            || selectedDeck.GetSelectedDeckVersion().Cards.Any(c2 => c.Id == c2.Id && c.Count <= c2.Count)))
				{
					if(Config.Instance.AskBeforeDiscardingGame)
					{
						var discardDialog = new DiscardGameDialog(Game.CurrentGameStats);
						discardDialog.Topmost = true;
						discardDialog.ShowDialog();
						if(discardDialog.Result == DiscardGameDialogResult.Discard)
						{
							Logger.WriteLine("Assigned current game to NO deck - selected deck does not match cards played (dialogresult: discard)",
							                 "GameEventHandler");
							Game.CurrentGameStats.DeleteGameFile();
							_assignedDeck = null;
							return;
						}
						if(discardDialog.Result == DiscardGameDialogResult.MoveToOther)
						{
							var moveDialog = new MoveGameDialog(DeckList.Instance.Decks.Where(d => d.Class == Game.CurrentGameStats.PlayerHero));
							moveDialog.Topmost = true;
							moveDialog.ShowDialog();
							var targetDeck = moveDialog.SelectedDeck;
							if(targetDeck != null)
							{
								selectedDeck = targetDeck;
								Game.CurrentGameStats.PlayerDeckVersion = moveDialog.SelectedVersion;
								Game.CurrentGameStats.HearthStatsDeckVersionId = targetDeck.GetVersion(moveDialog.SelectedVersion).HearthStatsDeckVersionId;
								//...continue as normal
							}
							else
							{
								Logger.WriteLine("No deck selected in move game dialog after discard dialog, discarding game", "GameEventHandler");
								Game.CurrentGameStats.DeleteGameFile();
								_assignedDeck = null;
								return;
							}
						}
					}
					else
					{
						Logger.WriteLine("Assigned current game to NO deck - selected deck does not match cards played (no dialog)", "GameEventHandler");
						Game.CurrentGameStats.DeleteGameFile();
						_assignedDeck = null;
						return;
					}
				}
				else
				{
					Game.CurrentGameStats.PlayerDeckVersion = DeckList.Instance.ActiveDeckVersion.Version;
					Game.CurrentGameStats.HearthStatsDeckVersionId = DeckList.Instance.ActiveDeckVersion.HearthStatsDeckVersionId;
				}

				_lastGame = Game.CurrentGameStats;
				selectedDeck.DeckStats.AddGameResult(_lastGame);
				if(Config.Instance.ShowNoteDialogAfterGame && !Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
				{
					_showedNoteDialog = true;
					new NoteDialog(Game.CurrentGameStats);
				}
				Logger.WriteLine("Assigned current game to deck: " + selectedDeck.Name, "GameStats");
				_assignedDeck = selectedDeck;

				// Unarchive the active deck after we have played a game with it
				if(_assignedDeck.Archived)
				{
					Logger.WriteLine("Automatically unarchiving deck " + selectedDeck.Name + " after assigning current game", "GameEventHandler");
					Helper.MainWindow.ArchiveDeck(_assignedDeck, false);
				}

				if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewGames)
				{
					if(Game.CurrentGameMode == GameMode.None)
						await GameModeDetection(300); //give the user 5 minutes to get out of the victory/defeat screen
					if(Game.CurrentGameMode == GameMode.Casual)
						await HsLogReader.Instance.RankedDetection();
					if(Game.CurrentGameMode == GameMode.Ranked && !_lastGame.HasRank)
						await RankDetection(5);
					await GameModeSaved(15);
					if(Game.CurrentGameMode == GameMode.Arena)
						HearthStatsManager.UploadArenaMatchAsync(_lastGame, selectedDeck, background: true);
					else
						HearthStatsManager.UploadMatchAsync(_lastGame, selectedDeck, background: true);
				}
				_lastGame = null;
			}
			else
			{
				DefaultDeckStats.Instance.GetDeckStats(Game.PlayingAs).AddGameResult(Game.CurrentGameStats);
				Logger.WriteLine(string.Format("Assigned current deck to default {0} deck.", Game.PlayingAs), "GameStats");
				_assignedDeck = null;
			}
		}
#pragma warning restore 4014
		private static async Task RankDetection(int timeoutInSeconds)
		{
			Logger.WriteLine("waiting for rank detection", "GameEventHandler");
			var startTime = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(_lastGame != null && !_lastGame.HasRank && (DateTime.Now - startTime) < timeout)
				await Task.Delay(100);
		}

		private static async Task GameModeDetection(int timeoutInSeconds)
		{
			Logger.WriteLine("waiting for game mode detection", "GameEventHandler");
			var startTime = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(Game.CurrentGameMode == GameMode.None && (DateTime.Now - startTime) < timeout)
				await Task.Delay(100);
		}

		private static async Task GameModeSaved(int timeoutInSeconds)
		{
			Logger.WriteLine("waiting for game mode to be saved to game", "GameEventHandler");
			var startTime = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(_lastGame != null && _lastGame.GameMode == GameMode.None && (DateTime.Now - startTime) < timeout)
				await Task.Delay(100);
		}

		private static void LogEvent(string type, string id = "", int turn = 0, int from = -1)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", type, id, turn, from), "GameEventHandler");
		}

		public static void PlayerSetAside(string id)
		{
			Game.SetAsideCards.Add(id);
			Logger.WriteLine("set aside: " + id, "GameEventHandler");
		}

		public static void HandleWin()
		{
			if(Game.CurrentGameStats == null)
				return;
			Logger.WriteLine("Game was won!", "GameEventHandler");
			Game.CurrentGameStats.Result = GameResult.Win;
			GameEvents.OnGameWon.Execute();
		}

		public static void HandleLoss()
		{
			if(Game.CurrentGameStats == null)
				return;
			Logger.WriteLine("Game was lost!", "GameEventHandler");
			Game.CurrentGameStats.Result = GameResult.Loss;
			GameEvents.OnGameLost.Execute();
		}

		public static void HandleTied()
		{
			if(Game.CurrentGameStats == null)
				return;
			Logger.WriteLine("Game was a tie!", "GameEventHandler");
			Game.CurrentGameStats.Result = GameResult.Draw;
			GameEvents.OnGameTied.Execute();
		}

		public static void SetGameMode(GameMode mode)
		{
			Game.CurrentGameMode = mode;
		}

		private static void SaveAndUpdateStats()
		{
			var statsControl = Config.Instance.StatsInWindow ? Helper.MainWindow.StatsWindow.StatsControl : Helper.MainWindow.DeckStatsFlyout;
			if(RecordCurrentGameMode)
			{
				if(Config.Instance.ShowNoteDialogAfterGame && Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
				{
					_showedNoteDialog = true;
					new NoteDialog(Game.CurrentGameStats);
				}

				if(Game.CurrentGameStats != null)
				{
					Game.CurrentGameStats.Turns = HsLogReader.Instance.GetTurnNumber();
					if(Config.Instance.DiscardZeroTurnGame && Game.CurrentGameStats.Turns < 1)
					{
						Logger.WriteLine("Game has 0 turns, discarded. (DiscardZeroTurnGame)", "GameEventHandler");
						return;
					}
					Game.CurrentGameStats.GameMode = Game.CurrentGameMode;
					Logger.WriteLine("Set CurrentGameStats.GameMode to " + Game.CurrentGameMode, "GameEventHandler");
					Game.CurrentGameStats = null;
				}

				if(_assignedDeck == null)
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
			else if(_assignedDeck != null && _assignedDeck.DeckStats.Games.Contains(Game.CurrentGameStats))
			{
				//game was not supposed to be recorded, remove from deck again.
				_assignedDeck.DeckStats.Games.Remove(Game.CurrentGameStats);
				statsControl.Refresh();
			}
		}

		public static void HandlePlayerHeroPower(string cardId, int turn)
		{
			LogEvent("PlayerHeroPower", cardId, turn);
			Game.AddPlayToCurrentGame(PlayType.PlayerHeroPower, turn, cardId);
			GameEvents.OnPlayerHeroPower.Execute();
		}

		public static void HandleOpponentHeroPower(string cardId, int turn)
		{
			LogEvent("OpponentHeroPower", cardId, turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentHeroPower, turn, cardId);
			GameEvents.OnOpponentHeroPower.Execute();
		}

		public static void HandlePlayerFatigue(int currentDamage)
		{
			LogEvent("PlayerFatigue", "", currentDamage);
			Game.PlayerFatigueCount = currentDamage;
			GameEvents.OnPlayerFatigue.Execute(currentDamage);
		}

		public static void HandleOpponentFatigue(int currentDamage)
		{
			LogEvent("OpponentFatigue", "", currentDamage);
			Game.OpponentFatigueCount = currentDamage;
			GameEvents.OnOpponentFatigue.Execute(currentDamage);
		}

		#region Player

		public void HandlePlayerName(string name)
		{
			Game.PlayerName = name;
		}

		public void HandlePlayerGetToDeck(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerGetToDeck", cardId);
			Game.PlayerGetToDeck(cardId, turn);
			Game.AddPlayToCurrentGame(PlayType.PlayerGetToDeck, turn, cardId);
		}

		public static void HandlePlayerGet(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerGet", cardId);
			Game.PlayerGet(cardId, false, turn);

			if(cardId == "GAME_005" && Game.CurrentGameStats != null)
			{
				Game.CurrentGameStats.Coin = true;
				Logger.WriteLine("Got coin", "GameStats");
			}

			Game.AddPlayToCurrentGame(PlayType.PlayerGet, turn, cardId);
			GameEvents.OnPlayerGet.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerBackToHand(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerBackToHand", cardId);
			Game.PlayerGet(cardId, true, turn);
			Game.AddPlayToCurrentGame(PlayType.PlayerBackToHand, turn, cardId);
			GameEvents.OnPlayerPlayToHand.Execute(Game.GetCardFromId(cardId));
		}

		public static async void HandlePlayerDraw(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			if(cardId == "GAME_005")
				HandlePlayerGet(cardId, turn);
			else
			{
				if(Game.SetAsideCards.Contains(cardId))
					Game.SetAsideCards.Remove(cardId);
				LogEvent("PlayerDraw", cardId);
				var correctDeck = Game.PlayerDraw(cardId);

				if(!(await correctDeck) && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage
				   && !Helper.MainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade && Game.CurrentGameMode != GameMode.Spectator)
				{
					Helper.MainWindow.NeedToIncorrectDeckMessage = true;
					Logger.WriteLine("Found incorrect deck on PlayerDraw", "GameEventHandler");
				}
				Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
				Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
				Game.AddPlayToCurrentGame(PlayType.PlayerDraw, turn, cardId);
			}
			GameEvents.OnPlayerDraw.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerMulligan(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerMulligan", cardId);
			TurnTimer.Instance.MulliganDone(ActivePlayer.Player);
			Game.PlayerMulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

			Game.AddPlayToCurrentGame(PlayType.PlayerMulligan, 0, cardId);
			GameEvents.OnPlayerMulligan.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerSecretPlayed(string cardId, int turn, bool fromDeck)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerSecretPlayed", cardId);
			if(fromDeck)
				Game.PlayerDeckDiscard(cardId);
			else
				Game.PlayerHandDiscard(cardId);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.PlayerSecretPlayed, turn, cardId);
			GameEvents.OnPlayerPlay.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerHandDiscard(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerHandDiscard", cardId);
			if(Game.SetAsideCards.Contains(cardId))
				Game.SetAsideCards.Remove(cardId);
			Game.PlayerHandDiscard(cardId);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.PlayerHandDiscard, turn, cardId);
			GameEvents.OnPlayerHandDiscard.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerPlay(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerPlay", cardId);
			Game.PlayerPlayed(cardId);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

			Game.AddPlayToCurrentGame(PlayType.PlayerPlay, turn, cardId);
			GameEvents.OnPlayerPlay.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerDeckDiscard(string cardId, int turn)
		{
			LogEvent("PlayerDeckDiscard", cardId);
			var correctDeck = Game.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if(!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage
			   && !Helper.MainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade && Game.CurrentGameMode != GameMode.Spectator)
			{
				Helper.MainWindow.NeedToIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck on PlayerDeckDiscard", "GameEventHandler");
			}
			Game.AddPlayToCurrentGame(PlayType.PlayerDeckDiscard, turn, cardId);

			//temp fix for deck not being updated here
			//todo: figure out why draw is updating but deckdiscard is not
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
			GameEvents.OnPlayerDeckDiscard.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlePlayerPlayToDeck(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerPlayToDeck", cardId);
			Game.PlayerPlayToDeck(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

			Game.AddPlayToCurrentGame(PlayType.PlayerPlayToDeck, turn, cardId);
			GameEvents.OnPlayerPlayToDeck.Execute(Game.GetCardFromId(cardId));
		}

		#endregion

		#region Opponent

		public void HandleOpponentName(string name)
		{
			Game.OpponentName = name;
		}

		public void HandleOpponentGetToDeck(int turn)
		{
			LogEvent("OpponentGetToDeck", turn: turn);
			Game.OpponentGetToDeck(turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentGetToDeck, turn, string.Empty);
		}

		public void SetRank(int rank)
		{
			if(Game.CurrentGameStats != null)
			{
				Game.CurrentGameStats.Rank = rank;
				Logger.WriteLine("set rank to " + rank, "GameEventHandler");
			}
			else if(_lastGame != null)
			{
				_lastGame.Rank = rank;
				Logger.WriteLine("set rank to " + rank, "GameEventHandler");
			}
		}

		public static void HandleOpponentPlay(string cardId, int from, int turn)
		{
			LogEvent("OpponentPlay", cardId, turn, from);
			Game.OpponentPlay(cardId, from, turn);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentPlay, turn, cardId);
			GameEvents.OnOpponentPlay.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandleOpponentHandDiscard(string cardId, int from, int turn)
		{
			LogEvent("OpponentHandDiscard", cardId, turn, from);
			try
			{
				Game.OpponentPlay(cardId, from, turn);
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString(), "OpponentHandDiscard");
			}
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentHandDiscard, turn, cardId);
			GameEvents.OnOpponentHandDiscard.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandlOpponentDraw(int turn)
		{
			LogEvent("OpponentDraw", turn: turn);
			Game.OpponentDraw(turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentDraw, turn, string.Empty);
			GameEvents.OnOpponentDraw.Execute();
		}

		public static void HandleOpponentMulligan(int from)
		{
			LogEvent("OpponentMulligan", from: from);
			Game.OpponentMulligan(from);
			TurnTimer.Instance.MulliganDone(ActivePlayer.Opponent);
			Game.AddPlayToCurrentGame(PlayType.OpponentMulligan, 0, string.Empty);
			GameEvents.OnOpponentMulligan.Execute();
		}

		public static void HandleOpponentGet(int turn, int id)
		{
			LogEvent("OpponentGet", turn: turn);
			Game.OpponentGet(turn, id);
			Game.AddPlayToCurrentGame(PlayType.OpponentGet, turn, string.Empty);
			GameEvents.OnOpponentGet.Execute();
		}

		public static void HandleOpponentSecretPlayed(string cardId, int from, int turn, bool fromDeck, int otherId)
		{
			LogEvent("OpponentSecretPlayed");
			Game.OpponentSecretCount++;
			if(fromDeck)
				Game.OpponentDeckDiscard(cardId);
			else
				Game.OpponentPlay(cardId, from, turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentSecretPlayed, turn, cardId);

			var isStolenCard = from > 0 && Game.OpponentHandMarks[from - 1] == CardMark.Stolen;
			Game.OpponentSecrets.NewSecretPlayed(otherId, isStolenCard);

			Helper.MainWindow.Overlay.ShowSecrets();
			GameEvents.OnOpponentPlay.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandleOpponentPlayToHand(string cardId, int turn, int id)
		{
			LogEvent("OpponentBackToHand", cardId, turn);
			Game.OpponentBackToHand(cardId, turn, id);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentBackToHand, turn, cardId);
			GameEvents.OnOpponentPlayToHand.Execute(Game.GetCardFromId(cardId));
		}


		public static void HandleOpponentPlayToDeck(string cardId, int turn)
		{
			LogEvent("OpponentPlayToDeck", cardId, turn);
			Game.OpponentPlayToDeck(cardId, turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentPlayToDeck, turn, cardId);
			Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			GameEvents.OnOpponentPlayToDeck.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandleOpponentSecretTrigger(string cardId, int turn, int otherId)
		{
			LogEvent("OpponentSecretTrigger", cardId);
			Game.OpponentSecretTriggered(cardId);
			Game.OpponentSecretCount--;
			Game.OpponentSecrets.SecretRemoved(otherId);
			if(Game.OpponentSecretCount <= 0)
				Helper.MainWindow.Overlay.HideSecrets();
			else
			{
				if(Config.Instance.AutoGrayoutSecrets)
					Game.OpponentSecrets.SetZero(cardId);
				Helper.MainWindow.Overlay.ShowSecrets();
			}
			Game.AddPlayToCurrentGame(PlayType.OpponentSecretTriggered, turn, cardId);
			GameEvents.OnOpponentSecretTriggered.Execute(Game.GetCardFromId(cardId));
		}

		public static void HandleOpponentDeckDiscard(string cardId, int turn)
		{
			LogEvent("OpponentDeckDiscard", cardId);
			Game.OpponentDeckDiscard(cardId);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			Helper.MainWindow.Overlay.ListViewOpponent.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentDeckDiscard, turn, cardId);
			GameEvents.OnOpponentDeckDiscard.Execute(Game.GetCardFromId(cardId));
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