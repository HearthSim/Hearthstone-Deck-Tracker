﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for PlayerWindow.xaml
	/// </summary>
	public partial class OpponentWindow : INotifyPropertyChanged
	{
		public static double Scaling = 1.0;
	    private readonly GameV2 _game;
		private bool _appIsClosing;

		public OpponentWindow(GameV2 game)
		{
			InitializeComponent();
		    _game = game;
			//ListViewOpponent.ItemsSource = opponentDeck;
			//opponentDeck.CollectionChanged += OpponentDeckOnCollectionChanged;
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
			Update();
		}

		public List<Card> OpponentDeck
		{
			get { return _game.Opponent.DisplayRevealedCards; }
		}

		public bool ShowToolTip
		{
			get { return Config.Instance.WindowCardToolTips; }
		}

		public void Update()
		{
			LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			CanvasOpponentChance.Visibility = Config.Instance.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
			CanvasOpponentCount.Visibility = Config.Instance.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Visibility.Collapsed : Visibility.Visible;

			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			if(!string.IsNullOrEmpty(_game.Opponent.Class))
			{
				var winsVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == _game.Opponent.Class);
				var lossesVs = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == _game.Opponent.Class);
				var percent = (winsVs + lossesVs) > 0 ? Math.Round(winsVs * 100.0 / (winsVs + lossesVs), 0).ToString(CultureInfo.InvariantCulture) : "-";
				LblWinRateAgainst.Text = string.Format("VS {0}: {1} - {2} ({3}%)", _game.Opponent.Class, winsVs, lossesVs, percent);
			}
		}

		public void UpdateOpponentLayout()
		{
			StackPanelMain.Children.Clear();
			foreach(var item in Config.Instance.PanelOrderOpponent)
			{
				switch(item)
				{
					case "Cards":
						StackPanelMain.Children.Add(ListViewOpponent);
						break;
					case "Draw Chances":
						StackPanelMain.Children.Add(CanvasOpponentChance);
						break;
					case "Card Counter":
						StackPanelMain.Children.Add(CanvasOpponentCount);
						break;
					case "Fatigue Counter":
						StackPanelMain.Children.Add(LblOpponentFatigue);
						break;
					case "Win Rate":
						StackPanelMain.Children.Add(ViewboxWinRateAgainst);
						break;
				}
			}
		}

		public void SetOpponentCardCount(int cardCount, int cardsLeftInDeck, bool opponentHasCoin)
		{
			LblOpponentCardCount.Text = cardCount.ToString();
			LblOpponentDeckCount.Text = cardsLeftInDeck.ToString();

			if(cardsLeftInDeck <= 0)
			{
				LblOpponentFatigue.Text = "Next draw fatigues for: " + (_game.Opponent.Fatigue + 1);

				LblOpponentDrawChance2.Text = "0%";
				LblOpponentDrawChance1.Text = "0%";
				LblOpponentHandChance2.Text = cardCount <= 0 ? "0%" : "100%";
				LblOpponentHandChance1.Text = cardCount <= 0 ? "0%" : "100%";
				return;
			}

			LblOpponentFatigue.Text = "";

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

		private void OpponentDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Scale();
		}

		private void Scale()
		{
			const int offsetToMakeSureGraphicsAreNotClipped = 40;
			var allLabelsHeight = CanvasOpponentCount.ActualHeight + CanvasOpponentChance.ActualHeight + LblWinRateAgainst.ActualHeight + LblOpponentFatigue.ActualHeight + offsetToMakeSureGraphicsAreNotClipped;
			if(((Height - allLabelsHeight) - (ListViewOpponent.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
			{
				var previousScaling = Scaling;
				Scaling = (Height - allLabelsHeight) / (ListViewOpponent.Items.Count * 35);
				if(Scaling > 1)
					Scaling = 1;

				if(previousScaling != Scaling)
					ListViewOpponent.Items.Refresh();
			}
		}

		private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			Scale();
			ListViewOpponent.Items.Refresh();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(_appIsClosing)
				return;
			e.Cancel = true;
			Hide();
		}

		private void Window_Activated_1(object sender, EventArgs e)
		{
			Scale();
			ListViewOpponent.Items.Refresh();
			Topmost = true;
		}

		internal void Shutdown()
		{
			_appIsClosing = true;
			Close();
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		public void SetTextLocation(bool top)
		{
			StackPanelMain.Children.Clear();
			if(top)
			{
				StackPanelMain.Children.Add(CanvasOpponentChance);
				StackPanelMain.Children.Add(CanvasOpponentCount);
				StackPanelMain.Children.Add(ListViewOpponent);
			}
			else
			{
				StackPanelMain.Children.Add(ListViewOpponent);
                StackPanelMain.Children.Add(CanvasOpponentChance);
                StackPanelMain.Children.Add(CanvasOpponentCount);
            }
		}

		private DateTime _lastOpponentUpdateReqest = DateTime.MinValue;
		public async void UpdateOpponentCards()
		{
			_lastOpponentUpdateReqest = DateTime.Now;
			await Task.Delay(50);
			if((DateTime.Now - _lastOpponentUpdateReqest).Milliseconds < 50)
				return;
			OnPropertyChanged("OpponentDeck");
			Scale();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}