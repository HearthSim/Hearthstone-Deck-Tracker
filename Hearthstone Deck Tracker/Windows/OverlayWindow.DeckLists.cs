﻿using System;
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
			foreach(var item in Config.Instance.DeckPanelOrderLocalPlayer)
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
					case DeckPanel.CardsTop:
						StackPanelPlayer.Children.Add(PlayerTopDeckLens);
						break;
					case DeckPanel.CardsBottom:
						StackPanelPlayer.Children.Add(PlayerBottomDeckLens);
						break;
					case DeckPanel.Sideboards:
						StackPanelPlayer.Children.Add(PlayerSideboards);
						break;
				}
			}
		}

		public void UpdateOpponentLayout()
		{
			StackPanelOpponent.Children.Clear();
			foreach(var item in Config.Instance.DeckPanelOrderOpponent)
			{
				switch(item)
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

			if(!Config.Instance.HideOpponentRelatedCards)
			{
				StackPanelOpponent.Children.Add(OpponentRelatedCardsDeckLens);
			}
		}

		public void SetWinRates()
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;

			LblWins.Text = $"{selectedDeck.WinLossString} ({selectedDeck.WinPercentString})";

			if(!string.IsNullOrEmpty(_game.Opponent.OriginalClass))
			{
				var winsVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == _game.Opponent.OriginalClass);
				var lossesVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == _game.Opponent.OriginalClass);
				var percent = (winsVs + lossesVs) > 0 ? Math.Round(winsVs * 100.0 / (winsVs + lossesVs), 0).ToString() : "-";
				LblWinRateAgainst.Text = $"VS {_game.Opponent.OriginalClass}: {winsVs}-{lossesVs} ({percent}%)";
			}
		}

		private void SetDeckTitle() => LblDeckTitle.Text = DeckList.Instance.ActiveDeck?.Name ?? "";

		private void SetOpponentCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblOpponentCardCount.Text = cardCount.ToString();
			LblOpponentDeckCount.Text = cardsLeftInDeck.ToString();

			var fatigueDamage = Math.Max(_game.Opponent.Fatigue + 1, 1);
			if(cardsLeftInDeck <= 0)
			{
				LblOpponentFatigue.Text = string.Format(
					LocUtil.Get("Overlay_DeckList_Label_FatigueNextDraw"),
					fatigueDamage
				);

				LblOpponentDrawChance2.Text = "0%";
				LblOpponentDrawChance1.Text = "0%";
				LblOpponentHandChance2.Text = cardCount <= 0 ? "0%" : "100%";
				LblOpponentHandChance1.Text = cardCount <= 0 ? "0%" : "100%";
				return;
			}
			else if(fatigueDamage > 1 || WotogCounterHelper.ShowOpponentFatigueCounter)
			{
				LblOpponentFatigue.Text = string.Format(
					LocUtil.Get("Overlay_DeckList_Label_FatigueDamage"),
					fatigueDamage
				);
			}
			else
			{
				LblOpponentFatigue.Text = "";
			}

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

			var fatigueDamage = Math.Max(_game.Player.Fatigue + 1, 1);
			if(cardsLeftInDeck <= 0)
			{
				LblPlayerFatigue.Text = string.Format(
					LocUtil.Get("Overlay_DeckList_Label_FatigueNextDraw"),
					fatigueDamage
				);

				LblDrawChance2.Text = "0%";
				LblDrawChance1.Text = "0%";
				return;
			}
			else if(fatigueDamage > 1 || WotogCounterHelper.ShowPlayerFatigueCounter)
			{
				LblPlayerFatigue.Text = string.Format(
					LocUtil.Get("Overlay_DeckList_Label_FatigueDamage"),
					fatigueDamage
				);
			}
			else
			{
				LblPlayerFatigue.Text = "";
			}

			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblDrawChance2.Text = (cardsLeftInDeck == 1 ? 100 : drawNextTurn2) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		public void UpdatePlayerCards(List<Card> cards, bool reset, List<Card> top, List<Card> bottom, List<Sideboard> sideboards)
		{
			ListViewPlayer.Update(cards, reset);
			PlayerTopDeckLens.Update(top, reset);
			PlayerBottomDeckLens.Update(bottom, reset);
			PlayerSideboards.Update(sideboards, reset);
		}

		public void UpdateOpponentCards(List<Card> cards, List<Card> cardsWithRelatedCards, bool reset)
		{
			ListViewOpponent.Update(cards, reset);
			OpponentRelatedCardsDeckLens.Update(cardsWithRelatedCards.Where(card => cards.All(c => c.Id != card.Id)).ToList(), reset);
		}
	}
}
