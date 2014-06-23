using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow
    {
        public static double Scaling = 1.0;
        private bool _appIsClosing;
        private readonly Config _config;

        public PlayerWindow(Config config, ObservableCollection<Card> playerDeck)
        {
            InitializeComponent();
            _config = config;
            ListViewPlayer.ItemsSource = playerDeck;
            playerDeck.CollectionChanged += PlayerDeckOnCollectionChanged;
            Height = (_config.PlayerWindowHeight == 0) ? 400 : _config.PlayerWindowHeight;
            if (_config.PlayerWindowLeft != 0 && _config.PlayerWindowLeft != -32000)
            {
                Left = _config.PlayerWindowLeft;
            }
            if (_config.PlayerWindowTop != 0 && _config.PlayerWindowTop != -32000)
            {
                Top = _config.PlayerWindowTop;
            }
            Topmost = _config.WindowsTopmost;
            
        }

        public void SetCardCount(int cardCount, int cardsLeftInDeck)
        {
            LblCardCount.Text = "Hand: " + cardCount;
            LblDeckCount.Text = "Deck: " + cardsLeftInDeck;

            if (cardsLeftInDeck <= 0) return;

            LblDrawChance2.Text = "[2]: " + Math.Round(200.0f/cardsLeftInDeck, 2) + "%";
            LblDrawChance1.Text = "[1]: " + Math.Round(100.0f/cardsLeftInDeck, 2) + "%";
        }

        private void PlayerDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Scale();
        }

        private void Scale()
        {
            if (((Height - LblDrawChance1.ActualHeight - LblDeckCount.ActualHeight) - (ListViewPlayer.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
            {
                var previousScaling = Scaling;
                Scaling = (Height - LblDrawChance1.ActualHeight - LblDeckCount.ActualHeight) / (ListViewPlayer.Items.Count * 35);
                if (Scaling > 1)
                    Scaling = 1;

                if (previousScaling != Scaling)
                    ListViewPlayer.Items.Refresh();
            }
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            Scale();
            ListViewPlayer.Items.Refresh();
            _config.PlayerWindowHeight = (int)Height;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
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
            if (WindowState == WindowState.Minimized) return;
            _config.PlayerWindowLeft = Left;
            _config.PlayerWindowTop = Top;
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
