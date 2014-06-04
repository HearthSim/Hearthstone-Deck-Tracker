using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private readonly Config _config;
        private Hearthstone _hearthstone;

        //these are for people having issues with the overlay not displaying on the window.
        private int _offsetX;
        private int _offsetY;
        private int _customHeight;
        private int _customWidth;
        private int _cardCount;
        private int _opponentCardCount;

        public OverlayWindow(Config config, Hearthstone hearthstone)
        {
            InitializeComponent();
            _config = config;
            _hearthstone = hearthstone;

            ListViewPlayer.ItemsSource = _hearthstone.PlayerDeck;
            ListViewOpponent.ItemsSource = _hearthstone.EnemyCards;
            Scaling = 1.0;
            OpponentScaling = 1.0;
            ShowInTaskbar = _config.ShowInTaskbar;
            if (_config.VisibleOverlay)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");
            }
            _offsetX = _config.OffsetX;
            _offsetY = _config.OffsetY;
            _customWidth = _config.CustomWidth;
            _customHeight = _config.CustomHeight;
        }

        public void SortViews()
        {
            SortCardCollection(ListViewPlayer.ItemsSource);
            SortCardCollection(ListViewOpponent.ItemsSource);
        }

        private void SortCardCollection(IEnumerable collection)
        {
            var view1 = (CollectionView)CollectionViewSource.GetDefaultView(collection);
            view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
            view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
            view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void SetEnemyCardCount(int cardCount)
        {
            //previous cardcout > current -> enemy played -> resort list
            if (_opponentCardCount > cardCount)
            {
                SortCardCollection(ListViewOpponent.ItemsSource);
            }
            _opponentCardCount = cardCount;
            LblEnemyCardCount.Content = "Cards in Hand: " + cardCount;
        }

        private void SetCardCount(int cardCount, int cardsLeftInDeck)
        {
            //previous < current -> draw
            if (_cardCount < cardCount)
            {
                if(!Hearthstone.IsUsingPremade)
                    SortCardCollection(ListViewPlayer.ItemsSource);
            }
            _cardCount = cardCount;
            LblCardCount.Content = "Cards in Hand: " + cardCount;
            if (cardsLeftInDeck <= 0) return;

            if (Hearthstone.IsUsingPremade)
            {

                LblDrawChance2.Content = "[2]: " + Math.Round(200.0f/cardsLeftInDeck, 2) + "%";
                LblDrawChance1.Content = "[1]: " + Math.Round(100.0f/cardsLeftInDeck, 2) + "%";
            }
            else
            {
                LblDrawChance2.Content = "[2]: " + Math.Round(200.0f / (30 - cardsLeftInDeck), 2) + "%";
                LblDrawChance1.Content = "[1]: " + Math.Round(100.0f / (30 - cardsLeftInDeck), 2) + "%";
            }
        }
    


        public void EnableCanvas(bool enable)
        {
            CanvasInfo.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetRect(int top, int left, int width, int height)
        {
            Top = top + _offsetY;
            Left = left + _offsetX;
            Width = (_customWidth == -1)?width:_customWidth;
            Height = (_customHeight == -1)?height:_customHeight;
            CanvasInfo.Height = (_customHeight == -1) ? height : _customHeight;
            CanvasInfo.Width = (_customWidth == -1) ? width : _customWidth;
        }

        private void ReSizePosLists()
        {
            //player
            if (((Height * _config.PlayerDeckHeight / 100) - (ListViewPlayer.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
            {
                var previousScaling = Scaling;
                Scaling = (Height * _config.PlayerDeckHeight / 100) / (ListViewPlayer.Items.Count * 35);
                if (Scaling > 1)
                    Scaling = 1;

                if(previousScaling != Scaling)
                    ListViewPlayer.Items.Refresh();
            }

            ListViewPlayer.Height = 35 * ListViewPlayer.Items.Count * Scaling - LblDrawChance1.Height - LblCardCount.Height;

            Canvas.SetTop(StackPanelPlayer, Height * _config.PlayerDeckTop / 100);
            Canvas.SetLeft(StackPanelPlayer, Width * _config.PlayerDeckLeft/100 - StackPanelPlayer.ActualWidth);



            //opponent
            if (((Height * _config.OpponentDeckHeight / 100) - (ListViewOpponent.Items.Count * 35 * OpponentScaling)) < 1 || OpponentScaling < 1)
            {
                var previousScaling = OpponentScaling;
                OpponentScaling = (Height * _config.OpponentDeckHeight / 100) / (ListViewOpponent.Items.Count * 35);

                if (previousScaling != OpponentScaling)
                    ListViewOpponent.Items.Refresh();
            }
            ListViewOpponent.Height = 35 * ListViewOpponent.Items.Count * OpponentScaling - LblEnemyCardCount.Height;


            Canvas.SetTop(StackPanelOpponent, Height * _config.OpponentDeckTop / 100);
            Canvas.SetLeft(StackPanelOpponent, Width * _config.OpponentDeckLeft / 100); 



        
        }

        private void Window_SourceInitialized_1(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetWindowExTransparent(hwnd);
        }

        public void Update(bool refresh)
        {
            if (refresh)
            {
                ListViewPlayer.Items.Refresh();
                ListViewOpponent.Items.Refresh();
            }

            Opacity = _config.OverlayOpacity/100;

            LblDrawChance1.Visibility = _config.HideDrawChances ? Visibility.Hidden : Visibility.Visible;
            LblDrawChance2.Visibility = _config.HideDrawChances ? Visibility.Hidden : Visibility.Visible;
            LblEnemyCardCount.Visibility = _config.HideEnemyCardCount ? Visibility.Hidden : Visibility.Visible;
            ListViewOpponent.Visibility = _config.HideEnemyCards ? Visibility.Hidden : Visibility.Visible;
            LblCardCount.Visibility = _config.HidePlayerCardCount ? Visibility.Hidden : Visibility.Visible;

            SetCardCount(_hearthstone.PlayerHandCount, _hearthstone.PlayerDeck.Sum(deckcard => deckcard.Count));
            SetEnemyCardCount(_hearthstone.EnemyHandCount);

            ReSizePosLists();
        }

        private bool _needToRefresh;

        public void UpdatePosition()
        {
            if (!User32.IsForegroundWindow("Hearthstone") && !_needToRefresh)
            {
                _needToRefresh = true;

            } else if (_needToRefresh && User32.IsForegroundWindow("Hearthstone"))
            {
                _needToRefresh = false;
                Update(true);
            }

            //hide the overlay depenting on options
            EnableCanvas(!(
                (_config.HideInBackground && !User32.IsForegroundWindow("Hearthstone")) 
                || (_config.HideInMenu && _hearthstone.IsInMenu)
                || _config.HideOverlay));
            
            var hsRect = new User32.Rect();
            User32.GetWindowRect(User32.FindWindow(null, "Hearthstone"), ref hsRect);

            //hs window has height 0 if it just launched, screwing things up if the tracker is started before hs is. 
            //this prevents that from happening. 
            if (hsRect.bottom - hsRect.top == 0)
            {
                return;
            }

            SetRect(hsRect.top, hsRect.left, hsRect.right - hsRect.left, hsRect.bottom - hsRect.top);
            ReSizePosLists();
        }

        public static double Scaling { get; set; }
        public static double OpponentScaling { get; set; }

    }
}