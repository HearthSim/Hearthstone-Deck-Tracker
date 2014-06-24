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
    public partial class OpponentWindow
    {
        public static double Scaling = 1.0;
        private bool _appIsClosing;
        private readonly Config _config;
        public OpponentWindow(Config config, ObservableCollection<Card> opponentDeck)
        {
            InitializeComponent();
            _config = config;
            ListViewOpponent.ItemsSource = opponentDeck;
            opponentDeck.CollectionChanged += OpponentDeckOnCollectionChanged;
            Height = (_config.OpponentWindowHeight == 0) ? 400 : _config.OpponentWindowHeight;
            if (_config.OpponentWindowLeft != 0 && _config.OpponentWindowLeft != -32000)
            {
                Left = _config.OpponentWindowLeft;
            }
            if (_config.OpponentWindowTop != 0 && _config.OpponentWindowTop != -32000)
            {
                Top = _config.OpponentWindowTop;
            }
            Topmost = _config.WindowsTopmost;

            LblOpponentDrawChance1.Visibility = _config.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
            LblOpponentDrawChance2.Visibility = _config.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
            LblOpponentCardCount.Visibility = _config.HideEnemyCardCount ? Visibility.Collapsed : Visibility.Visible;
            LblOpponentDeckCount.Visibility = _config.HideEnemyCardCount ? Visibility.Collapsed : Visibility.Visible;
        }
        public void SetOpponentCardCount(int cardCount, int cardsLeftInDeck, bool opponentHasCoin)
        {
            LblOpponentCardCount.Text = "Hand: " + cardCount;
            LblOpponentDeckCount.Text = "Deck: " + cardsLeftInDeck;

            if (cardsLeftInDeck <= 0) return;

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
            var allLabelsHeight = LblOpponentCardCount.ActualHeight + LblOpponentDrawChance1.ActualHeight +
                                  LblOpponentDrawChance2.ActualHeight;
            if (((Height - allLabelsHeight) - (ListViewOpponent.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
            {
                var previousScaling = Scaling;
                Scaling = (Height - allLabelsHeight) / (ListViewOpponent.Items.Count * 35);
                if (Scaling > 1)
                    Scaling = 1;

                if (previousScaling != Scaling)
                    ListViewOpponent.Items.Refresh();
            }
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            Scale();
            ListViewOpponent.Items.Refresh();
            _config.OpponentWindowHeight = (int)Height;
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
            if (!_config.WindowsTopmost)
                Topmost = false;
        }

        private void MetroWindow_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized) return;
            _config.OpponentWindowLeft = Left;
            _config.OpponentWindowTop = Top;
        }

        public void SetTextLocation(bool top)
        {
            StackPanelMain.Children.Clear();
            if (top)
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
