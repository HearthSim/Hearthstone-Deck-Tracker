#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for PlayerWindow.xaml
	/// </summary>
	public partial class PlayerWindow
	{
		private readonly GameV2 _game;

		public PlayerWindow(GameV2 game, List<Hearthstone.Card>? forScreenshot = null)
		{
			InitializeComponent();
			_game = game;
			Height = Config.Instance.PlayerWindowHeight;
			if(Config.Instance.PlayerWindowLeft.HasValue)
				Left = Config.Instance.PlayerWindowLeft.Value;
			if(Config.Instance.PlayerWindowTop.HasValue)
				Top = Config.Instance.PlayerWindowTop.Value;
			Topmost = Config.Instance.WindowsTopmost;

			var titleBarCorners = new[]
			{
				new Point((int)Left + 5, (int)Top + 5),
				new Point((int)(Left + Width) - 5, (int)Top + 5),
				new Point((int)Left + 5, (int)(Top + TitlebarHeight) - 5),
				new Point((int)(Left + Width) - 5, (int)(Top + TitlebarHeight) - 5)
			};
			if(!Screen.AllScreens.Any(s => titleBarCorners.Any(c => s.WorkingArea.Contains(c))))
			{
				Top = 100;
				Left = 100;
			}

			if(forScreenshot != null)
			{
				CanvasPlayerChance.Visibility = Visibility.Collapsed;
				CanvasPlayerCount.Visibility = Visibility.Collapsed;
				LblWins.Visibility = Visibility.Collapsed;
				LblDeckTitle.Visibility = Visibility.Collapsed;
				ListViewPlayer.Update(forScreenshot, true);

				var items = ListViewPlayer.AnimatedCards;
				var margins = items.Aggregate(0.0, (c, n) => c + n.Margin.Top + n.Margin.Bottom);
				Height = ListViewPlayer.ViewModel.MaxHeightCard * items.Count + margins;
			}

			DeckList.Instance.ActiveDeckChanged += _ =>
			{
				ListViewPlayer.ItemsControl.Items.Refresh();
			};


			{
				CanvasPlayerChance.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
				CanvasPlayerCount.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
			};
		}

		public void Update()
		{
			CanvasPlayerChance.Visibility = Config.Instance.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			CanvasPlayerCount.Visibility = Config.Instance.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblPlayerFatigue.Visibility = Config.Instance.HidePlayerFatigueCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewPlayer.Visibility = Config.Instance.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;

			LblWins.Visibility = Config.Instance.ShowDeckWins && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;

			SetDeckTitle();
			SetWinRates();
		}

		private void SetWinRates()
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			LblWins.Text = $"{selectedDeck.WinLossString} ({selectedDeck.WinPercentString})";
		}

		private void SetDeckTitle() => LblDeckTitle.Text = DeckList.Instance.ActiveDeck?.Name ?? string.Empty;

		public void UpdatePlayerLayout()
		{
			StackPanelMain.Children.Clear();
			foreach(var item in Config.Instance.DeckPanelOrderLocalPlayer)
			{
				switch(item)
				{
					case DeckPanel.Cards:
						StackPanelMain.Children.Add(ListViewPlayer);
						break;
					case DeckPanel.CardsTop:
						StackPanelMain.Children.Add(PlayerTopDeckLens);
						break;
					case DeckPanel.CardsBottom:
						StackPanelMain.Children.Add(PlayerBottomDeckLens);
						break;
					case DeckPanel.Sideboards:
						StackPanelMain.Children.Add(PlayerSideboards);
						break;
					case DeckPanel.CardCounter:
						StackPanelMain.Children.Add(CanvasPlayerCount);
						break;
					case DeckPanel.DrawChances:
						StackPanelMain.Children.Add(CanvasPlayerChance);
						break;
					case DeckPanel.Fatigue:
						StackPanelMain.Children.Add(LblPlayerFatigue);
						break;
					case DeckPanel.DeckTitle:
						StackPanelMain.Children.Add(LblDeckTitle);
						break;
					case DeckPanel.Wins:
						StackPanelMain.Children.Add(LblWins);
						break;
				}
			}
		}

		public void SetCardCount(int cardCount, int cardsLeftInDeck)
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

			LblDrawChance2.Text = Math.Round(200.0f / cardsLeftInDeck, 1) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		private void PlayerWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
			{
				if(!double.IsNaN(Left))
					Config.Instance.PlayerWindowLeft = (int)Left;
				if(!double.IsNaN(Top))
					Config.Instance.PlayerWindowTop = (int)Top;
				if(!double.IsNaN(Height) && Height > 0)
					Config.Instance.PlayerWindowHeight = (int)Height;
			}
			else
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void PlayerWindow_OnActivated(object sender, EventArgs e) => Topmost = true;

		private void PlayerWindow_OnDeactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		public async Task UpdatePlayerCards(List<Hearthstone.Card> cards, bool reset, List<Hearthstone.Card> top, List<Hearthstone.Card> bottom, List<Sideboard> sideboards)
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

		private void ListViewPlayerCard_OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
		{
			if(sender is CardTile { DataContext: CardTileViewModel vm })
			{
				HighlightPlayerDeckCards(vm.Card.Id);
			}
		}

		private void ListViewPlayerCard_OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
		{
			HighlightPlayerDeckCards(null);
		}

		private void PlayerWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			Update();
			UpdatePlayerLayout();
		}
	}
}
