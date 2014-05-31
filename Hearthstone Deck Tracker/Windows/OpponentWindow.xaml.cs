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
    public partial class OpponentWindow : Window
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
                        ListViewOpponent.Background = new SolidColorBrush(bgColor);
                    }
                }
                catch (Exception)
                {
                    //... no valid hex
                }
            }
        }

        private void OpponentDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Scale();
        }

        private void Scale()
        {
            //wtf are the correct values here
            if (((Height) - (ListViewOpponent.Items.Count * 35 * Scaling)) < 5)
            {
                Scaling = (Height) / (ListViewOpponent.Items.Count * 35);
            }
            else if (Scaling < 1)
            {
                Scaling = 1.0;
            }
            if (Scaling > 1) Scaling = 1.0;
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
        }


        internal void Shutdown()
        {
            _appIsClosing = true;
            Close();
        }
    }
}
