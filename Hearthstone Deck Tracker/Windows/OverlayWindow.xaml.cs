using System;
using System.ComponentModel;
using System.Linq;
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

        public OverlayWindow(Config config, Hearthstone hearthstone)
        {
            InitializeComponent();
            _config = config;
            _hearthstone = hearthstone;

            ListViewPlayer.ItemsSource = _hearthstone.PlayerDeck;
            ListViewEnemy.ItemsSource = _hearthstone.EnemyCards;
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

        private void SortViews()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewPlayer.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            CollectionView view1 = (CollectionView)CollectionViewSource.GetDefaultView(ListViewEnemy.ItemsSource);
            view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
            view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void SetEnemyCardCount(int count)
        {
            LblEnemyCardCount.Content = "Cards in Hand: " + count;
        }

        private void SetCardCount(int p, int cardsLeftInDeck)
        {
            LblCardCount.Content = "Cards in Hand: " + p;
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
            if ((Height * 0.65 - (ListViewPlayer.Items.Count * 35 * Scaling)) < 5)
            {
                Scaling = (Height*0.65)/(ListViewPlayer.Items.Count*35 );
            }
            else if (Scaling < 1)
            {
                Scaling = 1;
            }
            ListViewPlayer.Height = 35 * ListViewPlayer.Items.Count * Scaling;


            if ((Height * 0.65 - (ListViewEnemy.Items.Count * 35 * OpponentScaling)) < 5)
            {
                OpponentScaling = (Height * 0.65) / (ListViewEnemy.Items.Count * 35);
            }
            else if (OpponentScaling < 1)
            {
                OpponentScaling = 1;
            }
            ListViewEnemy.Height = 35 * ListViewEnemy.Items.Count * OpponentScaling;
            

            Canvas.SetTop(ListViewEnemy, Height * 0.17);
            Canvas.SetTop(ListViewPlayer, Height * 0.17);
            Canvas.SetLeft(ListViewPlayer, Width - ListViewPlayer.Width - 5);

            Canvas.SetTop(LblDrawChance2, Height * 0.17 + ListViewPlayer.Height*0.95 );
            Canvas.SetLeft(LblDrawChance2, Width - (ListViewPlayer.Width / 2) - LblDrawChance1.ActualWidth/2 - 5 - LblDrawChance2.ActualWidth / 2);
            Canvas.SetTop(LblDrawChance1, Height * 0.17 + ListViewPlayer.Height*0.95 );
            Canvas.SetLeft(LblDrawChance1, Width - (ListViewPlayer.Width / 2) - 5 - LblDrawChance1.ActualWidth/2 + LblDrawChance2.ActualWidth/2);

            Canvas.SetTop(LblCardCount, Height * 0.17 + ListViewPlayer.Height*0.95 + 10);
            Canvas.SetLeft(LblCardCount, Width - ListViewPlayer.Width / 2 - 5 - LblCardCount.ActualWidth / 2);

            Canvas.SetTop(LblEnemyCardCount, Height * 0.17 + ListViewEnemy.Height*0.95);
            Canvas.SetLeft(LblEnemyCardCount, 5 + ListViewEnemy.Width / 2 - LblEnemyCardCount.ActualWidth / 2);
        
        }

        private void Window_SourceInitialized_1(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            User32.SetWindowExTransparent(hwnd);
        }

        public void Update()
        {
            if (_config.HideDrawChances)
            {
                LblDrawChance1.Visibility = Visibility.Hidden;
                LblDrawChance2.Visibility = Visibility.Hidden;
            }
            else
            {
                LblDrawChance1.Visibility = Visibility.Visible;
                LblDrawChance2.Visibility = Visibility.Visible;
            }
            LblEnemyCardCount.Visibility = _config.HideEnemyCardCount ? Visibility.Hidden : Visibility.Visible;
            ListViewEnemy.Visibility = _config.HideEnemyCards ? Visibility.Hidden : Visibility.Visible;
            LblCardCount.Visibility = _config.HidePlayerCardCount ? Visibility.Hidden : Visibility.Visible;

            SetCardCount(_hearthstone.PlayerHandCount, _hearthstone.PlayerDeck.Sum(deckcard => deckcard.Count));
            SetEnemyCardCount(_hearthstone.EnemyHandCount);
            SortViews();
            ReSizePosLists();
        }

        public void UpdatePosition()
        {
            //hide the overlay depenting on options
            EnableCanvas(!(_config.HideInBackground || _config.HideInMenu || _config.HideOverlay));

            var hsRect = new User32.Rect();
            User32.GetWindowRect(User32.FindWindow(null, "Hearthstone"), ref hsRect);
            SetRect(hsRect.top, hsRect.left, hsRect.right - hsRect.left, hsRect.bottom - hsRect.top);
            ReSizePosLists();
        }

        public static double Scaling { get; set; }
        public static double OpponentScaling { get; set; }
    }
}