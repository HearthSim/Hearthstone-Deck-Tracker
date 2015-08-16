#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for PlayerWindow.xaml
	/// </summary>
	public partial class OpponentWindow
	{
		public static double Scaling = 1.0;
		private readonly Config _config;
		private bool _appIsClosing;

		public OpponentWindow(Config config, ObservableCollection<Card> opponentDeck)
		{
			InitializeComponent();
			_config = config;
			ListViewOpponent.ItemsSource = opponentDeck;
			opponentDeck.CollectionChanged += OpponentDeckOnCollectionChanged;
			Height = _config.OpponentWindowHeight;
			if(_config.OpponentWindowLeft.HasValue)
				Left = _config.OpponentWindowLeft.Value;
			if(_config.OpponentWindowTop.HasValue)
				Top = _config.OpponentWindowTop.Value;
			Topmost = _config.WindowsTopmost;

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

		public bool ShowToolTip
		{
			get { return _config.WindowCardToolTips; }
		}

		public void Update()
		{
			LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && Game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			LblOpponentDrawChance1.Visibility = _config.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblOpponentDrawChance2.Visibility = _config.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblOpponentCardCount.Visibility = _config.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblOpponentDeckCount.Visibility = _config.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewOpponent.Visibility = _config.HideOpponentCards ? Visibility.Collapsed : Visibility.Visible;

			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			if(Game.PlayingAgainst != string.Empty)
			{
				var winsVS = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == Game.PlayingAgainst);
				var lossesVS = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == Game.PlayingAgainst);
				var percent = (winsVS + lossesVS) > 0 ? Math.Round(winsVS * 100.0 / (winsVS + lossesVS), 0).ToString() : "-";
				LblWinRateAgainst.Text = string.Format("VS {0}: {1} - {2} ({3}%)", Game.PlayingAgainst, winsVS, lossesVS, percent);
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
						StackPanelMain.Children.Add(LblOpponentDrawChance1);
						StackPanelMain.Children.Add(LblOpponentDrawChance2);
						break;
					case "Card Counter":
						StackPanelMain.Children.Add(StackPanelCount);
						break;
					case "Fatigue Counter":
						StackPanelMain.Children.Add(StackPanelOpponentFatigue);
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
				LblOpponentFatigue.Text = "Next draw fatigues for: " + (Game.OpponentFatigueCount + 1);

				LblOpponentDrawChance2.Text = cardCount <= 0 ? "[2]: -% / -%" : "[2]: 100% / -%";
				LblOpponentDrawChance1.Text = cardCount <= 0 ? "[1]: -% / -%" : "[1]: 100% / -%";
				return;
			}

			LblOpponentFatigue.Text = "";

			var handWithoutCoin = cardCount - (opponentHasCoin ? 1 : 0);

			var holdingNextTurn2 = Math.Round(100.0f * Helper.DrawProbability(2, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 2);
			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 2);
			LblOpponentDrawChance2.Text = "[2]: " + holdingNextTurn2 + "% / " + drawNextTurn2 + "%";

			var holdingNextTurn = Math.Round(100.0f * Helper.DrawProbability(1, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 2);
			var drawNextTurn = Math.Round(100.0f / cardsLeftInDeck, 2);
			LblOpponentDrawChance1.Text = "[1]: " + holdingNextTurn + "% / " + drawNextTurn + "%";
		}

		private void OpponentDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Scale();
		}

		private void Scale()
		{
			var allLabelsHeight = LblOpponentCardCount.ActualHeight + LblOpponentDrawChance1.ActualHeight + LblOpponentDrawChance2.ActualHeight
			                      + LblWinRateAgainst.ActualHeight;
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
			if(!_config.WindowsTopmost)
				Topmost = false;
		}

		public void SetTextLocation(bool top)
		{
			StackPanelMain.Children.Clear();
			if(top)
			{
				StackPanelMain.Children.Add(LblOpponentDrawChance2);
				StackPanelMain.Children.Add(LblOpponentDrawChance1);
				StackPanelMain.Children.Add(StackPanelCount);
				StackPanelMain.Children.Add(ListViewOpponent);
			}
			else
			{
				StackPanelMain.Children.Add(ListViewOpponent);
				StackPanelMain.Children.Add(LblOpponentDrawChance2);
				StackPanelMain.Children.Add(LblOpponentDrawChance1);
				StackPanelMain.Children.Add(StackPanelCount);
			}
		}
	}
}