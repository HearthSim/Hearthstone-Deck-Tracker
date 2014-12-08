using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker
{
    public class GameEventHandler : IGameHandler
	{
		#region Player

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
		}

        public static void HandlePlayerBackToHand(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerBackToHand", cardId);
			Game.PlayerGet(cardId, true, turn);
			Game.AddPlayToCurrentGame(PlayType.PlayerBackToHand, turn, cardId);
		}

		public static async void HandlePlayerDraw(string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			if(Game.SetAsideCards.Contains(cardId))
				Game.SetAsideCards.Remove(cardId);
			LogEvent("PlayerDraw", cardId);
			var correctDeck = Game.PlayerDraw(cardId);

			if(!(await correctDeck) && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage &&
			   !Helper.MainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				Helper.MainWindow.NeedToIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck");
			}
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.PlayerDraw, turn, cardId);
		}

		public static void HandlePlayerMulligan(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			LogEvent("PlayerMulligan", cardId);
			TurnTimer.Instance.MulliganDone(Turn.Player);
			Game.PlayerMulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();

			Game.AddPlayToCurrentGame(PlayType.PlayerMulligan, 0, cardId);
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
		}

		public static void HandlePlayerDeckDiscard(string cardId, int turn)
		{
			LogEvent("PlayerDeckDiscard", cardId);
			var correctDeck = Game.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if(!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow.NeedToIncorrectDeckMessage &&
			   !Helper.MainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				Helper.MainWindow.NeedToIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
			}
			Game.AddPlayToCurrentGame(PlayType.PlayerDeckDiscard, turn, cardId);

			//temp fix for deck not being updated here
			//todo: figure out why draw is updating but deckdiscard is not
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.PlayerWindow.ListViewPlayer.Items.Refresh();
		}

		#endregion

		#region Opponent

		public static void HandleOpponentPlay(string cardId, int from, int turn)
		{
			LogEvent("OpponentPlay", cardId, turn, from);
			Game.OpponentPlay(cardId, from, turn);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentPlay, turn, cardId);
		}

		public static void HandleOpponentHandDiscard(string cardId, int from, int turn)
		{
			LogEvent("OpponentHandDiscard", cardId, turn, from);
			Game.OpponentPlay(cardId, from, turn);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentHandDiscard, turn, cardId);
		}

		public static void HandlOpponentDraw(int turn)
		{
			LogEvent("OpponentDraw", turn: turn);
			Game.OpponentDraw(turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentDraw, turn, string.Empty);
		}

		public static void HandleOpponentMulligan(int from)
		{
			LogEvent("OpponentMulligan", from: from);
			Game.OpponentMulligan(from);
			TurnTimer.Instance.MulliganDone(Turn.Opponent);
			Game.AddPlayToCurrentGame(PlayType.OpponentMulligan, 0, string.Empty);
		}

		public static void HandleOpponentGet(int turn)
		{
			LogEvent("OpponentGet", turn: turn);
			Game.OpponentGet(turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentGet, turn, string.Empty);
		}

		public static void HandleOpponentSecretPlayed(string cardId, int from, int turn, bool fromDeck)
		{
			LogEvent("OpponentSecretPlayed");
			Game.OpponentSecretCount++;
			if(fromDeck)
				Game.OpponentDeckDiscard(cardId);
			else
				Game.OpponentPlay(cardId, from, turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentSecretPlayed, turn, cardId);
			Helper.MainWindow.Overlay.ShowSecrets(Game.PlayingAgainst);
		}

		public static void HandleOpponentPlayToHand(string cardId, int turn)
		{
			LogEvent("OpponentBackToHand", cardId, turn);
			Game.OpponentBackToHand(cardId, turn);
			Helper.MainWindow.Overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow.OpponentWindow.ListViewOpponent.Items.Refresh();
			Game.AddPlayToCurrentGame(PlayType.OpponentBackToHand, turn, cardId);
		}

		public static void HandleOpponentSecretTrigger(string cardId, int turn)
		{
			LogEvent("OpponentSecretTrigger", cardId);
			Game.OpponentSecretTriggered(cardId);
			Game.OpponentSecretCount--;
			if(Game.OpponentSecretCount <= 0)
				Helper.MainWindow.Overlay.HideSecrets();
			Game.AddPlayToCurrentGame(PlayType.OpponentSecretTriggered, turn, cardId);
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
		}

		#endregion

		public static void SetOpponentHero(string hero)
		{
			Game.PlayingAgainst = hero;

			if(Game.CurrentGameStats != null)
				Game.CurrentGameStats.OpponentHero = hero;

			Logger.WriteLine("Playing against " + hero, "Hearthstone");
		}

		public static void SetPlayerHero(string hero)
		{
			Game.PlayingAs = hero;

			var selectedDeck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if(selectedDeck != null)
				Game.SetPremadeDeck((Deck)selectedDeck.Clone());

			if(!string.IsNullOrEmpty(hero))
			{
				if(!Game.IsUsingPremade || !Config.Instance.AutoDeckDetection) return;

				if(selectedDeck == null || selectedDeck.Class != Game.PlayingAs)
				{
					var classDecks = Helper.MainWindow.DeckList.DecksList.Where(d => d.Class == Game.PlayingAs).ToList();
					if(classDecks.Count == 0)
						Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
					else if(classDecks.Count == 1)
					{
						Helper.MainWindow.DeckPickerList.SelectDeck(classDecks[0]);
						Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
					}
					else if(Helper.MainWindow.DeckList.LastDeckClass.Any(ldc => ldc.Class == Game.PlayingAs))
					{
						var lastDeckName = Helper.MainWindow.DeckList.LastDeckClass.First(ldc => ldc.Class == Game.PlayingAs).Name;
						Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

						var deck = Helper.MainWindow.DeckList.DecksList.FirstOrDefault(d => d.Name == lastDeckName);

						if(deck != null)
						{
							Helper.MainWindow.DeckPickerList.SelectDeck(deck);
							Helper.MainWindow.UpdateDeckList(deck);
							Helper.MainWindow.UseDeck(deck);
						}
					}
				}
			}
		}

		public static void TurnStart(Turn player, int turnNumber)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", player, turnNumber + 1), "LogReader");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			TurnTimer.Instance.SetCurrentPlayer(player);
			TurnTimer.Instance.Restart();
			if(player == Turn.Player && !Game.IsInMenu)
			{
				if(Config.Instance.FlashHsOnTurnStart)
					User32.FlashHs();

				if(Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}
		}

		public static void HandleGameStart()
		{
			//avoid new game being started when jaraxxus is played
			if(!Game.IsInMenu) return;
			
			Logger.WriteLine("Game start");

			if(Config.Instance.FlashHsOnTurnStart)
				User32.FlashHs();
			if(Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if(Config.Instance.KeyPressOnGameStart != "None" &&
			   Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart);
			}

			Game.IsInMenu = false;
			Game.Reset();
		}

		private static Deck _assignedDeck;
