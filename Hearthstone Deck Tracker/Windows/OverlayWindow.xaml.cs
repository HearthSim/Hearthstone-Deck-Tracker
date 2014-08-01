#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Hearthstone_Deck_Tracker.Controls;
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
		private readonly List<HearthstoneTextBlock> _cardLabels;
		private readonly List<HearthstoneTextBlock> _cardMarkLabels;
		private readonly List<StackPanel> _stackPanelsMarks;
		private readonly Dictionary<UIElement, ResizeGrip> _movableElements; 
		private readonly int _customHeight;
		private readonly int _customWidth;
		private readonly int _offsetX;
		private readonly int _offsetY;
		private readonly User32.MouseInput _mouseInput;
		private int _cardCount;
		private int _opponentCardCount;
		private string _lastSecretsClass;
		private bool _needToRefreshSecrets;
		private bool _playerCardsHidden;
		private bool _opponentCardsHidden;
		private bool _secretsTempVisible;
		private bool _uiMovable;
		private bool _lmbDown;
		private UIElement _selectedUIElement;
		private bool _resizeElement;
		private Point _mousePos;

		public static double Scaling { get; set; }
		public static double OpponentScaling { get; set; }

		public OverlayWindow()
		{
			InitializeComponent();
			//_game = game;

			_mouseInput = new User32.MouseInput();
            _mouseInput.LmbDown += MouseInputOnLmbDown;

	        _mouseInput.LmbUp += MouseInputOnLmbUp;

			_mouseInput.MouseMoved += MouseInputOnMouseMoved;

			ListViewPlayer.ItemsSource = Game.IsUsingPremade ? Game.PlayerDeck : Game.PlayerDrawn;
			ListViewOpponent.ItemsSource = Game.OpponentCards;
			Scaling = 1.0;
			OpponentScaling = 1.0;
			ShowInTaskbar = Config.Instance.ShowInTaskbar;
			if (Config.Instance.VisibleOverlay)
			{
				Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");
			}
			_offsetX = Config.Instance.OffsetX;
			_offsetY = Config.Instance.OffsetY;
			_customWidth = Config.Instance.CustomWidth;
			_customHeight = Config.Instance.CustomHeight;

			_cardLabels = new List<HearthstoneTextBlock>
				{
					LblCard0,
					LblCard1,
					LblCard2,
					LblCard3,
					LblCard4,
					LblCard5,
					LblCard6,
					LblCard7,
					LblCard8,
					LblCard9,
				};
			_cardMarkLabels = new List<HearthstoneTextBlock>
				{
					LblCardMark0,
					LblCardMark1,
					LblCardMark2,
					LblCardMark3,
					LblCardMark4,
					LblCardMark5,
					LblCardMark6,
					LblCardMark7,
					LblCardMark8,
					LblCardMark9,
				};
			_stackPanelsMarks = new List<StackPanel>
				{
					Marks0,
					Marks1,
					Marks2,
					Marks3,
					Marks4,
					Marks5,
					Marks6,
					Marks7,
					Marks8,
					Marks9,
				};
			_movableElements = new Dictionary<UIElement, ResizeGrip>
				{
					{StackPanelPlayer, new ResizeGrip()},
					{StackPanelOpponent, new ResizeGrip()},
					{StackPanelSecrets, new ResizeGrip()},
					{LblTurnTime, new ResizeGrip()}
				};

			UpdateScaling();
		}

		private void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
		{
			if (_selectedUIElement != null)
				Config.Save();
			_selectedUIElement = null;
			_lmbDown = false;
			_resizeElement = false;
		}

		private void MouseInputOnMouseMoved(object sender, EventArgs eventArgs)
	    {
		    if (!_lmbDown) return;

			var pos = User32.GetMousePos();
			var newPos = new Point(pos.X, pos.Y);
		    var delta = new Point((newPos.X - _mousePos.X)*100, (newPos.Y - _mousePos.Y)*100);

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
						Canvas.SetTop(_movableElements[panel], Height * Config.Instance.PlayerDeckTop/100);
						Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.PlayerDeckLeft / 100 -
						   StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);
						
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
					Config.Instance.SecretsTop += delta.Y / Height;
					Config.Instance.SecretsLeft += delta.X / Width;
					Canvas.SetTop(_movableElements[panel], Height * Config.Instance.SecretsTop / 100);
					Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.SecretsLeft / 100);
				}
			}

			var timer = _selectedUIElement as HearthstoneTextBlock;
			if (timer != null)
			{
				if (timer.Name.Contains("Turn"))
				{
					Config.Instance.TimersVerticalPosition += delta.Y / Height;
					Config.Instance.TimersHorizontalPosition += delta.X / Width;
					Canvas.SetTop(_movableElements[timer], Height * Config.Instance.TimersVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[timer], Width * Config.Instance.TimersHorizontalPosition / 100);
				}
			}

		    _mousePos = newPos;

	    }

		private void MouseInputOnLmbDown(object sender, EventArgs eventArgs)
		{
			if (!User32.IsForegroundWindow("Hearthstone")) return;

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
							if (Math.Abs(relativePos.X - movableElement.Value.ActualWidth) < 30 && Math.Abs(relativePos.Y - movableElement.Value.ActualHeight) < 30)
								_resizeElement = true;

							_selectedUIElement = movableElement.Key;
							return;
						}
					}

					var timer = movableElement.Key as HearthstoneTextBlock;
					if (timer != null)
					{
						if (PointInsideControl(relativePos, timer.ActualWidth, timer.ActualHeight))
						{
							if (Math.Abs(relativePos.X - timer.ActualWidth) < 30 && Math.Abs(relativePos.Y - timer.ActualHeight) < 30)
								_resizeElement = true;

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
			var leftPanel = Canvas.GetLeft(StackPanelOpponent) < 200 ? StackPanelOpponent : StackPanelPlayer;
			if (leftPanel != null && !Config.Instance.HideDecksInOverlay)
			{
				//if panel visible, only continue of click was in the button left corner
				if (!(clickPos.X < 150 && clickPos.Y > Height - 100) && leftPanel.Visibility == Visibility.Visible) 
					return;

				var checkForFriendsList = true;
				if (leftPanel.Equals(StackPanelPlayer) && Config.Instance.HidePlayerCards)
					checkForFriendsList = false;
				else if (leftPanel.Equals(StackPanelOpponent) && Config.Instance.HideOpponentCards)
					checkForFriendsList = false;

				if (checkForFriendsList)
				{
					if (await Helper.FriendsListOpen())
					{
						var needToHide = Canvas.GetTop(leftPanel) + leftPanel.ActualHeight > Height*0.3;
						if (needToHide)
						{
							leftPanel.Visibility = Visibility.Collapsed;
							if (leftPanel.Equals(StackPanelPlayer))
								_playerCardsHidden = true;
							else
								_opponentCardsHidden = true;
						}
					}
					else if (leftPanel.Visibility == Visibility.Collapsed)
					{
						leftPanel.Visibility = Visibility.Visible;
						if (leftPanel.Equals(StackPanelPlayer))
							_playerCardsHidden = false;
						else
							_opponentCardsHidden = false;
					}
				}
			}
		}

		private void GrayOutSecrets(Point mousePos)
		{
			if (!PointInsideControl(StackPanelSecrets.PointFromScreen(mousePos), StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
				return;

				var card = ToolTipCard.DataContext as Card;
				if (card == null) return;

				// 1: normal, 0: grayed out
				card.Count = card.Count == 0 ? 1 : 0;


				//reload secrets panel
				var cards = StackPanelSecrets.Children.OfType<Controls.Card>().Select(c => c.DataContext).OfType<Card>().ToList();

				StackPanelSecrets.Children.Clear();
				foreach (var c in cards)
				{
					var cardObj = new Controls.Card();
					cardObj.SetValue(DataContextProperty, c);
					StackPanelSecrets.Children.Add(cardObj);
				}

				//reset secrets when new secret is played
				_needToRefreshSecrets = true;
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
			{
				Helper.SortCardCollection(ListViewOpponent.ItemsSource, Config.Instance.CardSortingClassFirst);
			}
			_opponentCardCount = cardCount;

			LblOpponentCardCount.Text = "Hand: " + cardCount;
			LblOpponentDeckCount.Text = "Deck: " + cardsLeftInDeck;


			if (cardsLeftInDeck <= 0) return;

			var handWithoutCoin = cardCount - (Game.OpponentHasCoin ? 1 : 0);


			var holdingNextTurn2 =
				Math.Round(100.0f * Helper.DrawProbability(2, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 2);
			var drawNextTurn2 = Math.Round(200.0f / cardsLeftInDeck, 2);
			LblOpponentDrawChance2.Text = "[2]: " + holdingNextTurn2 + "% / " + drawNextTurn2 + "%";

			var holdingNextTurn =
				Math.Round(100.0f * Helper.DrawProbability(1, (cardsLeftInDeck + handWithoutCoin), handWithoutCoin + 1), 2);
			var drawNextTurn = Math.Round(100.0f / cardsLeftInDeck, 2);
			LblOpponentDrawChance1.Text = "[1]: " + holdingNextTurn + "% / " + drawNextTurn + "%";
		}

		private void SetCardCount(int cardCount, int cardsLeftInDeck)
		{
			//previous < current -> draw
			if (_cardCount < cardCount)
			{
				Helper.SortCardCollection(ListViewPlayer.ItemsSource, Config.Instance.CardSortingClassFirst);
			}
			_cardCount = cardCount;
			LblCardCount.Text = "Hand: " + cardCount;
			LblDeckCount.Text = "Deck: " + cardsLeftInDeck;

			if (cardsLeftInDeck <= 0) return;

			LblDrawChance2.Text = "[2]: " + Math.Round(200.0f / cardsLeftInDeck, 2) + "%";
			LblDrawChance1.Text = "[1]: " + Math.Round(100.0f / cardsLeftInDeck, 2) + "%";
		}

		public void ShowOverlay(bool enable)
		{
			if (enable)
				Show();
			else Hide();
		}

		private void SetRect(int top, int left, int width, int height)
		{
			Top = top + _offsetY;
			Left = left + _offsetX;
			Width = (_customWidth == -1) ? width : _customWidth;
			Height = (_customHeight == -1) ? height : _customHeight;
			CanvasInfo.Width = (_customWidth == -1) ? width : _customWidth;
			CanvasInfo.Height = (_customHeight == -1) ? height : _customHeight;
		}

		private void ReSizePosLists()
		{
			//player
			if (((Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100) -
				 (ListViewPlayer.Items.Count * 35 * Scaling)) < 1 || Scaling < 1)
			{
				var previousScaling = Scaling;
				Scaling = (Height * Config.Instance.PlayerDeckHeight / (Config.Instance.OverlayPlayerScaling / 100) / 100) /
						  (ListViewPlayer.Items.Count * 35);
				if (Scaling > 1)
					Scaling = 1;

				if (previousScaling != Scaling)
					ListViewPlayer.Items.Refresh();
			}

			Canvas.SetTop(StackPanelPlayer, Height * Config.Instance.PlayerDeckTop / 100);
			Canvas.SetLeft(StackPanelPlayer,
						   Width * Config.Instance.PlayerDeckLeft / 100 -
						   StackPanelPlayer.ActualWidth * Config.Instance.OverlayPlayerScaling / 100);

			//opponent
			if (((Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100) -
				 (ListViewOpponent.Items.Count * 35 * OpponentScaling)) < 1 || OpponentScaling < 1)
			{
				var previousScaling = OpponentScaling;
				OpponentScaling = (Height * Config.Instance.OpponentDeckHeight / (Config.Instance.OverlayOpponentScaling / 100) / 100) /
								  (ListViewOpponent.Items.Count * 35);
				if (OpponentScaling > 1)
					OpponentScaling = 1;

				if (previousScaling != OpponentScaling)
					ListViewOpponent.Items.Refresh();
			}


			Canvas.SetTop(StackPanelOpponent, Height * Config.Instance.OpponentDeckTop / 100);
			Canvas.SetLeft(StackPanelOpponent, Width * Config.Instance.OpponentDeckLeft / 100);

			//Secrets
			Canvas.SetTop(StackPanelSecrets, Height * Config.Instance.SecretsTop / 100);
			Canvas.SetLeft(StackPanelSecrets, Width * Config.Instance.SecretsLeft / 100);

			// Timers
			Canvas.SetTop(LblTurnTime,
						  Height * Config.Instance.TimersVerticalPosition / 100 - 5);
			Canvas.SetLeft(LblTurnTime, Width * Config.Instance.TimersHorizontalPosition / 100);

			Canvas.SetTop(LblOpponentTurnTime,
						  Height * Config.Instance.TimersVerticalPosition / 100 -
						  Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblOpponentTurnTime,
						   (Width * Config.Instance.TimersHorizontalPosition / 100) + Config.Instance.TimersHorizontalSpacing);

			Canvas.SetTop(LblPlayerTurnTime,
						  Height * Config.Instance.TimersVerticalPosition / 100 +
						  Config.Instance.TimersVerticalSpacing);
			Canvas.SetLeft(LblPlayerTurnTime,
						   Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);


			Canvas.SetTop(LblGrid, Height * 0.03);


			var ratio = Width / Height;
			LblGrid.Width = ratio < 1.5 ? Width * 0.3 : Width * 0.15 * (ratio / 1.33);
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
				Topmost = false;
				Topmost = true;
				Logger.WriteLine("Refreshed overlay topmost status");
			}


			var handCount = Game.OpponentHandCount;
			if (handCount < 0) handCount = 0;
			if (handCount > 10) handCount = 10;
			//offset label-grid based on handcount
			Canvas.SetLeft(LblGrid, Width / 2 - LblGrid.ActualWidth / 2 - Width * 0.002 * handCount);

			var labelDistance = LblGrid.Width / (handCount + 1);

			for (int i = 0; i < handCount; i++)
			{
				var offset = labelDistance * Math.Abs(i - handCount / 2);
				offset = offset * offset * 0.0015;

				if (handCount % 2 == 0)
				{
					if (i < handCount / 2 - 1)
					{
						//even hand count -> both middle labels at same height
						offset = labelDistance * Math.Abs(i - (handCount / 2 - 1));
						offset = offset * offset * 0.0015;
						_stackPanelsMarks[i].Margin = new Thickness(0, -offset, 0, 0);
					}
					else if (i > handCount / 2)
					{
						_stackPanelsMarks[i].Margin = new Thickness(0, -offset, 0, 0);
					}
					else
					{
						var left = (handCount == 2 && i == 0) ? Width * 0.02 : 0;
						_stackPanelsMarks[i].Margin = new Thickness(left, 0, 0, 0);
					}
				}
				else
				{
					_stackPanelsMarks[i].Margin = new Thickness(0, -offset, 0, 0);
				}

				if (!Config.Instance.HideOpponentCardAge)
				{
					_cardLabels[i].Text = Game.OpponentHandAge[i].ToString();
					_cardLabels[i].Visibility = Visibility.Visible;
				}
				else
				{
					_cardLabels[i].Visibility = Visibility.Collapsed;
				}

				if (!Config.Instance.HideOpponentCardMarks)
				{
					_cardMarkLabels[i].Text = ((char)Game.OpponentHandMarks[i]).ToString();
					_cardMarkLabels[i].Visibility = Visibility.Visible;
				}
				else
				{
					_cardMarkLabels[i].Visibility = Visibility.Collapsed;
				}

				_stackPanelsMarks[i].Visibility = Visibility.Visible;
			}
			for (int i = handCount; i < 10; i++)
			{
				_stackPanelsMarks[i].Visibility = Visibility.Collapsed;
			}

			StackPanelPlayer.Opacity = Config.Instance.PlayerOpacity / 100;
			StackPanelOpponent.Opacity = Config.Instance.OpponentOpacity / 100;
			Opacity = Config.Instance.OverlayOpacity / 100;

			if(!_playerCardsHidden)
				StackPanelPlayer.Visibility = Config.Instance.HideDecksInOverlay ? Visibility.Collapsed : Visibility.Visible;

			if(!_opponentCardsHidden)
				StackPanelOpponent.Visibility = Config.Instance.HideDecksInOverlay ? Visibility.Collapsed : Visibility.Visible;

			LblDrawChance1.Visibility = Config.Instance.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblDrawChance2.Visibility = Config.Instance.HideDrawChances ? Visibility.Collapsed : Visibility.Visible;
			LblCardCount.Visibility = Config.Instance.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblDeckCount.Visibility = Config.Instance.HidePlayerCardCount ? Visibility.Collapsed : Visibility.Visible;

			LblOpponentDrawChance1.Visibility = Config.Instance.HideOpponentDrawChances
													? Visibility.Collapsed
													: Visibility.Visible;
			LblOpponentDrawChance2.Visibility = Config.Instance.HideOpponentDrawChances
													? Visibility.Collapsed
													: Visibility.Visible;
			LblOpponentCardCount.Visibility = Config.Instance.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;
			LblOpponentDeckCount.Visibility = Config.Instance.HideOpponentCardCount ? Visibility.Collapsed : Visibility.Visible;

			ListViewOpponent.Visibility = Config.Instance.HideOpponentCards ? Visibility.Collapsed : Visibility.Visible;
			ListViewPlayer.Visibility = Config.Instance.HidePlayerCards ? Visibility.Collapsed : Visibility.Visible;

			LblGrid.Visibility = Game.IsInMenu ? Visibility.Hidden : Visibility.Visible;

			DebugViewer.Visibility = Config.Instance.Debug ? Visibility.Visible : Visibility.Hidden;
			DebugViewer.Width = (Width * Config.Instance.TimerLeft / 100);

			SetCardCount(Game.PlayerHandCount,
						 Game.IsUsingPremade ? Game.PlayerDeck.Sum(c => c.Count) : 30 - Game.PlayerDrawn.Sum(c => c.Count));

			SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount);

			ReSizePosLists();
		}

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight)
		{
			if (pos.X > 0 && pos.X < actualWidth)
			{
				if (pos.Y > 0 && pos.Y < actualHeight)
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateCardTooltip()
		{
			//todo: if distance to left or right of overlay < tooltip width -> switch side
			var pos = User32.GetMousePos();
			var relativePlayerDeckPos = StackPanelPlayer.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentDeckPos = ListViewOpponent.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeSecretsPos = StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y));
			var visibility = Config.Instance.OverlayCardToolTips ? Visibility.Visible : Visibility.Hidden;

			//player card tooltips
			if (PointInsideControl(relativePlayerDeckPos, ListViewPlayer.ActualWidth, ListViewPlayer.ActualHeight))
			{
				//card size = card list height / ammount of cards
				var cardSize = ListViewPlayer.ActualHeight / ListViewPlayer.Items.Count;
				var cardIndex = (int)(relativePlayerDeckPos.Y / cardSize);
				if (cardIndex < 0 || cardIndex >= ListViewPlayer.Items.Count)
					return;

				ToolTipCard.SetValue(DataContextProperty, ListViewPlayer.Items[cardIndex]);

				//offset is affected by scaling
				var topOffset = Canvas.GetTop(StackPanelPlayer) + cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100;

				//prevent tooltip from going outside of the overlay
				if (topOffset + ToolTipCard.ActualHeight > Height)
					topOffset = Height - ToolTipCard.ActualHeight;

				SetTooltipPosition(topOffset, StackPanelPlayer);

				ToolTipCard.Visibility = visibility;
			}
			//opponent card tooltips
			else if (PointInsideControl(relativeOpponentDeckPos, ListViewOpponent.ActualWidth, ListViewOpponent.ActualHeight))
			{
				//card size = card list height / ammount of cards
				var cardSize = ListViewOpponent.ActualHeight / ListViewOpponent.Items.Count;
				var cardIndex = (int)(relativeOpponentDeckPos.Y / cardSize);
				if (cardIndex < 0 || cardIndex >= ListViewOpponent.Items.Count)
					return;

				ToolTipCard.SetValue(DataContextProperty, ListViewOpponent.Items[cardIndex]);

				//offset is affected by scaling
				var topOffset = Canvas.GetTop(StackPanelOpponent) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100;

				//prevent tooltip from going outside of the overlay
				if (topOffset + ToolTipCard.ActualHeight > Height)
					topOffset = Height - ToolTipCard.ActualHeight;

				SetTooltipPosition(topOffset, StackPanelOpponent);

				ToolTipCard.Visibility = visibility;
			}
			else if (PointInsideControl(relativeSecretsPos, StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
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

				ToolTipCard.Visibility = visibility;
			}
			else
			{
				ToolTipCard.Visibility = Visibility.Hidden;
			}
		}

		private void SetTooltipPosition(double yOffset, StackPanel stackpanel)
		{
			Canvas.SetTop(ToolTipCard, yOffset);

			if (Canvas.GetLeft(stackpanel) < Width / 2)
			{
				Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100);
			}
			else
			{
				Canvas.SetLeft(ToolTipCard, Canvas.GetLeft(stackpanel) - ToolTipCard.Width);
			}
		}

		public void UpdatePosition()
		{
			//hide the overlay depenting on options
			ShowOverlay(!(
							 (Config.Instance.HideInBackground && !User32.IsForegroundWindow("Hearthstone"))
							 || (Config.Instance.HideInMenu && Game.IsInMenu)
							 || Config.Instance.HideOverlay));


			var hsRect = User32.GetHearthstoneRect(true);

			//hs window has height 0 if it just launched, screwing things up if the tracker is started before hs is. 
			//this prevents that from happening. 
			if (hsRect.Height == 0)
			{
				return;
			}

			SetRect(hsRect.Top, hsRect.Left, hsRect.Width, hsRect.Height);
			ReSizePosLists();

			UpdateCardTooltip();
		}

		internal void UpdateTurnTimer(TimerEventArgs timerEventArgs)
		{
			if (timerEventArgs.Running && (timerEventArgs.PlayerSeconds > 0 || timerEventArgs.OpponentSeconds > 0))
			{
				ShowTimers();

				LblTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.Seconds / 60) % 60,
												 timerEventArgs.Seconds % 60);
				LblPlayerTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.PlayerSeconds / 60) % 60,
													   timerEventArgs.PlayerSeconds % 60);
				LblOpponentTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.OpponentSeconds / 60) % 60,
														 timerEventArgs.OpponentSeconds % 60);

				if (Config.Instance.Debug)
				{
					LblDebugLog.Text += string.Format("Current turn: {0} {1} {2} \n",
													  timerEventArgs.CurrentTurn.ToString(),
													  timerEventArgs.PlayerSeconds.ToString(),
													  timerEventArgs.OpponentSeconds.ToString());
					DebugViewer.ScrollToBottom();
				}
			}
		}

		public void UpdateScaling()
		{
			Config.Instance.OverlayPlayerScaling += 0.00001;
			Config.Instance.OverlayOpponentScaling += 0.00001;
			StackPanelPlayer.RenderTransform = new ScaleTransform(Config.Instance.OverlayPlayerScaling / 100,
																  Config.Instance.OverlayPlayerScaling / 100);
			StackPanelOpponent.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
																	Config.Instance.OverlayOpponentScaling / 100);
			StackPanelSecrets.RenderTransform = new ScaleTransform(Config.Instance.OverlayOpponentScaling / 100,
																	Config.Instance.OverlayOpponentScaling / 100);
		}

		public void HideTimers()
		{
			LblPlayerTurnTime.Visibility = LblOpponentTurnTime.Visibility = LblTurnTime.Visibility = Visibility.Hidden;
		}

		public void ShowTimers()
		{
			LblPlayerTurnTime.Visibility =
				LblOpponentTurnTime.Visibility =
				LblTurnTime.Visibility = Config.Instance.HideTimers ? Visibility.Hidden : Visibility.Visible;
		}

		public void SetOpponentTextLocation(bool top)
		{
			StackPanelOpponent.Children.Clear();
			if (top)
			{
				StackPanelOpponent.Children.Add(LblOpponentDrawChance2);
				StackPanelOpponent.Children.Add(LblOpponentDrawChance1);
				StackPanelOpponent.Children.Add(StackPanelOpponentCount);
				StackPanelOpponent.Children.Add(ListViewOpponent);
			}
			else
			{
				StackPanelOpponent.Children.Add(ListViewOpponent);
				StackPanelOpponent.Children.Add(LblOpponentDrawChance2);
				StackPanelOpponent.Children.Add(LblOpponentDrawChance1);
				StackPanelOpponent.Children.Add(StackPanelOpponentCount);
			}
		}

		public void SetPlayerTextLocation(bool top)
		{
			StackPanelPlayer.Children.Clear();
			if (top)
			{
				StackPanelPlayer.Children.Add(StackPanelPlayerDraw);
				StackPanelPlayer.Children.Add(StackPanelPlayerCount);
				StackPanelPlayer.Children.Add(ListViewPlayer);
			}
			else
			{
				StackPanelPlayer.Children.Add(ListViewPlayer);
				StackPanelPlayer.Children.Add(StackPanelPlayerDraw);
				StackPanelPlayer.Children.Add(StackPanelPlayerCount);
			}
		}

		public void ShowSecrets(string hsClass, bool force = false)
		{
			if (Config.Instance.HideSecrets && !force) return;
			if (_lastSecretsClass != hsClass || _needToRefreshSecrets)
			{
				List<string> ids;
				switch (hsClass)
				{
					case "Hunter":
						ids = Game.SecretIdsHunter;
						break;
					case "Mage":
						ids = Game.SecretIdsMage;
						break;
					case "Paladin":
						ids = Game.SecretIdsPaladin;
						break;
					default:
						return;

				}
				StackPanelSecrets.Children.Clear();
				foreach (var id in ids)
				{
					var cardObj = new Controls.Card();
					cardObj.SetValue(DataContextProperty, Game.GetCardFromId(id));
					StackPanelSecrets.Children.Add(cardObj);
				}
				_lastSecretsClass = hsClass;
				_needToRefreshSecrets = false;
			}
			StackPanelSecrets.Visibility = Visibility.Visible;

		}

		public void HideSecrets()
		{
			StackPanelSecrets.Visibility = Visibility.Collapsed;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			_mouseInput.Dispose();
		}

	    public async Task<bool> UnlockUI()
	    {
		    _uiMovable = !_uiMovable;
			if (_uiMovable)
			{
				if(StackPanelSecrets.Visibility != Visibility.Visible)
				{
					_secretsTempVisible = true;
					ShowSecrets("Mage", true);
					//need to wait for panel to actually show up
					await Task.Delay(50);
				}
				if (LblTurnTime.Visibility != Visibility.Visible)
				{
					ShowTimers();
				}
				foreach (var movableElement in _movableElements)
				{
					if (!CanvasInfo.Children.Contains(movableElement.Value))
						CanvasInfo.Children.Add(movableElement.Value);

					movableElement.Value.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#4C0000FF");

					Canvas.SetTop(movableElement.Value, Canvas.GetTop(movableElement.Key));
					Canvas.SetLeft(movableElement.Value, Canvas.GetLeft(movableElement.Key));

					Size elementSize = GetUiElementSize(movableElement.Key);
					if (movableElement.Key == StackPanelPlayer)
					{
						movableElement.Value.Height = Config.Instance.PlayerDeckHeight*Height/100;

					}
					else if (movableElement.Key == StackPanelOpponent)
					{
						movableElement.Value.Height = Config.Instance.OpponentDeckHeight*Height/100;
					}
					else
					{
						movableElement.Value.Height = elementSize.Height;
					}

					movableElement.Value.Width = elementSize.Width;

					movableElement.Value.Visibility = Visibility.Visible;
				}
			}
			else
			{
				if(_secretsTempVisible)
					HideSecrets();
				if(Game.IsInMenu)
					HideTimers();

				foreach (var movableElement in _movableElements)
				{
					movableElement.Value.Visibility = Visibility.Collapsed;
				}
			}

		    return _uiMovable;
	    }

		private Size GetUiElementSize(UIElement element)
		{
			if(element == null) return new Size();
			if(element is StackPanel)
				return new Size(((StackPanel)element).ActualWidth, ((StackPanel)element).ActualHeight);
			if (element is HearthstoneTextBlock)
				return new Size(((HearthstoneTextBlock)element).ActualWidth, ((HearthstoneTextBlock)element).ActualHeight);
			return new Size();
		}
	}
}