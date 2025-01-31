#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Point = System.Drawing.Point;
using Panel = System.Windows.Controls.Panel;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for PlayerWindow.xaml
	/// </summary>
	public partial class OpponentWindow : INotifyPropertyChanged
	{
		private readonly GameV2 _game;
		private bool _appIsClosing;

		public OpponentWindow(GameV2 game)
		{
			InitializeComponent();
			_game = game;
			Height = Config.Instance.OpponentWindowHeight;
			if(Config.Instance.OpponentWindowLeft.HasValue)
				Left = Config.Instance.OpponentWindowLeft.Value;
			if(Config.Instance.OpponentWindowTop.HasValue)
				Top = Config.Instance.OpponentWindowTop.Value;
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
		}

		public List<Card> OpponentDeck => _game.Opponent.OpponentCardList;

		public bool ShowToolTip => Config.Instance.WindowCardToolTips;

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Update()
		{
			LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			CanvasOpponentChance.Visibility = Config.Instance.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
			CanvasOpponentCount.Visibility = Config.Instance.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Visibility.Collapsed : Visibility.Visible;

			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			if(!string.IsNullOrEmpty(_game.Opponent.OriginalClass))
			{
				var winsVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == _game.Opponent.OriginalClass);
				var lossesVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == _game.Opponent.OriginalClass);
				var percent = (winsVs + lossesVs) > 0
					              ? Math.Round(winsVs * 100.0 / (winsVs + lossesVs), 0).ToString(CultureInfo.InvariantCulture) : "-";
				LblWinRateAgainst.Text = $"VS {_game.Opponent.OriginalClass}: {winsVs}-{lossesVs} ({percent}%)";
			}
		}

		public void UpdateOpponentLayout()
		{
			StackPanelMain.Children.Clear();
			foreach(var item in Config.Instance.DeckPanelOrderOpponent)
			{
				switch(item)
				{
					case DeckPanel.Cards:
						StackPanelMain.Children.Add(ViewBoxOpponent);
						break;
					case DeckPanel.DrawChances:
						StackPanelMain.Children.Add(CanvasOpponentChance);
						break;
					case DeckPanel.CardCounter:
						StackPanelMain.Children.Add(CanvasOpponentCount);
						break;
					case DeckPanel.Fatigue:
						StackPanelMain.Children.Add(LblOpponentFatigue);
						break;
					case DeckPanel.Winrate:
						StackPanelMain.Children.Add(LblWinRateAgainst);
						break;
				}
			}
			OnPropertyChanged(nameof(OpponentDeckMaxHeight));
		}

		public void SetOpponentCardCount(int cardCount, int cardsLeftInDeck, bool opponentHasCoin)
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

			var handWithoutCoin = cardCount - (opponentHasCoin ? 1 : 0);

			var holdingNextTurn2 = Math.Round(100.0f * Helper.DrawProbability(2, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
			LblOpponentDrawChance2.Text = drawNextTurn2 + "%";
			LblOpponentHandChance2.Text = holdingNextTurn2 + "%";

			var holdingNextTurn = Math.Round(100.0f * Helper.DrawProbability(1, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
			var drawNextTurn = Math.Round(100.0f / cardsLeftInDeck, 1);
			LblOpponentDrawChance1.Text = drawNextTurn + "%";
			LblOpponentHandChance1.Text = holdingNextTurn + "%";
		}

		public double OpponentDeckMaxHeight =>  ActualHeight - OpponentLabelsHeight;

		public double OpponentLabelsHeight => CanvasOpponentChance.ActualHeight + CanvasOpponentCount.ActualHeight
			+ LblOpponentFatigue.ActualHeight + LblWinRateAgainst.ActualHeight + 42;

		private void OpponentWindow_OnSizeChanged(object sender, SizeChangedEventArgs e) => OnPropertyChanged(nameof(OpponentDeckMaxHeight));

		private void OpponentWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
			{
				if(!double.IsNaN(Left))
					Config.Instance.OpponentWindowLeft = (int)Left;
				if(!double.IsNaN(Top))
					Config.Instance.OpponentWindowTop = (int)Top;
				if(!double.IsNaN(Height) && Height > 0)
					Config.Instance.OpponentWindowHeight = (int)Height;
			}
			else
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void OpponentWindow_OnActivated(object sender, EventArgs e) => Topmost = true;

		private void OpponentWindow_OnDeactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		public void UpdateOpponentCards(List<Card> cards, bool reset) => ListViewOpponent.Update(cards, reset);

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OpponentWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			Update();
			UpdateOpponentLayout();
		}

		public void UpdateCardFrames()
		{
			CanvasOpponentChance.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
			CanvasOpponentCount.GetBindingExpression(Panel.BackgroundProperty)?.UpdateTarget();
		}
	}
}