#pragma warning disable 4014
		public static void HandleGameEnd(bool backInMenu)
		{
			if(!backInMenu)
			{
				Helper.MainWindow.Overlay.HideTimers();
				if(Game.CurrentGameStats == null)
					return;
				Game.CurrentGameStats.Turns = HsLogReader.Instance.GetTurnNumber();
				Game.CurrentGameStats.GameEnd();

				var selectedDeck = Helper.MainWindow.DeckPickerList.SelectedDeck;
				if(selectedDeck != null)
				{
					if(Config.Instance.DiscardGameIfIncorrectDeck && !Game.PlayerDrawn.All(c => selectedDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count <= c2.Count)))
					{
						Logger.WriteLine("Assigned current game to NO deck - selected deck does not match cards played");
						_assignedDeck = null;
						return;
					}
					selectedDeck.DeckStats.AddGameResult(Game.CurrentGameStats);
					if(Config.Instance.ShowNoteDialogAfterGame)
						new NoteDialog(Game.CurrentGameStats);
					Logger.WriteLine("Assigned current game to deck: " + selectedDeck.Name, "GameStats");
					_assignedDeck = selectedDeck;
				}
				else
				{
					DefaultDeckStats.Instance.GetDeckStats(Game.PlayingAs).AddGameResult(Game.CurrentGameStats);
					Logger.WriteLine(string.Format("Assigned current deck to default {0} deck.", Game.PlayingAs), "GameStats");
					_assignedDeck = null;
				}
				return;
			}
			Logger.WriteLine("Game end");
			if(Config.Instance.KeyPressOnGameEnd != "None" && Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd);
			}
			TurnTimer.Instance.Stop();
			Helper.MainWindow.Overlay.HideTimers();
			Helper.MainWindow.Overlay.HideSecrets();
			if(!Game.IsUsingPremade)
				Game.DrawnLastGame = new List<Card>(Game.PlayerDrawn);
			if(!Config.Instance.KeepDecksVisible)
			{
				var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
				if(deck != null)
					Game.SetPremadeDeck((Deck)deck.Clone());

				Game.Reset(false);
			}
			Game.IsInMenu = true;
		}
