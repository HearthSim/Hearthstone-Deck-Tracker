using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		public void UpdatePlayerLayout()
		{
			StackPanelPlayer.Children.Clear();
			foreach(var item in Config.Instance.DeckPanelOrderPlayer)
			{
				switch(item)
				{
					case DeckPanel.DrawChances:
						StackPanelPlayer.Children.Add(CanvasPlayerChance);
						break;
					case DeckPanel.CardCounter:
						StackPanelPlayer.Children.Add(CanvasPlayerCount);
						break;
					case DeckPanel.Fatigue:
						StackPanelPlayer.Children.Add(LblPlayerFatigue);
						break;
					case DeckPanel.DeckTitle:
						StackPanelPlayer.Children.Add(LblDeckTitle);
						break;
					case DeckPanel.Wins:
						StackPanelPlayer.Children.Add(LblWins);
						break;
					case DeckPanel.Cards:
						StackPanelPlayer.Children.Add(ViewBoxPlayer);
						break;
				}
			}
		}

		public void UpdateOpponentLayout()
		{
			StackPanelOpponent.Children.Clear();
			foreach (var item in Config.Instance.DeckPanelOrderOpponent)
			{
				switch (item)
				{
					case DeckPanel.DrawChances:
						StackPanelOpponent.Children.Add(CanvasOpponentChance);
						break;
					case DeckPanel.CardCounter:
						StackPanelOpponent.Children.Add(CanvasOpponentCount);
						break;
					case DeckPanel.Fatigue:
						StackPanelOpponent.Children.Add(LblOpponentFatigue);
						break;
					case DeckPanel.Winrate:
						StackPanelOpponent.Children.Add(LblWinRateAgainst);
						break;
					case DeckPanel.Cards:
						StackPanelOpponent.Children.Add(ViewBoxOpponent);
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

		private void SetDeckTitle() => LblDeckTitle.Text = DeckList.Instance.ActiveDeck?.Name ?? "";

		private void SetOpponentCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblOpponentCardCount.Text = cardCount.ToString();
			LblOpponentDeckCount.Text = cardsLeftInDeck.ToString();

			if (cardsLeftInDeck <= 0)
			{
				LblOpponentFatigue.Text = LocUtil.Get(LocFatigue) + " " + (_game.Opponent.Fatigue + 1);

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
				LblPlayerFatigue.Text = LocUtil.Get(LocFatigue) + " " + (_game.Player.Fatigue + 1);

				LblDrawChance2.Text = "0%";
				LblDrawChance1.Text = "0%";
				return;
			}
			LblPlayerFatigue.Text = "";

			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblDrawChance2.Text = (cardsLeftInDeck == 1 ? 100 : drawNextTurn2) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		public void UpdatePlayerCards(List<Card> cards, bool reset) => ListViewPlayer.Update(cards, reset);

		public void UpdateOpponentCards(List<Card> cards, bool reset) => ListViewOpponent.Update(cards, reset);
	}
}
