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
            Topmost = _config.WindowsTopmost;
            if (_config.WindowsBackgroundHex != "")
            {
                try
                {
                    var convertFromString = ColorConverter.ConvertFromString(_config.WindowsBackgroundHex);
                    if (convertFromString != null)
                    {
                        var bgColor = (Color) convertFromString;
                        ListViewPlayer.Background = new SolidColorBrush(bgColor);
                    }
                }
                catch (Exception)
                {
                    //... no valid hex
                }
            }
        }

        private void PlayerDeckOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Scale();
        }

        private void Scale()
        {
            //wtf are the correct values here
            if (((Height) - (ListViewPlayer.Items.Count * 35 * Scaling)) < 5)
            {
                Scaling = (Height) / (ListViewPlayer.Items.Count * 35);
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
        }


        internal void Shutdown()
        {
            _appIsClosing = true;
            Close();
        }
    }
}
