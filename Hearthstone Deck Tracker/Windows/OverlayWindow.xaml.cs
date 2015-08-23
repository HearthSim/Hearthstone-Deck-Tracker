#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

#endregion

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow
    {
        private readonly GameV2 _game;
        private readonly int _customHeight;
        private readonly int _customWidth;
        private readonly Dictionary<UIElement, ResizeGrip> _movableElements;
        private readonly int _offsetX;
        private readonly int _offsetY;
        private readonly List<CardMarker> _cardMarks;
        private int _cardCount;
        private bool? _isFriendsListOpen;
        private string _lastToolTipCardId;
        private bool _lmbDown;
        private User32.MouseInput _mouseInput;
        private Point _mousePos;
        private int _opponentCardCount;
        private bool _opponentCardsHidden;
        private bool _playerCardsHidden;
        private bool _resizeElement;
        private bool _secretsTempVisible;
        private UIElement _selectedUIElement;
        private bool _uiMovable;
        private readonly Point[][] _cardMarkPos;

        public OverlayWindow(GameV2 game)
        {
            _game = game;
            InitializeComponent();

            if (Config.Instance.ExtraFeatures && Config.Instance.ForceMouseHook)
                HookMouse();

            ListViewPlayer.ItemsSource = _game.IsUsingPremade ? _game.PlayerDeck : _game.PlayerDrawn;
            ListViewOpponent.ItemsSource = _game.OpponentCards;
            Scaling = 1.0;
            OpponentScaling = 1.0;
            ShowInTaskbar = Config.Instance.ShowInTaskbar;
            if (Config.Instance.VisibleOverlay)
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");
            _offsetX = Config.Instance.OffsetX;
            _offsetY = Config.Instance.OffsetY;
            _customWidth = Config.Instance.CustomWidth;
            _customHeight = Config.Instance.CustomHeight;

            // Add CardMarkers to the list.  
            _cardMarks = new List<CardMarker> { Marks0, Marks1, Marks2, Marks3, Marks4, Marks5, Marks6, Marks7, Marks8, Marks9 };

            const double tWidth = 1024.0;
            const double tHeight = 768.0;
            _cardMarkPos = new Point[10][];
            _cardMarkPos[0] = new[] { new Point(480 / tWidth, 48 / tHeight) };
            _cardMarkPos[1] = new[] { new Point(439 / tWidth, 47 / tHeight), new Point(520 / tWidth, 48 / tHeight) };
            _cardMarkPos[2] = new[]
                       {
               new Point(392 / tWidth, 33 / tHeight),
               new Point(479 / tWidth, 47 / tHeight),
               new Point(569 / tWidth, 40 / tHeight)
           };
            _cardMarkPos[3] = new[]
                       {
               new Point(382 / tWidth, 21 / tHeight),
               new Point(446 / tWidth, 41 / tHeight),
               new Point(512 / tWidth, 47 / tHeight),
               new Point(580 / tWidth, 43 / tHeight)
           };
            _cardMarkPos[4] = new[]
                       {
               new Point(375 / tWidth, 23 / tHeight),
               new Point(427 / tWidth, 39 / tHeight),
               new Point(479 / tWidth, 47 / tHeight),
               new Point(533 / tWidth, 46 / tHeight),
               new Point(586 / tWidth, 36 / tHeight)
           };
            _cardMarkPos[5] = new[]
                       {
               new Point(371 / tWidth, 12 / tHeight),
               new Point(414 / tWidth, 30 / tHeight),
               new Point(458 / tWidth, 43 / tHeight),
               new Point(502 / tWidth, 48 / tHeight),
               new Point(546 / tWidth, 47 / tHeight),
               new Point(591 / tWidth, 39 / tHeight)
           };
            _cardMarkPos[6] = new[]
                       {
               new Point(368 / tWidth, 15 / tHeight),
               new Point(405 / tWidth, 31 / tHeight),
               new Point(442 / tWidth, 41 / tHeight),
               new Point(479 / tWidth, 48 / tHeight),
               new Point(517 / tWidth, 47 / tHeight),
               new Point(555 / tWidth, 41 / tHeight),
               new Point(594 / tWidth, 31 / tHeight)
           };
            _cardMarkPos[7] = new[]
                       {
               new Point(365 / tWidth, 04 / tHeight),
               new Point(397 / tWidth, 22 / tHeight),
               new Point(430 / tWidth, 35 / tHeight),
               new Point(462 / tWidth, 45 / tHeight),
               new Point(496 / tWidth, 48 / tHeight),
               new Point(530 / tWidth, 48 / tHeight),
               new Point(563 / tWidth, 43 / tHeight),
               new Point(597 / tWidth, 33 / tHeight)
           };
            _cardMarkPos[8] = new[]
                       {
               new Point(363 / tWidth, 07 / tHeight),
               new Point(392 / tWidth, 23 / tHeight),
               new Point(421 / tWidth, 35 / tHeight),
               new Point(450 / tWidth, 43 / tHeight),
               new Point(479 / tWidth, 48 / tHeight),
               new Point(508 / tWidth, 47 / tHeight),
               new Point(539 / tWidth, 43 / tHeight),
               new Point(569 / tWidth, 35 / tHeight),
               new Point(599 / tWidth, 23 / tHeight)
           };
            _cardMarkPos[9] = new[]
                       {
               new Point(364 / tWidth, 04 / tHeight),
               new Point(388 / tWidth, 13 / tHeight),
               new Point(414 / tWidth, 28 / tHeight),
               new Point(440 / tWidth, 38 / tHeight),
               new Point(467 / tWidth, 45 / tHeight),
               new Point(492 / tWidth, 48 / tHeight),
               new Point(420 / tWidth, 48 / tHeight),
               new Point(546 / tWidth, 44 / tHeight),
               new Point(573 / tWidth, 37 / tHeight),
               new Point(600 / tWidth, 27 / tHeight)
           };
            _movableElements = new Dictionary<UIElement, ResizeGrip>
            {
                {StackPanelPlayer, new ResizeGrip()},
                {StackPanelOpponent, new ResizeGrip()},
                {StackPanelSecrets, new ResizeGrip()},
                {LblTurnTime, new ResizeGrip()},
                {LblPlayerTurnTime, new ResizeGrip()}
            };

            UpdateScaling();
        }

        public bool ForceHidden { get; set; }
        public static double Scaling { get; set; }
        public static double OpponentScaling { get; set; }
        public Visibility WarningVisibility { get; set; }

        private void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
        {
            if (Visibility != Visibility.Visible)
                return;
            if (_selectedUIElement != null)
                Config.Save();
            _selectedUIElement = null;
            _lmbDown = false;
            _resizeElement = false;
        }

        private void MouseInputOnMouseMoved(object sender, EventArgs eventArgs)
        {
            if (!_lmbDown || Visibility != Visibility.Visible)
                return;

            var pos = User32.GetMousePos();
            var newPos = new Point(pos.X, pos.Y);
            var delta = new Point((newPos.X - _mousePos.X) * 100, (newPos.Y - _mousePos.Y) * 100);

            var panel = _selectedUIElement as StackPanel;
            if (panel != null)
            {
                if (panel.Name.Contains("Player"))
                {
                    if (_resizeElement)
                    {
                        Config.Instance.PlayerDeckHeight += delta.Y / Height;
                        _movableElements[panel].Height = Height * Config.Instance.PlayerDeckHeight / 100;
                    }
                    else
                    {
                        Config.Instance.PlayerDeckTop += delta.Y / Height;
                        Config.Instance.PlayerDeckLeft += delta.X / Width;
                        Canvas.SetTop(_movableElements[panel], Height * Config.Instance.PlayerDeckTop / 100);
                        Canvas.SetLeft(_movableElements[panel],
                                       Width * Config.Instance.PlayerDeckLeft / 100
                                       - StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);
                    }
                }
                else if (panel.Name.Contains("Opponent"))
                {
                    if (_resizeElement)
                    {
                        Config.Instance.OpponentDeckHeight += delta.Y / Height;
                        _movableElements[panel].Height = Height * Config.Instance.OpponentDeckHeight / 100;
                    }
                    else
                    {
                        Config.Instance.OpponentDeckTop += delta.Y / Height;
                        Config.Instance.OpponentDeckLeft += delta.X / Width;
                        Canvas.SetTop(_movableElements[panel], Height * Config.Instance.OpponentDeckTop / 100);
                        Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.OpponentDeckLeft / 100);
                    }
                }
                else if (panel.Name.Contains("Secret"))
                {
                    if (_resizeElement)
                    {
                        const int width = 215;
                        var height = 35 * StackPanelSecrets.Children.Count;
                        Config.Instance.SecretsPanelScaling += Math.Abs(delta.Y) > Math.Abs(delta.X) ? delta.Y / (100 * height) : delta.X / (100 * width);
                        if (Config.Instance.SecretsPanelScaling > 1)
                            Config.Instance.SecretsPanelScaling = 1;
                        else if (Config.Instance.SecretsPanelScaling < 0.4)
                            Config.Instance.SecretsPanelScaling = 0.4;
                        _movableElements[panel].RenderTransform = new ScaleTransform(Config.Instance.SecretsPanelScaling,
                                                                                     Config.Instance.SecretsPanelScaling);
                    }
                    else
                    {
                        Config.Instance.SecretsTop += delta.Y / Height;
                        Config.Instance.SecretsLeft += delta.X / Width;
                        Canvas.SetTop(_movableElements[panel], Height * Config.Instance.SecretsTop / 100);
                        Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.SecretsLeft / 100);
                    }
                }
            }

            var timer = _selectedUIElement as HearthstoneTextBlock;
            if (timer != null)
            {
                if (timer.Name.Contains("Player"))
                {
                    Config.Instance.TimersVerticalSpacing += delta.Y / 100;
                    Config.Instance.TimersHorizontalSpacing += delta.X / 100;
                    Canvas.SetTop(_movableElements[timer],
                                  Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
                    Canvas.SetLeft(_movableElements[timer],
                                   Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
                }
                else if (timer.Name.Contains("Turn"))
                {
                    Config.Instance.TimersVerticalPosition += delta.Y / Height;
                    Config.Instance.TimersHorizontalPosition += delta.X / Width;
                    Canvas.SetTop(_movableElements[timer], Height * Config.Instance.TimersVerticalPosition / 100);
                    Canvas.SetLeft(_movableElements[timer], Width * Config.Instance.TimersHorizontalPosition / 100);

                    var playerTimer =
                        _movableElements.First(e => e.Key is HearthstoneTextBlock && ((HearthstoneTextBlock)e.Key).Name.Contains("Player")).Value;
                    Canvas.SetTop(playerTimer, Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
                    Canvas.SetLeft(playerTimer, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
                }
            }

            _mousePos = newPos;
        }

        private void MouseInputOnLmbDown(object sender, EventArgs eventArgs)
        {
            if (!User32.IsHearthstoneInForeground() || Visibility != Visibility.Visible)
                return;

            var pos = User32.GetMousePos();
            _mousePos = new Point(pos.X, pos.Y);

            if (_uiMovable)
            {
                _lmbDown = true;
                foreach (var movableElement in _movableElements)
                {
                    var relativePos = movableElement.Value.PointFromScreen(_mousePos);

                    var panel = movableElement.Key as StackPanel;
                    if (panel != null)
                    {
                        if (PointInsideControl(relativePos, movableElement.Value.ActualWidth, movableElement.Value.ActualHeight))
                        {
                            if (Math.Abs(relativePos.X - movableElement.Value.ActualWidth) < 30
                               && Math.Abs(relativePos.Y - movableElement.Value.ActualHeight) < 30)
                                _resizeElement = true;

                            _selectedUIElement = movableElement.Key;
                            return;
                        }
                    }

                    var timer = movableElement.Key as HearthstoneTextBlock;
                    if (timer != null)
                    {
                        if (PointInsideControl(relativePos, movableElement.Value.ActualWidth, movableElement.Value.ActualHeight))
                        {
                            //if(Math.Abs(relativePos.X - timer.ActualWidth) < 30 && Math.Abs(relativePos.Y - timer.ActualHeight) < 30)
                            //_resizeElement = true;

                            _selectedUIElement = movableElement.Key;
                            return;
                        }
                    }
                }
            }

            HideCardsWhenFriendsListOpen(PointFromScreen(_mousePos));

            GrayOutSecrets(_mousePos);
        }

        private async void HideCardsWhenFriendsListOpen(Point clickPos)
        {
            var panels = new List<StackPanel>();
            if (Canvas.GetLeft(StackPanelPlayer) - 200 < 500)
                panels.Add(StackPanelPlayer);
            if (Canvas.GetLeft(StackPanelOpponent) < 500)
                panels.Add(StackPanelOpponent);

            _isFriendsListOpen = null;
            if (panels.Count > 0 && !Config.Instance.HideDecksInOverlay)
            {
                foreach (var panel in panels)
                {
                    //if panel visible, only continue of click was in the button left corner
                    if (!(clickPos.X < 150 && clickPos.Y > Height - 100) && panel.Visibility == Visibility.Visible)
                        continue;

                    var checkForFriendsList = true;
                    if (panel.Equals(StackPanelPlayer) && Config.Instance.HidePlayerCards)
                        checkForFriendsList = false;
                    else if (panel.Equals(StackPanelOpponent) && Config.Instance.HideOpponentCards)
                        checkForFriendsList = false;

                    if (checkForFriendsList)
                    {
                        if (_isFriendsListOpen == null)
                            _isFriendsListOpen = await Helper.FriendsListOpen();
                        if (_isFriendsListOpen.Value)
                        {
                            var needToHide = Canvas.GetTop(panel) + panel.ActualHeight > Height * 0.3;
                            if (needToHide)
                            {
                                panel.Visibility = Visibility.Collapsed;
                                if (panel.Equals(StackPanelPlayer))
                                    _playerCardsHidden = true;
                                else
                                    _opponentCardsHidden = true;
                            }
                        }
                        else if (panel.Visibility == Visibility.Collapsed)
                        {
                            if (!(_game.IsInMenu && Config.Instance.HideInMenu))
                            {
                                panel.Visibility = Visibility.Visible;
                                if (panel.Equals(StackPanelPlayer))
                                    _playerCardsHidden = false;
                                else
                                    _opponentCardsHidden = false;
                            }
                        }
                    }
                }
            }
        }

        private void GrayOutSecrets(Point mousePos)
        {
            if (!PointInsideControl(StackPanelSecrets.PointFromScreen(mousePos), StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
                return;

            var card = ToolTipCard.DataContext as Card;
            if (card == null)
                return;

            _game.OpponentSecrets.Trigger(card.Id);
            ShowSecrets();
        }

        public void SortViews()
        {
            Helper.SortCardCollection(ListViewPlayer.ItemsSource, Config.Instance.CardSortingClassFirst);
            Helper.SortCardCollection(ListViewOpponent.ItemsSource, Config.Instance.CardSortingClassFirst);
        }

        private void SetOpponentCardCount(int cardCount, int cardsLeftInDeck)
        {
            //previous cardcout > current -> opponent played -> resort list
            if (_opponentCardCount > cardCount)
                Helper.SortCardCollection(ListViewOpponent.ItemsSource, Config.Instance.CardSortingClassFirst);
            _opponentCardCount = cardCount;

            LblOpponentCardCount.Text = cardCount.ToString();
            LblOpponentDeckCount.Text = cardsLeftInDeck.ToString();

            if (cardsLeftInDeck <= 0)
            {
                LblOpponentFatigue.Text = "Next draw fatigues for: " + (_game.OpponentFatigueCount + 1);

                LblOpponentDrawChance2.Text = "0%";
                LblOpponentDrawChance1.Text = "0%";
                LblOpponentHandChance2.Text = cardCount <= 0 ? "0%" : "100%"; ;
                LblOpponentHandChance1.Text = cardCount <= 0 ? "0%" : "100%";
                return;
            }
            LblOpponentFatigue.Text = "";

            var handWithoutCoin = cardCount - (_game.OpponentHasCoin ? 1 : 0);

            var holdingNextTurn2 = Math.Round(100.0f * Helper.DrawProbability(2, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
            var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 1);
            LblOpponentDrawChance2.Text = drawNextTurn2 + "%";
            LblOpponentHandChance2.Text = holdingNextTurn2 + "%";

            var holdingNextTurn = Math.Round(100.0f * Helper.DrawProbability(1, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 1);
            var drawNextTurn = Math.Round(100.0f / cardsLeftInDeck, 1);
            LblOpponentDrawChance1.Text = drawNextTurn + "%";
            LblOpponentHandChance1.Text = holdingNextTurn + "%";
        }

        private void SetCardCount(int cardCount, int cardsLeftInDeck)
        {
            //previous < current -> draw
            if (_cardCount < cardCount)
                Helper.SortCardCollection(ListViewPlayer.ItemsSource, Config.Instance.CardSortingClassFirst);
            _cardCount = cardCount;

            LblCardCount.Text = cardCount.ToString();
            LblDeckCount.Text = cardsLeftInDeck.ToString();

            if (cardsLeftInDeck <= 0)
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

        public void ShowOverlay(bool enable)
        {
            if (enable)
            {
                Show();
                if (User32.GetForegroundWindow() == new WindowInteropHelper(this).Handle)
                    User32.BringHsToForeground();
            }
            else
                Hide();
        }

        private void SetRect(int top, int left, int width, int height)
        {
            Top = top + _offsetY;
            Left = left + _offsetX;
            Width = (_customWidth == -1) ? width : _customWidth;
            Height = (_customHeight == -1) ? height : _customHeight;
            CanvasInfo.Width = (_customWidth == -1) ? width : _customWidth;
            CanvasInfo.Height = (_customHeight == -1) ? height : _customHeight;
            StackPanelAdditionalTooltips.MaxHeight = Height;
        }

        private void ReSizePosLists()
        {
            //player TODO: take labels into account
            var totalPlayerLabelsHeight = CanvasPlayerChance.ActualHeight + CanvasPlayerCount.ActualHeight + LblPlayerFatigue.ActualHeight
                                          + LblDeckTitle.ActualHeight + LblWins.ActualHeight;
            if (((Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100)
                - (ListViewPlayer.Items.Count * 35 * Scaling + totalPlayerLabelsHeight)) < 1 || Scaling < 1)
            {
                var previousScaling = Scaling;
                Scaling = (Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100)
                          / (ListViewPlayer.Items.Count * 35 + totalPlayerLabelsHeight);
                if (Scaling > 1)
                    Scaling = 1;

                if (previousScaling != Scaling)
                    ListViewPlayer.Items.Refresh();
            }

            Canvas.SetTop(StackPanelPlayer, Height * Config.Instance.PlayerDeckTop / 100);
            Canvas.SetLeft(StackPanelPlayer,
                           Width * Config.Instance.PlayerDeckLeft / 100
                           - StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);

            //opponent
            var totalOpponentLabelsHeight = CanvasOpponentChance.ActualHeight + CanvasOpponentCount.ActualHeight + LblOpponentFatigue.ActualHeight
                                          + LblWinRateAgainst.ActualHeight;
            if (((Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100)
                - (ListViewOpponent.Items.Count * 35 * OpponentScaling + totalOpponentLabelsHeight)) < 1 || OpponentScaling < 1)
            {
                var previousScaling = OpponentScaling;
                OpponentScaling = (Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100)
                                  / (ListViewOpponent.Items.Count * 35 + totalOpponentLabelsHeight);
                if (OpponentScaling > 1)
                    OpponentScaling = 1;

                if (previousScaling != OpponentScaling)
                    ListViewOpponent.Items.Refresh();
            }

            //secrets
            StackPanelSecrets.RenderTransform = new ScaleTransform(Config.Instance.SecretsPanelScaling, Config.Instance.SecretsPanelScaling);

            Canvas.SetTop(StackPanelOpponent, Height * Config.Instance.OpponentDeckTop / 100);
            Canvas.SetLeft(StackPanelOpponent, Width * Config.Instance.OpponentDeckLeft / 100);

            //Secrets
            Canvas.SetTop(StackPanelSecrets, Height * Config.Instance.SecretsTop / 100);
            Canvas.SetLeft(StackPanelSecrets, Width * Config.Instance.SecretsLeft / 100);

            // Timers
            Canvas.SetTop(LblTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - 5);
            Canvas.SetLeft(LblTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100);

            Canvas.SetTop(LblOpponentTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 - Config.Instance.TimersVerticalSpacing);
            Canvas.SetLeft(LblOpponentTurnTime,
                           (Width * Config.Instance.TimersHorizontalPosition / 100) + Config.Instance.TimersHorizontalSpacing);

            Canvas.SetTop(LblPlayerTurnTime, Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
            Canvas.SetLeft(LblPlayerTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);

            //Canvas.SetTop(LblGrid, Height * 0.03);
            var handCount = _game.OpponentHandCount;
            if (handCount < 0)
                handCount = 0;
            if (handCount > 10)
                handCount = 10;

            var ratio = (4.0 / 3.0) / (Width / Height);
            for (int i = 0; i < handCount; i++)
            {
                Canvas.SetLeft(_cardMarks[i],
                DeckExporter.GetXPos(_cardMarkPos[handCount - 1][i].X, (int)Width, ratio) - _cardMarks[i].ActualWidth / 2);
                Canvas.SetTop(_cardMarks[i], Math.Max(_cardMarkPos[handCount - 1][i].Y * Height - _cardMarks[i].ActualHeight / 3, 5));
            }

            //Gold progress
            var goldFrameHeight = Height * 25 / 768;
            var goldFrameWidth = 6 * goldFrameHeight;
            var goldFrameOffset = 85 / 25 * goldFrameHeight;
            RectGoldDisplay.Height = goldFrameHeight;
            RectGoldDisplay.Width = goldFrameWidth;
            LblGoldProgress.Height = goldFrameHeight;
            var left = Width - RectGoldDisplay.ActualWidth - goldFrameOffset;
            var top = Height - RectGoldDisplay.ActualHeight - 2;
            Canvas.SetTop(RectGoldDisplay, top);
            Canvas.SetLeft(RectGoldDisplay, left);
            Canvas.SetTop(LblGoldProgress, top + (goldFrameHeight - LblGoldProgress.ActualHeight) / 2 - 2);
            Canvas.SetLeft(LblGoldProgress, left - LblGoldProgress.ActualWidth - 10);


        }

        private void Window_SourceInitialized_1(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            User32.SetWindowExStyle(hwnd, User32.WsExTransparent | User32.WsExToolWindow);
        }

        public void Update(bool refresh)
        {
            if (refresh)
            {
                ListViewPlayer.Items.Refresh();
                ListViewOpponent.Items.Refresh();
                Topmost = false;
                Topmost = true;
                Logger.WriteLine("Refreshed overlay topmost status", "UpdateOverlay");
            }


            var handCount = _game.OpponentHandCount;
            if (handCount < 0)
                handCount = 0;
            if (handCount > 10)
                handCount = 10;


            for (var i = 0; i < handCount; i++)
            {
                if (!Config.Instance.HideOpponentCardAge)
                {
                    _cardMarks[i].Text = _game.OpponentHandAge[i].ToString();
                }
                else
                {
                    _cardMarks[i].Text = "";
                }

                if (!Config.Instance.HideOpponentCardMarks)
                {
                    _cardMarks[i].Mark = _game.OpponentHandMarks[i];
                }
                else
                {
                    _cardMarks[i].Mark = CardMark.None;
                }

                _cardMarks[i].Visibility = _game.IsInMenu ? Visibility.Hidden : Visibility.Visible;
            }
            //Hide unneeded card marks.
            for (var i = handCount; i < 10; i++)
            {
                _cardMarks[i].Visibility = Visibility.Collapsed;
            }

            StackPanelPlayer.Opacity = Config.Instance.PlayerOpacity / 100;
            StackPanelOpponent.Opacity = Config.Instance.OpponentOpacity / 100;
            Opacity = Config.Instance.OverlayOpacity / 100;

            if (!_playerCardsHidden)
            {
                StackPanelPlayer.Visibility = (Config.Instance.HideDecksInOverlay || (Config.Instance.HideInMenu && _game.IsInMenu)) && !_uiMovable
                                                  ? Visibility.Collapsed : Visibility.Visible;
            }

            if (!_opponentCardsHidden)
            {
                StackPanelOpponent.Visibility = (Config.Instance.HideDecksInOverlay || (Config.Instance.HideInMenu && _game.IsInMenu))
                                                && !_uiMovable ? Visibility.Collapsed : Visibility.Visible;
            }

            CanvasPlayerChance.Visibility = Config.Instance.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
            LblPlayerFatigue.Visibility = Config.Instance.HidePlayerFatigueCount ? Visibility.Collapsed : Visibility.Visible;
            CanvasPlayerCount.Visibility = Config.Instance.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;

            CanvasOpponentChance.Visibility = Config.Instance.HideOpponentDrawChances ? Visibility.Collapsed : Visibility.Visible;
            LblOpponentFatigue.Visibility = Config.Instance.HideOpponentFatigueCount ? Visibility.Collapsed : Visibility.Visible;
            CanvasOpponentCount.Visibility = Config.Instance.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;

            if (_game.IsInMenu && !_uiMovable)
                HideTimers();

            ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Visibility.Collapsed : Visibility.Visible;
            ListViewPlayer.Visibility = Config.Instance.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;

            DebugViewer.Visibility = Config.Instance.Debug ? Visibility.Visible : Visibility.Hidden;
            DebugViewer.Width = (Width * Config.Instance.TimerLeft / 100);

            SetCardCount(_game.PlayerHandCount, _game.PlayerDeckSize - _game.PlayerDrawn.Where(c => !c.IsStolen).Sum(c => c.Count));

            SetOpponentCardCount(_game.OpponentHandCount, _game.OpponentDeckCount);


            LblWins.Visibility = Config.Instance.ShowDeckWins && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
            LblDeckTitle.Visibility = Config.Instance.ShowDeckTitle && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;
            LblWinRateAgainst.Visibility = Config.Instance.ShowWinRateAgainst && _game.IsUsingPremade ? Visibility.Visible : Visibility.Collapsed;

            var showWarning = !_game.IsInMenu && _game.NoMatchingDeck;
            StackPanelWarning.Visibility = showWarning ? Visibility.Visible : Visibility.Collapsed;
            if (showWarning)
            {
                var drawn = new Deck { Cards = new ObservableCollection<Card>(_game.PlayerDrawn.Where(c => !c.IsStolen)) };
                var diff = (drawn - DeckList.Instance.ActiveDeckVersion).Where(c => c.Count > 0).ToList();
                if (diff.Count > 0)
                {
                    var count = diff.Count > 3 ? 3 : diff.Count;
                    LblWarningCards.Text = diff.Take(count).Select(c => c.LocalizedName).Aggregate((c, n) => c + ", " + n);
                    if (diff.Count > 3)
                        LblWarningCards.Text += ", ...";
                }
            }

            if (_game.IsInMenu)
            {
                if (Config.Instance.AlwaysShowGoldProgress)
                {
                    UpdateGoldProgress();
                    LblGoldProgress.Visibility = Visibility.Visible;
                }
            }
            else
                LblGoldProgress.Visibility = Visibility.Collapsed;

            SetDeckTitle();
            SetWinRates();

            ReSizePosLists();


            if (Helper.MainWindow.PlayerWindow.Visibility == Visibility.Visible)
                Helper.MainWindow.PlayerWindow.Update();
            if (Helper.MainWindow.OpponentWindow.Visibility == Visibility.Visible)
                Helper.MainWindow.OpponentWindow.Update();
        }

        private void UpdateGoldProgress()
        {
            var region = (int)_game.CurrentRegion - 1;
            if (region >= 0)
            {
                LblGoldProgress.Text = string.Format("Wins: {0}/3 ({1}/100G)", Config.Instance.GoldProgress[region],
                                                     Config.Instance.GoldProgressTotal[region]);
            }
        }

        private void SetWinRates()
        {
            var selectedDeck = DeckList.Instance.ActiveDeck;
            if (selectedDeck == null)
                return;

            LblWins.Text = string.Format("{0} ({1})", selectedDeck.WinLossString, selectedDeck.WinPercentString);

            if (_game.PlayingAgainst != string.Empty)
            {
                var winsVS = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Win && g.OpponentHero == _game.PlayingAgainst);
                var lossesVS = selectedDeck.GetRelevantGames().Count(g => g.Result == GameResult.Loss && g.OpponentHero == _game.PlayingAgainst);
                var percent = (winsVS + lossesVS) > 0 ? Math.Round(winsVS * 100.0 / (winsVS + lossesVS), 0).ToString() : "-";
                LblWinRateAgainst.Text = string.Format("VS {0}: {1} - {2} ({3}%)", _game.PlayingAgainst, winsVS, lossesVS, percent);
            }
        }

        private void SetDeckTitle()
        {
            var selectedDeck = DeckList.Instance.ActiveDeckVersion;
            LblDeckTitle.Text = selectedDeck != null ? selectedDeck.Name : string.Empty;
        }

        public bool PointInsideControl(Point pos, double actualWidth, double actualHeight)
        {
            return PointInsideControl(pos, actualWidth, actualHeight, new Thickness(0));
        }

        public bool PointInsideControl(Point pos, double actualWidth, double actualHeight, Thickness margin)
        {
            if (pos.X > 0 - margin.Left && pos.X < actualWidth + margin.Right)
            {
                if (pos.Y > 0 - margin.Top && pos.Y < actualHeight + margin.Bottom)
                    return true;
            }
            return false;
        }

        private async Task UpdateCardTooltip()
        {
            var pos = User32.GetMousePos();
            var relativePlayerDeckPos = ListViewPlayer.PointFromScreen(new Point(pos.X, pos.Y));
            var relativeOpponentDeckPos = ListViewOpponent.PointFromScreen(new Point(pos.X, pos.Y));
            var relativeSecretsPos = StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y));
            var relativeCardMark = _cardMarks.Select(x => new { Label = x, Pos = x.PointFromScreen(new Point(pos.X, pos.Y)) });
            var visibility = (Config.Instance.OverlayCardToolTips && !Config.Instance.OverlaySecretToolTipsOnly)
                                 ? Visibility.Visible : Visibility.Hidden;

            var cardMark =
                relativeCardMark.FirstOrDefault(
                                                x =>
                                                x.Label.IsVisible
                                                && PointInsideControl(x.Pos, x.Label.ActualWidth, x.Label.ActualHeight, new Thickness(3, 1, 7, 1)));
            if (!Config.Instance.HideOpponentCardMarks && cardMark != null)
            {
                var index = _cardMarks.IndexOf(cardMark.Label);
                var card = _game.OpponentStolenCardsInformation[index];
                if (card != null)
                {
                    ToolTipCard.SetValue(DataContextProperty, card);
                    var topOffset = Canvas.GetTop(_cardMarks[index]) + _cardMarks[index].ActualHeight;
                    var leftOffset = Canvas.GetLeft(_cardMarks[index]) + _cardMarks[index].ActualWidth * index;
                    Canvas.SetTop(ToolTipCard, topOffset);
                    Canvas.SetLeft(ToolTipCard, leftOffset);
                    ToolTipCard.Visibility = Config.Instance.OverlayCardMarkToolTips ? Visibility.Visible : Visibility.Hidden;
                }
            }
            //player card tooltips
            else if (ListViewPlayer.Visibility == Visibility.Visible && StackPanelPlayer.Visibility == Visibility.Visible
                    && PointInsideControl(relativePlayerDeckPos, ListViewPlayer.ActualWidth, ListViewPlayer.ActualHeight))
            {
                //card size = card list height / ammount of cards
                var cardSize = ListViewPlayer.ActualHeight / ListViewPlayer.Items.Count;
                var cardIndex = (int)(relativePlayerDeckPos.Y / cardSize);
                if (cardIndex < 0 || cardIndex >= ListViewPlayer.Items.Count)
                    return;

                ToolTipCard.SetValue(DataContextProperty, ListViewPlayer.Items[cardIndex]);

                //offset is affected by scaling
                var topOffset = Canvas.GetTop(StackPanelPlayer) + GetListViewOffset(StackPanelPlayer)
                                + cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100;

                //prevent tooltip from going outside of the overlay
                if (topOffset + ToolTipCard.ActualHeight > Height)
                    topOffset = Height - ToolTipCard.ActualHeight;

                SetTooltipPosition(topOffset, StackPanelPlayer);

                ToolTipCard.Visibility = visibility;
            }
            //opponent card tooltips
            else if (ListViewOpponent.Visibility == Visibility.Visible && StackPanelOpponent.Visibility == Visibility.Visible
                    && PointInsideControl(relativeOpponentDeckPos, ListViewOpponent.ActualWidth, ListViewOpponent.ActualHeight))
            {
                //card size = card list height / ammount of cards
                var cardSize = ListViewOpponent.ActualHeight / ListViewOpponent.Items.Count;
                var cardIndex = (int)(relativeOpponentDeckPos.Y / cardSize);
                if (cardIndex < 0 || cardIndex >= ListViewOpponent.Items.Count)
                    return;

                ToolTipCard.SetValue(DataContextProperty, ListViewOpponent.Items[cardIndex]);

                //offset is affected by scaling
                var topOffset = Canvas.GetTop(StackPanelOpponent) + GetListViewOffset(StackPanelOpponent)
                                + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100;

                //prevent tooltip from going outside of the overlay
                if (topOffset + ToolTipCard.ActualHeight > Height)
                    topOffset = Height - ToolTipCard.ActualHeight;

                SetTooltipPosition(topOffset, StackPanelOpponent);

                ToolTipCard.Visibility = visibility;
            }
            else if (StackPanelSecrets.Visibility == Visibility.Visible
                    && PointInsideControl(relativeSecretsPos, StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
            {
                //card size = card list height / ammount of cards
                var cardSize = StackPanelSecrets.ActualHeight / StackPanelSecrets.Children.Count;
                var cardIndex = (int)(relativeSecretsPos.Y / cardSize);
                if (cardIndex < 0 || cardIndex >= StackPanelSecrets.Children.Count)
                    return;

                ToolTipCard.SetValue(DataContextProperty, StackPanelSecrets.Children[cardIndex].GetValue(DataContextProperty));

                //offset is affected by scaling
                var topOffset = Canvas.GetTop(StackPanelSecrets) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100;

                //prevent tooltip from going outside of the overlay
                if (topOffset + ToolTipCard.ActualHeight > Height)
                    topOffset = Height - ToolTipCard.ActualHeight;

                SetTooltipPosition(topOffset, StackPanelSecrets);

                ToolTipCard.Visibility = Config.Instance.OverlaySecretToolTipsOnly ? Visibility.Visible : visibility;
            }
            else
            {
                ToolTipCard.Visibility = Visibility.Hidden;
                HideAdditionalToolTips();
            }

            if (ToolTipCard.Visibility == Visibility.Visible)
            {
                var card = ToolTipCard.GetValue(DataContextProperty) as Card;
                if (card != null)
                {
                    if (_lastToolTipCardId != card.Id)
                    {
                        _lastToolTipCardId = card.Id;
                        ShowAdditionalToolTips();
                    }
                }
                else
                    HideAdditionalToolTips();
            }
            else
            {
                HideAdditionalToolTips();
                _lastToolTipCardId = string.Empty;
            }


            if (!Config.Instance.ForceMouseHook)
            {
                if (Config.Instance.ExtraFeatures)
                {
                    var relativePos = PointFromScreen(new Point(pos.X, pos.Y));
                    if ((StackPanelSecrets.IsVisible
                        && (PointInsideControl(StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y)), StackPanelSecrets.ActualWidth,
                                               StackPanelSecrets.ActualHeight, new Thickness(20)))
                        || relativePos.X < 170 && relativePos.Y > Height - 120))
                    {
                        if (_mouseInput == null)
                            HookMouse();
                    }
                    else if (_mouseInput != null && !((_isFriendsListOpen.HasValue && _isFriendsListOpen.Value) || await Helper.FriendsListOpen()))
                        UnHookMouse();
                }
                else if (_mouseInput != null)
                    UnHookMouse();
            }

            if (!Config.Instance.AlwaysShowGoldProgress)
            {
                if (_game.IsInMenu
                   && PointInsideControl(RectGoldDisplay.PointFromScreen(new Point(pos.X, pos.Y)), RectGoldDisplay.ActualWidth,
                                         RectGoldDisplay.ActualHeight))
                {
                    UpdateGoldProgress();
                    LblGoldProgress.Visibility = Visibility.Visible;
                }
                else
                    LblGoldProgress.Visibility = Visibility.Hidden;
            }
        }

        private double GetListViewOffset(StackPanel stackPanel)
        {
            var offset = 0.0;
            foreach (var child in stackPanel.Children)
            {
                var text = child as HearthstoneTextBlock;
                if (text != null)
                    offset += text.ActualHeight;
                else
                {
                    if (child is ListView)
                        break;
                    var sp = child as StackPanel;
                    if (sp != null)
                        offset += sp.ActualHeight;
                }
            }
            return offset;
        }

        private void HideAdditionalToolTips()
        {
            StackPanelAdditionalTooltips.Visibility = Visibility.Hidden;
        }

        private void SetTooltipPosition(double yOffset, StackPanel stackpanel)
        {
            Canvas.SetTop(ToolTipCard, yOffset);

            if (Canvas.GetLeft(stackpanel) < Width / 2)
                Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100);
            else
                Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) - ToolTipCard.ActualWidth);
        }

        public async void UpdatePosition()
        {
            //hide the overlay depenting on options
            ShowOverlay(
                        !((Config.Instance.HideInBackground && !User32.IsHearthstoneInForeground())
                          || (Config.Instance.HideOverlayInSpectator && _game.CurrentGameMode == GameMode.Spectator)
                          || Config.Instance.HideOverlay || ForceHidden));


            var hsRect = User32.GetHearthstoneRect(true);

            //hs window has height 0 if it just launched, screwing things up if the tracker is started before hs is. 
            //this prevents that from happening. 
            if (hsRect.Height == 0 || Visibility != Visibility.Visible)
                return;

            SetRect(hsRect.Top, hsRect.Left, hsRect.Width, hsRect.Height);
            ReSizePosLists();
            try
            {
                await UpdateCardTooltip();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.ToString(), "UpdateOverlayPosition");
            }
        }

        internal void UpdateTurnTimer(TimerEventArgs timerEventArgs)
        {
            if (timerEventArgs.Running && (timerEventArgs.PlayerSeconds > 0 || timerEventArgs.OpponentSeconds > 0))
            {
                ShowTimers();

                LblTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.Seconds / 60) % 60, timerEventArgs.Seconds % 60);
                LblPlayerTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.PlayerSeconds / 60) % 60, timerEventArgs.PlayerSeconds % 60);
                LblOpponentTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.OpponentSeconds / 60) % 60,
                                                         timerEventArgs.OpponentSeconds % 60);

                if (Config.Instance.Debug)
                {
                    LblDebugLog.Text += string.Format("Current turn: {0} {1} {2} \n", timerEventArgs.CurrentActivePlayer, timerEventArgs.PlayerSeconds,
                                                      timerEventArgs.OpponentSeconds);
                    DebugViewer.ScrollToBottom();
                }
            }
        }

        public void UpdateScaling()
        {
            StackPanelPlayer.RenderTransform = new ScaleTransform(Config.Instance.OverlayPlayerScaling / 100,
                                                                  Config.Instance.OverlayPlayerScaling / 100);
            StackPanelOpponent.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
                                                                    Config.Instance.OverlayOpponentScaling / 100);
        }

        public void HideTimers()
        {
            LblPlayerTurnTime.Visibility = LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Visibility.Hidden;
        }

        public void ShowTimers()
        {
            LblPlayerTurnTime.Visibility =
                LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Config.Instance.HideTimers ? Visibility.Hidden : Visibility.Visible;
        }

        public void UpdatePlayerLayout()
        {
            StackPanelPlayer.Children.Clear();
            foreach (var item in Config.Instance.PanelOrderPlayer)
            {
                switch (item)
                {
                    case "Cards":
                        StackPanelPlayer.Children.Add(ListViewPlayer);
                        break;
                    case "Draw Chances":
                        StackPanelPlayer.Children.Add(CanvasPlayerChance);
                        break;
                    case "Card Counter":
                        StackPanelPlayer.Children.Add(CanvasPlayerCount);
                        break;
                    case "Fatigue Counter":
                        StackPanelPlayer.Children.Add(LblPlayerFatigue);
                        break;
                    case "Deck Title":
                        StackPanelPlayer.Children.Add(LblDeckTitle);
                        break;
                    case "Wins":
                        StackPanelPlayer.Children.Add(LblWins);
                        break;
                }
            }
        }

        public void UpdateOpponentLayout()
        {
            StackPanelOpponent.Children.Clear();
            foreach (var item in Config.Instance.PanelOrderOpponent)
            {
                switch (item)
                {
                    case "Cards":
                        StackPanelOpponent.Children.Add(ListViewOpponent);
                        break;
                    case "Draw Chances":
                        StackPanelOpponent.Children.Add(CanvasOpponentChance);
                        break;
                    case "Card Counter":
                        StackPanelOpponent.Children.Add(CanvasOpponentCount);
                        break;
                    case "Fatigue Counter":
                        StackPanelOpponent.Children.Add(LblOpponentFatigue);
                        break;
                    case "Win Rate":
                        StackPanelOpponent.Children.Add(ViewBoxWinRateAgainst);
                        break;
                }
            }
        }

        public void ShowSecrets(bool force = false, HeroClass? heroClass = null)
        {
            if (Config.Instance.HideSecrets && !force)
                return;

            StackPanelSecrets.Children.Clear();
            var secrets = heroClass == null ? _game.OpponentSecrets.GetSecrets() : _game.OpponentSecrets.GetDefaultSecrets(heroClass.Value);
            foreach (var id in secrets)
            {
                var cardObj = new Controls.Card();
                var card = GameV2.GetCardFromId(id.CardId);
                card.Count = id.AdjustedCount(_game);
                cardObj.SetValue(DataContextProperty, card);
                StackPanelSecrets.Children.Add(cardObj);
            }

            StackPanelSecrets.Visibility = Visibility.Visible;
        }

        public void ShowAdditionalToolTips()
        {
            if (!Config.Instance.AdditionalOverlayTooltips)
                return;
            var card = ToolTipCard.DataContext as Card;
            if (card == null)
                return;
            if (!CardIds.SubCardIds.Keys.Contains(card.Id))
            {
                HideAdditionalToolTips();
                return;
            }

            StackPanelAdditionalTooltips.Children.Clear();
            foreach (var id in CardIds.SubCardIds[card.Id])
            {
                var tooltip = new CardToolTip();
                tooltip.SetValue(DataContextProperty, GameV2.GetCardFromId(id));
                StackPanelAdditionalTooltips.Children.Add(tooltip);
            }

            StackPanelAdditionalTooltips.UpdateLayout();

            //set position
            var tooltipLeft = Canvas.GetLeft(ToolTipCard);
            var left = tooltipLeft < Width / 2 ? tooltipLeft + ToolTipCard.ActualWidth : tooltipLeft - StackPanelAdditionalTooltips.ActualWidth;

            Canvas.SetLeft(StackPanelAdditionalTooltips, left);
            var top = Canvas.GetTop(ToolTipCard) - (StackPanelAdditionalTooltips.ActualHeight / 2 - ToolTipCard.ActualHeight / 2);
            if (top < 0)
                top = 0;
            else if (top + StackPanelAdditionalTooltips.ActualHeight > Height)
                top = Height - StackPanelAdditionalTooltips.ActualHeight;
            Canvas.SetTop(StackPanelAdditionalTooltips, top);

            StackPanelAdditionalTooltips.Visibility = Visibility.Visible;
        }

        public void HideSecrets()
        {
            StackPanelSecrets.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_mouseInput != null)
                UnHookMouse();
        }

        public async Task<bool> UnlockUI()
        {
            _uiMovable = !_uiMovable;
            Update(false);
            if (_uiMovable)
            {
                //if(!Config.Instance.ExtraFeatures)
                HookMouse();
                if (StackPanelSecrets.Visibility != Visibility.Visible)
                {
                    _secretsTempVisible = true;
                    ShowSecrets(true, HeroClass.Mage);
                    //need to wait for panel to actually show up
                    await Task.Delay(50);
                }
                if (LblTurnTime.Visibility != Visibility.Visible)
                    ShowTimers();
                foreach (var movableElement in _movableElements)
                {
                    try
                    {
                        if (!CanvasInfo.Children.Contains(movableElement.Value))
                            CanvasInfo.Children.Add(movableElement.Value);

                        movableElement.Value.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");

                        Canvas.SetTop(movableElement.Value, Canvas.GetTop(movableElement.Key));
                        Canvas.SetLeft(movableElement.Value, Canvas.GetLeft(movableElement.Key));

                        var elementSize = GetUiElementSize(movableElement.Key);
                        if (movableElement.Key == StackPanelPlayer)
                        {
                            if (!TrySetResizeGripHeight(movableElement.Value, Config.Instance.PlayerDeckHeight * Height / 100))
                            {
                                Config.Instance.Reset("PlayerDeckHeight");
                                TrySetResizeGripHeight(movableElement.Value, Config.Instance.PlayerDeckHeight * Height / 100);
                            }
                        }
                        else if (movableElement.Key == StackPanelOpponent)
                        {
                            if (!TrySetResizeGripHeight(movableElement.Value, Config.Instance.OpponentDeckHeight * Height / 100))
                            {
                                Config.Instance.Reset("OpponentDeckHeight");
                                TrySetResizeGripHeight(movableElement.Value, Config.Instance.OpponentDeckHeight * Height / 100);
                            }
                        }
                        else if (movableElement.Key == StackPanelSecrets)
                            movableElement.Value.Height = StackPanelSecrets.ActualHeight > 0 ? StackPanelSecrets.ActualHeight : 0;
                        else
                            movableElement.Value.Height = elementSize.Height > 0 ? elementSize.Height : 0;

                        movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width : 0;

                        movableElement.Value.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(ex.ToString(), "UnlockOverlay");
                    }
                }
            }
            else
            {
                if (!(Config.Instance.ExtraFeatures && Config.Instance.ForceMouseHook))
                    UnHookMouse();
                if (_secretsTempVisible)
                    HideSecrets();
                if (_game.IsInMenu)
                    HideTimers();

                foreach (var movableElement in _movableElements)
                    movableElement.Value.Visibility = Visibility.Collapsed;
            }

            return _uiMovable;
        }

        private bool TrySetResizeGripHeight(ResizeGrip element, double height)
        {
            if (height <= 0)
                return false;
            element.Height = height;
            return true;
        }

        private Size GetUiElementSize(UIElement element)
        {
            if (element == null)
                return new Size();
            var panel = element as StackPanel;
            if (panel != null)
                return new Size(panel.ActualWidth, panel.ActualHeight);
            var block = element as HearthstoneTextBlock;
            if (block != null)
                return new Size(block.ActualWidth, block.ActualHeight);
            return new Size();
        }

        public void HookMouse()
        {
            if (_mouseInput != null)
                return;
            _mouseInput = new User32.MouseInput();
            _mouseInput.LmbDown += MouseInputOnLmbDown;
            _mouseInput.LmbUp += MouseInputOnLmbUp;
            _mouseInput.MouseMoved += MouseInputOnMouseMoved;
            Logger.WriteLine("Enabled mouse hook", "Overlay");
        }

        public void UnHookMouse()
        {
            if (_uiMovable || _mouseInput == null)
                return;
            _mouseInput.Dispose();
            _mouseInput = null;
            Logger.WriteLine("Disabled mouse hook", "Overlay");
        }

        private void OverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //in addition to setting this in mainwindow_load: (in case of minimized)
            var presentationsource = PresentationSource.FromVisual(this);
            if (presentationsource != null) // make sure it's connected
            {
                Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
                Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
            }
        }
    }
}