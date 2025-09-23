using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using NuGet;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

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
					case DeckPanel.DeckTitle:
						StackPanelPlayer.Children.Add(LblDeckTitle);
						break;
					case DeckPanel.Wins:
						StackPanelPlayer.Children.Add(LblWins);
						break;
					case DeckPanel.Cards:
						StackPanelPlayer.Children.Add(ListViewPlayer);
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
					case DeckPanel.Winrate:
						StackPanelOpponent.Children.Add(LblWinRateAgainst);
						break;
					case DeckPanel.Cards:
						StackPanelOpponent.Children.Add(ListViewOpponent);
						break;
				}
			}

			if(!Config.Instance.HideOpponentArenaPackages)
			{
				StackPanelOpponent.Children.Add(OpponentPackageCardsDeckLens);
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
				LblOpponentDrawChance2.Text = "0%";
				LblOpponentDrawChance1.Text = "0%";
				LblOpponentHandChance2.Text = cardCount <= 0 ? "0%" : "100%";
				LblOpponentHandChance1.Text = cardCount <= 0 ? "0%" : "100%";
				return;
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

			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblDrawChance2.Text = (cardsLeftInDeck == 1 ? 100 : drawNextTurn2) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		public async Task UpdatePlayerCards(List<Card> cards, bool reset, List<Card> top, List<Card> bottom, List<Sideboard> sideboards)
		{
			var updates = new[]
			{
				ListViewPlayer.Update(cards, reset),
				PlayerTopDeckLens.Update(top, reset),
				PlayerBottomDeckLens.Update(bottom, reset),
				PlayerSideboards.Update(sideboards, reset),
			};
			await Task.WhenAll(updates);
		}

		public async Task UpdateOpponentCards(List<Card> cards, List<Card> cardsWithRelatedCards, (Card? packageKey, IEnumerable<Card> packageCards) arenaPackage, bool reset)
		{
			var arenaPacakges = arenaPackage.packageCards.Where(card => cards.All(c => c.Id != card.Id))
				.ToSortedCardList();

			// on current arena rotation, only 1 legendary is available per draft.
			// so if a legendary package is active, no legendary should appear on related cards
			var hasLegendaryPackage = !arenaPacakges.IsEmpty();
			var relatedCards = cardsWithRelatedCards.Where(card =>
					cards.All(c => c.Id != card.Id) &&
					arenaPacakges.All(c => c.Id != card.Id) &&
					!(hasLegendaryPackage && card.Rarity == Rarity.LEGENDARY)
				).ToSortedCardList();

			OpponentPackageCardsDeckLens.Label = string.Format(LocUtil.Get("Arena_Legendary_Group_Cards"), arenaPackage.packageKey?.LocalizedName ?? "");

			var updates = new[]
			{
				ListViewOpponent.Update(cards, reset),
				OpponentPackageCardsDeckLens.Update(arenaPacakges, reset),
				OpponentRelatedCardsDeckLens.Update(relatedCards, reset),
			};
			await Task.WhenAll(updates);
		}

		public async Task UpdateOpponentCards(List<Card> cards, List<Card> cardsWithRelatedCards, bool reset)
		{
			var updates = new[]
			{
				ListViewOpponent.Update(cards, reset),
				OpponentRelatedCardsDeckLens.Update(cardsWithRelatedCards.Where(card => cards.All(c => c.Id != card.Id)).ToList(), reset),
				OpponentPackageCardsDeckLens.Update(new List<Card>(), reset),
			};
			await Task.WhenAll(updates);
		}

		public void HighlightPlayerDeckCards(string? highlightSourceCardId)
		{
			if(string.IsNullOrEmpty(highlightSourceCardId) || Config.Instance.HidePlayerHighlightSynergies)
			{
				ListViewPlayer.ShouldHighlightCard = null;
				PlayerTopDeckLens.CardList.ShouldHighlightCard = null;
				PlayerBottomDeckLens.CardList.ShouldHighlightCard = null;
				return;
			}

			var highlightSourceCard = _game.RelatedCardsManager.GetCardWithHighlight(highlightSourceCardId!);
			ListViewPlayer.ShouldHighlightCard = highlightSourceCard != null ? highlightSourceCard.ShouldHighlight : null;
			PlayerTopDeckLens.CardList.ShouldHighlightCard = highlightSourceCard != null ? highlightSourceCard.ShouldHighlight : null;
			PlayerBottomDeckLens.CardList.ShouldHighlightCard = highlightSourceCard != null ? highlightSourceCard.ShouldHighlight : null;
		}

		private void ListViewPlayerCard_OnMouseEnter(object sender, MouseEventArgs e)
		{
			if(sender is CardTile { DataContext: CardTileViewModel vm })
			{
				HighlightPlayerDeckCards(vm.Card.Id);
			}

		}

		private void ListViewPlayerCard_OnMouseLeave(object sender, MouseEventArgs e)
		{
			HighlightPlayerDeckCards(null);
		}
	}
}
