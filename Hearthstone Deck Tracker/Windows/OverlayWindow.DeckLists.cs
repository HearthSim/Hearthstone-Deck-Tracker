using System;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private const string DeckPanelCards = "Cards";
		private const string DeckPanelDrawChances = "Draw Chances";
		private const string DeckPanelCardCounter = "Card Counter";
		private const string DeckPanelFatigueCounter = "Fatigue Counter";
		private const string DeckPanelDeckTitle = "Deck Title";
		private const string DeckPanelWins = "Wins";
		private const string DeckPanelWinrate = "Win Rate";

		public void UpdatePlayerLayout()
		{
			StackPanelPlayer.Children.Clear();
			foreach (var item in Config.Instance.PanelOrderPlayer)
			{
				switch (item)
				{
					case DeckPanelCards:
						StackPanelPlayer.Children.Add(ListViewPlayer);
						break;
					case DeckPanelDrawChances:
						StackPanelPlayer.Children.Add(CanvasPlayerChance);
						break;
					case DeckPanelCardCounter:
						StackPanelPlayer.Children.Add(CanvasPlayerCount);
						break;
					case DeckPanelFatigueCounter:
						StackPanelPlayer.Children.Add(LblPlayerFatigue);
						break;
					case DeckPanelDeckTitle:
						StackPanelPlayer.Children.Add(LblDeckTitle);
						break;
					case DeckPanelWins:
						StackPanelPlayer.Children.Add(LblWins);
						break;
				}
			}
		}

		public void UpdateOpponentLayout()
		{
			StackPanelOpponent.Children.Clear();
			foreach (var item in Config.Instance.PanelOrderOpponent)
			{
				switch (item)
				{
					case DeckPanelCards:
						StackPanelOpponent.Children.Add(ListViewOpponent);
						break;
					case DeckPanelDrawChances:
						StackPanelOpponent.Children.Add(CanvasOpponentChance);
						break;
					case DeckPanelCardCounter:
						StackPanelOpponent.Children.Add(CanvasOpponentCount);
						break;
					case DeckPanelFatigueCounter:
						StackPanelOpponent.Children.Add(LblOpponentFatigue);
						break;
					case DeckPanelWinrate:
						StackPanelOpponent.Children.Add(ViewBoxWinRateAgainst);
						break;
				}
			}
		}

		private void SetWinRates()
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if (selectedDeck == null)
				return;

			LblWins.Text = $"{selectedDeck.WinLossString} ({selectedDeck.WinPercentString})";

			if (!string.IsNullOrEmpty(_game.Opponent.Class))
			{
				var winsVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == _game.Opponent.Class);
				var lossesVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == _game.Opponent.Class);
				var percent = (winsVs + lossesVs) > 0 ? Math.Round(winsVs * 100.0 / (winsVs + lossesVs), 0).ToString() : "-";
				LblWinRateAgainst.Text = $"VS {_game.Opponent.Class}: {winsVs}-{lossesVs} ({percent}%)";
			}
		}

		private void SetDeckTitle() => LblDeckTitle.Text = DeckList.Instance.ActiveDeckVersion?.Name ?? "";

		private void SetOpponentCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblOpponentCardCount.Text = cardCount.ToString();
			LblOpponentDeckCount.Text = cardsLeftInDeck.ToString();

			if (cardsLeftInDeck <= 0)
			{
				LblOpponentFatigue.Text = "Next draw fatigues for: " + (_game.Opponent.Fatigue + 1);

				LblOpponentDrawChance2.Text = "0%";
				LblOpponentDrawChance1.Text = "0%";
				LblOpponentHandChance2.Text = cardCount <= 0 ? "0%" : "100%";
				;
				LblOpponentHandChance1.Text = cardCount <= 0 ? "0%" : "100%";
				return;
			}
			LblOpponentFatigue.Text = "";

			var handWithoutCoin = cardCount - (_game.Opponent.HasCoin ? 1 : 0);

			var holdingNextTurn2 = Math.Round(100.0f * Helper.DrawProbability(2, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblOpponentDrawChance2.Text = (cardsLeftInDeck == 1 ? 100 : drawNextTurn2) + "%";
			LblOpponentHandChance2.Text = holdingNextTurn2 + "%";

			var holdingNextTurn = Math.Round(100.0f * Helper.DrawProbability(1, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
			var drawNextTurn = Math.Round(100.0f / cardsLeftInDeck, 1);
			LblOpponentDrawChance1.Text = drawNextTurn + "%";
			LblOpponentHandChance1.Text = holdingNextTurn + "%";
		}

		private void SetCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblCardCount.Text = cardCount.ToString();
			LblDeckCount.Text = cardsLeftInDeck.ToString();

			if (cardsLeftInDeck <= 0)
			{
				LblPlayerFatigue.Text = "Next draw fatigues for: " + (_game.Player.Fatigue + 1);

				LblDrawChance2.Text = "0%";
				LblDrawChance1.Text = "0%";
				return;
			}
			LblPlayerFatigue.Text = "";

			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblDrawChance2.Text = (cardsLeftInDeck == 1 ? 100 : drawNextTurn2) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		public async void UpdatePlayerCards()
		{
			_lastPlayerUpdateReqest = DateTime.Now;
			await Task.Delay(50);
			if ((DateTime.Now - _lastPlayerUpdateReqest).Milliseconds < 50)
				return;
			OnPropertyChanged(nameof(PlayerDeck));
		}

		public async void UpdateOpponentCards()
		{
			_lastOpponentUpdateReqest = DateTime.Now;
			await Task.Delay(50);
			if ((DateTime.Now - _lastOpponentUpdateReqest).Milliseconds < 50)
				return;
			OnPropertyChanged(nameof(OpponentDeck));
		}

	}
}
