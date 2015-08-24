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
	    private readonly GameV2 _game;
	    private readonly bool _forScreenshot;
		private bool _appIsClosing;

		public PlayerWindow(GameV2 game, Config config, ObservableCollection<Card> playerDeck, bool forScreenshot = false)
		{
			InitializeComponent();
		    _game = game;
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
				CanvasPlayerChance.Visibility = Visibility.Collapsed;
				CanvasPlayerCount.Visibility = Visibility.Collapsed;
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
            CanvasPlayerChance.Visibility = _config.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			CanvasPlayerCount.Visibility = _config.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewPlayer.Visibility = _config.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;
			LblWins.Visibility = Config.Instance.ShowDeckWins && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
			LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;

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
						StackPanelMain.Children.Add(CanvasPlayerChance);
						break;
					case "Card Counter":
						StackPanelMain.Children.Add(CanvasPlayerCount);
						break;
					case "Fatigue Counter":
						StackPanelMain.Children.Add(LblPlayerFatigue);
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
				LblPlayerFatigue.Text = "Next draw fatigues for: " + (_game.PlayerFatigueCount + 1);

				LblDrawChance2.Text = "0%";
				LblDrawChance1.Text = "0%";
				return;
			}

			LblPlayerFatigue.Text = "";

			LblDrawChance2.Text = Math.Round(200.0f / cardsLeftInDeck, 1) + "%";
			LblDrawChance1.Text = Math.Round(100.0f / cardsLeftInDeck, 1) + "%";
		}

		private void PlayerDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Scale();
		}

		private void Scale()
		{
			const int offsetToMakeSureGraphicsAreNotClipped = 15;
			var allLabelsHeight = CanvasPlayerChance.ActualHeight + CanvasPlayerCount.ActualHeight + LblWins.ActualHeight + LblDeckTitle.ActualHeight + LblPlayerFatigue.ActualHeight + offsetToMakeSureGraphicsAreNotClipped;
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
				StackPanelMain.Children.Add(CanvasPlayerChance);
				StackPanelMain.Children.Add(CanvasPlayerCount);
				StackPanelMain.Children.Add(ListViewPlayer);
			}
			else
			{
				StackPanelMain.Children.Add(ListViewPlayer);
				StackPanelMain.Children.Add(CanvasPlayerChance);
				StackPanelMain.Children.Add(CanvasPlayerCount);
			}
		}
	}
}