#pragma warning restore 4014

		private static void LogEvent(string type, string id = "", int turn = 0, int from = -1)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", type, id, turn, from), "LogReader");
		}

		public static void PlayerSetAside(string id)
		{
			Game.SetAsideCards.Add(id);
			Logger.WriteLine("set aside: " + id);
		}

		public static void HandleWin()
		{
			if(!Game.IsInMenu || Game.CurrentGameStats == null)
				return;
			Logger.WriteLine("Game was won!", "GameStats");
			Game.CurrentGameStats.Result = GameResult.Win;
			SaveAndUpdateStats();
		}

		public static void HandleLoss()
		{
			if(!Game.IsInMenu || Game.CurrentGameStats == null)
				return;
			Logger.WriteLine("Game was lost!", "GameStats");
			Game.CurrentGameStats.Result = GameResult.Loss;
			SaveAndUpdateStats();
		}

		private static void SaveAndUpdateStats()
		{
			var statsControl = Config.Instance.StatsInWindow ? Helper.MainWindow.StatsWindow.StatsControl : Helper.MainWindow.DeckStatsFlyout;
			if(Game.CurrentGameMode == Game.GameMode.None && Config.Instance.RecordOther
			   || Game.CurrentGameMode == Game.GameMode.Practice && Config.Instance.RecordPractice
			   || Game.CurrentGameMode == Game.GameMode.Arena && Config.Instance.RecordArena
			   || Game.CurrentGameMode == Game.GameMode.Ranked && Config.Instance.RecordRanked
			   || Game.CurrentGameMode == Game.GameMode.Friendly && Config.Instance.RecordFriendly
			   || Game.CurrentGameMode == Game.GameMode.Casual && Config.Instance.RecordCasual)
			{
				if(Game.CurrentGameStats != null)
				{
					Game.CurrentGameStats.GameMode = Game.CurrentGameMode;
					Logger.WriteLine("Set gamemode to " + Game.CurrentGameMode);
				}

				if(_assignedDeck == null)
				{
					Logger.WriteLine("Saving DefaultDeckStats", "GameStats");
					DefaultDeckStats.Save();
				}
				else
				{
					Logger.WriteLine("Saving DeckStats", "GameStats");
					DeckStatsList.Save();
				}
				Game.CurrentGameStats = null;
				Helper.MainWindow.DeckPickerList.Items.Refresh();
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
		}

		public static void HandleOpponentHeroPower(string cardId, int turn)
		{
			LogEvent("OpponentHeroPower", cardId, turn);
			Game.AddPlayToCurrentGame(PlayType.OpponentHeroPower, turn, cardId);
		}


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

        void IGameHandler.HandleOpponentGet(int turn)
        {
            HandleOpponentGet(turn);
        }

        void IGameHandler.HandleOpponentSecretPlayed(string cardId, int @from, int turn, bool fromDeck)
        {
            HandleOpponentSecretPlayed(cardId, @from, turn, fromDeck);
        }

        void IGameHandler.HandleOpponentPlayToHand(string cardId, int turn)
        {
            HandleOpponentPlayToHand(cardId, turn);
        }

        void IGameHandler.HandleOpponentSecretTrigger(string cardId, int turn)
        {
            HandleOpponentSecretTrigger(cardId, turn);
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

        void IGameHandler.TurnStart(Turn player, int turnNumber)
        {
            TurnStart(player, turnNumber);
        }

        void IGameHandler.HandleGameStart()
        {
            HandleGameStart();
        }

        void IGameHandler.HandleGameEnd(bool backInMenu)
        {
            HandleGameEnd(backInMenu);
        }

        void IGameHandler.HandleLoss()
        {
            HandleLoss();
        }

        void IGameHandler.HandleWin()
        {
            HandleWin();
        }

        void IGameHandler.PlayerSetAside(string id)
        {
            PlayerSetAside(id);
        }

        void IGameHandler.HandlePlayerGet(string cardId, int turn)
        {
            HandlePlayerGet(cardId, turn);
        }

        #endregion IGameHandlerImplementation

	}
}