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
            Topmost = _config.WindowsTopmost; 
            if (_config.WindowsBackgroundHex != "")
            {
                try
                {
                    var convertFromString = ColorConverter.ConvertFromString(_config.WindowsBackgroundHex);
                    if (convertFromString != null)
                    {
                        var bgColor = (Color)convertFromString;
                        Background = new SolidColorBrush(bgColor);
                    }
                }
                catch (Exception)
                {
                    //... no valid hex
                }
            }
        }
        public void SetOpponentCardCount(int cardCount, int cardsLeftInDeck)
        {
            LblOpponentCardCount.Text = "Hand: " + cardCount;
            LblOpponentDeckCount.Text = "Deck: " + cardsLeftInDeck;
        }

        private void OpponentDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Scale();
        }

        private void Scale()
        {
            if (((ListViewOpponent.Height) - (ListViewOpponent.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
            {
                var previousScaling = Scaling;
                Scaling = (ListViewOpponent.Height) / (ListViewOpponent.Items.Count * 35);
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
    }
}
