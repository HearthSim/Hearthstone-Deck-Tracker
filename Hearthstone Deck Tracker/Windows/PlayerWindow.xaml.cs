using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;

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
			if (_config.PlayerWindowLeft.HasValue)
			{
				Left = _config.PlayerWindowLeft.Value;
			}
			if (_config.PlayerWindowTop.HasValue)
			{
				Top = _config.PlayerWindowTop.Value;
			}
			Topmost = _config.WindowsTopmost;

			LblDrawChance1.Visibility = _config.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblDrawChance2.Visibility = _config.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblCardCount.Visibility = _config.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblDeckCount.Visibility = _config.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			ListViewPlayer.Visibility = _config.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;

			if (forScreenshot)
			{
				StackPanelDraw.Visibility = Visibility.Collapsed;
				StackPanelCount.Visibility = Visibility.Collapsed;

				Height = 34*ListViewPlayer.Items.Count;
				Scale();
			}
		}

		public bool ShowToolTip
		{
			get { return _config.WindowCardToolTips; }
		}

		public void SetCardCount(int cardCount, int cardsLeftInDeck)
		{
			LblCardCount.Text = "Hand: " + cardCount;
			LblDeckCount.Text = "Deck: " + cardsLeftInDeck;

			if (cardsLeftInDeck <= 0) return;

			LblDrawChance2.Text = "[2]: " + Math.Round(200.0f/cardsLeftInDeck, 2) + "%";
			LblDrawChance1.Text = "[1]: " + Math.Round(100.0f/cardsLeftInDeck, 2) + "%";
		}

		private void PlayerDeckOnCollectionChanged(object sender,
		                                           NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Scale();
		}

		private void Scale()
		{
			if (((Height - LblDrawChance1.ActualHeight - LblDeckCount.ActualHeight) - (ListViewPlayer.Items.Count*35*Scaling)) <
			    1 || Scaling < 1)
			{
				var previousScaling = Scaling;
				Scaling = (Height - LblDrawChance1.ActualHeight - LblDeckCount.ActualHeight)/(ListViewPlayer.Items.Count*35);
				if (Scaling > 1)
					Scaling = 1;

				if (previousScaling != Scaling)
					ListViewPlayer.Items.Refresh();
			}
		}

		private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			if (_forScreenshot) return;
			Scale();
			ListViewPlayer.Items.Refresh();
			_config.PlayerWindowHeight = (int) Height;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_appIsClosing) return;
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
			if (!_config.WindowsTopmost)
				Topmost = false;
		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			if (_forScreenshot) return;
			if (WindowState == WindowState.Minimized) return;
			_config.PlayerWindowLeft = (int) Left;
			_config.PlayerWindowTop = (int) Top;
		}

		public void SetTextLocation(bool top)
		{
			StackPanelMain.Children.Clear();
			if (top)
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
