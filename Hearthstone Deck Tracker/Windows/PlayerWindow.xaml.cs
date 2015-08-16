#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for PlayerWindow.xaml
	/// </summary>
	public partial class PlayerWindow
	{
		public static double Scaling = 1.0;
		private readonly Config _config;
		private readonly bool _forScreenshot;
		private bool _appIsClosing;

		public PlayerWindow(Config config, ObservableCollection<Card> playerDeck, bool forScreenshot = false)
		{
			InitializeComponent();
			_forScreenshot = forScreenshot;
			_config = config;
			ListViewPlayer.ItemsSource = playerDeck;
			playerDeck.CollectionChanged += PlayerDeckOnCollectionChanged;
			Height = _config.PlayerWindowHeight;
			if(_config.PlayerWindowLeft.HasValue)
				Left = _config.PlayerWindowLeft.Value;
			if(_config.PlayerWindowTop.HasValue)
				Top = _config.PlayerWindowTop.Value;
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


			if(forScreenshot)
			{
				StackPanelDraw.Visibility = Visibility.Collapsed;
				StackPanelCount.Visibility = Visibility.Collapsed;
				LblWins.Visibility = Visibility.Collapsed;
				LblDeckTitle.Visibility = Visibility.Collapsed;

				Height = 34 * ListViewPlayer.Items.Count;
				Scale();
			}
			else
				Update();
		}

		public bool ShowToolTip
		{
			get { return _config.WindowCardToolTips; }
		}

		public void Update()
		{
			LblDrawChance1.Visibility = _config.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblDrawChance2.Visibility = _config.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblCardCount.Visibility = _config.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblDeckCount.Visibility = _config.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewPlayer.Visibility = _config.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;
			LblWins.Visibility = Config.Instance.ShowDeckWins && Game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && Game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;

			SetDeckTitle();
			SetWinRates();
			Scale();
		}

		private void SetWinRates()
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			LblWins.Text = string.Format("{0} ({1})", selectedDeck.WinLossString, selectedDeck.WinPercentString);
		}

		private void SetDeckTitle()
		{
			var selectedDeck = DeckList.Instance.ActiveDeckVersion;
			LblDeckTitle.Text = selectedDeck != null ? selectedDeck.Name : string.Empty;
		}

		public void UpdatePlayerLayout()
		{
			StackPanelMain.Children.Clear();
			foreach(var item in Config.Instance.PanelOrderPlayer)
			{
				switch(item)
				{
					case "Cards":
						StackPanelMain.Children.Add(ListViewPlayer);
						break;
					case "Draw Chances":
						StackPanelMain.Children.Add(StackPanelDraw);
						break;
					case "Card Counter":
						StackPanelMain.Children.Add(StackPanelCount);
						break;
					case "Fatigue Counter":
						StackPanelMain.Children.Add(StackPanelPlayerFatigue);
						break;
					case "Deck Title":
						StackPanelMain.Children.Add(LblDeckTitle);
						break;
					case "Wins":
						StackPanelMain.Children.Add(LblWins);
						break;
				}
			}
		}

		public void SetCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblCardCount.Text = cardCount.ToString();
			LblDeckCount.Text = cardsLeftInDeck.ToString();

			if(cardsLeftInDeck <= 0)
			{
				LblPlayerFatigue.Text = "Next draw fatigues for: " + (Game.PlayerFatigueCount + 1);

				LblDrawChance2.Text = "[2]: -%";
				LblDrawChance1.Text = "[1]: -%";
				return;
			}

			LblPlayerFatigue.Text = "";

			LblDrawChance2.Text = "[2]: " + Math.Round(200.0f / cardsLeftInDeck, 2) + "%";
			LblDrawChance1.Text = "[1]: " + Math.Round(100.0f / cardsLeftInDeck, 2) + "%";
		}

		private void PlayerDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Scale();
		}

		private void Scale()
		{
			var allLabelsHeight = LblDrawChance1.ActualHeight + LblDeckCount.ActualHeight + LblWins.ActualHeight + LblDeckTitle.ActualHeight;
			if(((Height - allLabelsHeight) - (ListViewPlayer.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
			{
				var previousScaling = Scaling;
				Scaling = (Height - allLabelsHeight) / (ListViewPlayer.Items.Count * 35);
				if(Scaling > 1)
					Scaling = 1;

				if(previousScaling != Scaling)
					ListViewPlayer.Items.Refresh();
			}
		}

		private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			if(_forScreenshot)
				return;
			Scale();
			ListViewPlayer.Items.Refresh();
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
			ListViewPlayer.Items.Refresh();
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
				StackPanelMain.Children.Add(StackPanelDraw);
				StackPanelMain.Children.Add(StackPanelCount);
				StackPanelMain.Children.Add(ListViewPlayer);
			}
			else
			{
				StackPanelMain.Children.Add(ListViewPlayer);
				StackPanelMain.Children.Add(StackPanelDraw);
				StackPanelMain.Children.Add(StackPanelCount);
			}
		}
	}
}