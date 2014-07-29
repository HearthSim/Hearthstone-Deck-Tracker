#region

using System.Linq;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class GameEventHandler
	{
		#region Player

		public static void HandlePlayerGet(string cardId)
		{
			LogEvent("PlayerGet", cardId);
			Game.PlayerGet(cardId);
		}

		public static void HandlePlayerDraw(string cardId)
		{
			LogEvent("PlayerDraw", cardId);
			var correctDeck = Game.PlayerDraw(cardId);

			if (!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow._showIncorrectDeckMessage &&
			    !Helper.MainWindow._showingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				Helper.MainWindow._showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck");
			}
		}

		public static void HandlePlayerMulligan(string cardId)
		{
			LogEvent("PlayerMulligan", cardId);
			TurnTimer.Instance.MulliganDone(Turn.Player);
			Game.Mulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		public static void HandlePlayerHandDiscard(string cardId)
		{
			LogEvent("PlayerHandDiscard", cardId);
			Game.PlayerHandDiscard(cardId);
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		public static void HandlePlayerPlay(string cardId)
		{
			LogEvent("PlayerPlay", cardId);
			Game.PlayerPlayed(cardId);
			Helper.MainWindow._overlay.ListViewPlayer.Items.Refresh();
			Helper.MainWindow._playerWindow.ListViewPlayer.Items.Refresh();
		}

		public static void HandlePlayerDeckDiscard(string cardId)
		{
			LogEvent("PlayerDeckDiscard", cardId);
			var correctDeck = Game.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if (!correctDeck && Config.Instance.AutoDeckDetection && !Helper.MainWindow._showIncorrectDeckMessage &&
			    !Helper.MainWindow._showingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				Helper.MainWindow._showIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
			}
		}

		#endregion

		#region Opponent

		public static void HandleOpponentPlay(string id, int from, int turn)
		{
			LogEvent("OpponentPlay", id, turn, from);
			Game.OpponentPlay(id, from, turn);
		}

		public static void HandlOpponentDraw(int turn)
		{
			LogEvent("OpponentDraw", turn: turn);
			Game.OpponentDraw(turn);
		}

		public static void HandleOpponentMulligan(int from)
		{
			LogEvent("OpponentMulligan", from: from);
			Game.OpponentMulligan(from);
			TurnTimer.Instance.MulliganDone(Turn.Opponent);
		}

		public static void HandleOpponentGet(int turn)
		{
			LogEvent("OpponentGet", turn: turn);
			Game.OpponentGet(turn);
		}

		public static void HandleOpponentSecretPlayed()
		{
			LogEvent("OpponentSecretPlayed");
			Game.OpponentSecretCount++;
			Helper.MainWindow._overlay.ShowSecrets(Game.PlayingAgainst);
		}

		public static void HandleOpponentPlayToHand(string cardId, int turn)
		{
			LogEvent("OpponentBackToHand", cardId, turn);
			Game.OpponentBackToHand(cardId, turn);
		}

		public static void HandleOpponentSecretTrigger(string cardId)
		{
			LogEvent("OpponentSecretTrigger", cardId);
			Game.OpponentSecretTriggered(cardId);
			Game.OpponentSecretCount--;
			if (Game.OpponentSecretCount <= 0)
			{
				Helper.MainWindow._overlay.HideSecrets();
			}
		}

		public static void HandleOpponentDeckDiscard(string cardId)
		{
			LogEvent("OpponentDeckDiscard", cardId);
			Game.OpponentDeckDiscard(cardId);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			Helper.MainWindow._overlay.ListViewOpponent.Items.Refresh();
			Helper.MainWindow._opponentWindow.ListViewOpponent.Items.Refresh();
		}

		#endregion

		public static void SetOpponentHero(string hero)
		{
			Game.PlayingAgainst = hero;
			Logger.WriteLine("Playing against " + hero, "Hearthstone");
		}

		public static void TurnStart(Turn player, int turnNumber)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", player, turnNumber + 1), "LogReader");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			TurnTimer.Instance.SetCurrentPlayer(player);
			TurnTimer.Instance.Restart();
			if (player == Turn.Player && !Game.IsInMenu)
			{
				if (Config.Instance.FlashHs)
					User32.FlashHs();

				if (Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}
		}

		public static void HandleGameStart(string playerHero)
		{
			//avoid new game being started when jaraxxus is played
			if (!Game.IsInMenu) return;

			Game.PlayingAs = hero;

			Logger.WriteLine("Game start");

			if (Config.Instance.FlashHs)
				User32.FlashHs();
			if (Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if (Config.Instance.KeyPressOnGameStart != "None" &&
			    Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart);
			}

			var selectedDeck = Helper.MainWindow.DeckPickerList.SelectedDeck;
			if (selectedDeck != null)
				Game.SetPremadeDeck((Deck) selectedDeck.Clone());

			Game.IsInMenu = false;
			Game.Reset();

			//select deck based on hero
			if (!string.IsNullOrEmpty(hero))
			{
				if (!Game.IsUsingPremade || !Config.Instance.AutoDeckDetection) return;

				if (selectedDeck == null || selectedDeck.Class != Game.PlayingAs)
				{
					var classDecks = Helper.MainWindow._deckList.DecksList.Where(d => d.Class == Game.PlayingAs).ToList();
					if (classDecks.Count == 0)
					{
						Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
					}
					else if (classDecks.Count == 1)
					{
						Helper.MainWindow.DeckPickerList.SelectDeck(classDecks[0]);
						Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
					}
					else if (Helper.MainWindow._deckList.LastDeckClass.Any(ldc => ldc.Class == Game.PlayingAs))
					{
						var lastDeckName = Helper.MainWindow._deckList.LastDeckClass.First(ldc => ldc.Class == Game.PlayingAs).Name;
						Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

						var deck = Helper.MainWindow._deckList.DecksList.FirstOrDefault(d => d.Name == lastDeckName);

						if (deck != null)
						{
							Helper.MainWindow.DeckPickerList.SelectDeck(deck);
							Helper.MainWindow.UpdateDeckList(deck);
							Helper.MainWindow.UseDeck(deck);
						}
					}
				}
			}
		}

		public static void HandleGameEnd()
		{
			Logger.WriteLine("Game end");
			if (Config.Instance.KeyPressOnGameEnd != "None" &&
			    Helper.MainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd);
			}
			TurnTimer.Instance.Stop();
			Helper.MainWindow._overlay.HideTimers();
			Helper.MainWindow._overlay.HideSecrets();
			if (Config.Instance.SavePlayedGames && !Game.IsInMenu)
			{
				Helper.MainWindow.SavePlayedCards();
			}
			if (!Config.Instance.KeepDecksVisible)
			{
				var deck = Helper.MainWindow.DeckPickerList.SelectedDeck;
				if (deck != null)
					Game.SetPremadeDeck((Deck) deck.Clone());

				Game.Reset();
			}
			Game.IsInMenu = true;
		}

		private static void LogEvent(string type, string id = "", int turn = 0, int from = -1)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", type, id, turn, from), "LogReader");
		}
	}
}