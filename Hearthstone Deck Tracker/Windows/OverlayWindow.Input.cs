using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{

		private void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
		{
			if (Visibility != Visibility.Visible)
				return;
			if (_selectedUiElement != null)
				Config.Save();
			_selectedUiElement = null;
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
			_mousePos = newPos;

			var panel = _selectedUiElement as StackPanel;
			if (panel != null)
			{
				if (panel.Equals(StackPanelPlayer))
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
					return;
				}
				if (panel.Equals(StackPanelOpponent))
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
					return;
				}
				if (panel.Equals(StackPanelSecrets))
				{
					if (!_resizeElement)
					{
						Config.Instance.SecretsTop += delta.Y / Height;
						Config.Instance.SecretsLeft += delta.X / Width;
						Canvas.SetTop(_movableElements[panel], Height * Config.Instance.SecretsTop / 100);
						Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.SecretsLeft / 100);
					}
				}
			}

			var timer = _selectedUiElement as HearthstoneTextBlock;
			if (timer != null)
			{
				if (timer.Equals(LblPlayerTurnTime))
				{
					Config.Instance.TimersVerticalSpacing += delta.Y / 100;
					Config.Instance.TimersHorizontalSpacing += delta.X / 100;
					Canvas.SetTop(_movableElements[timer],
								  Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
					Canvas.SetLeft(_movableElements[timer],
								   Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
					return;
				}
				if (timer.Equals(LblTurnTime))
				{
					Config.Instance.TimersVerticalPosition += delta.Y / Height;
					Config.Instance.TimersHorizontalPosition += delta.X / Width;
					Canvas.SetTop(_movableElements[timer], Height * Config.Instance.TimersVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[timer], Width * Config.Instance.TimersHorizontalPosition / 100);

					var playerTimer =
						_movableElements.First(e => e.Key is HearthstoneTextBlock && ((HearthstoneTextBlock)e.Key).Name.Contains("Player")).Value;
					Canvas.SetTop(playerTimer, Height * Config.Instance.TimersVerticalPosition / 100 + Config.Instance.TimersVerticalSpacing);
					Canvas.SetLeft(playerTimer, Width * Config.Instance.TimersHorizontalPosition / 100 + Config.Instance.TimersHorizontalSpacing);
					return;
				}
			}

			var grid = _selectedUiElement as Grid;
			if (grid != null)
			{
				if (grid.Equals(IconBoardAttackPlayer))
				{
					Config.Instance.AttackIconPlayerVerticalPosition += delta.Y / Height;
					Config.Instance.AttackIconPlayerHorizontalPosition += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[grid], Height * Config.Instance.AttackIconPlayerVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[grid],
								   Helper.GetScaledXPos(Config.Instance.AttackIconPlayerHorizontalPosition / 100, (int)Width, ScreenRatio));
					return;
				}
				if (grid.Equals(IconBoardAttackOpponent))
				{
					Config.Instance.AttackIconOpponentVerticalPosition += delta.Y / Height;
					Config.Instance.AttackIconOpponentHorizontalPosition += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[grid], Height * Config.Instance.AttackIconOpponentVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[grid],
								   Helper.GetScaledXPos(Config.Instance.AttackIconOpponentHorizontalPosition / 100, (int)Width, ScreenRatio));
				}
			}
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
					if (panel != null && PointInsideControl(relativePos, movableElement.Value.ActualWidth, movableElement.Value.ActualHeight))
					{
						if (Math.Abs(relativePos.X - movableElement.Value.ActualWidth) < 30
						   && Math.Abs(relativePos.Y - movableElement.Value.ActualHeight) < 30)
							_resizeElement = true;

						_selectedUiElement = movableElement.Key;
						return;
					}
					var timer = movableElement.Key as HearthstoneTextBlock;
					if (timer != null && PointInsideControl(relativePos, movableElement.Value.ActualWidth, movableElement.Value.ActualHeight))
					{
						_selectedUiElement = movableElement.Key;
						return;
					}
					var grid = movableElement.Key as Grid;
					if (grid != null && PointInsideControl(relativePos, movableElement.Value.ActualWidth, movableElement.Value.ActualHeight))
					{
						_selectedUiElement = movableElement.Key;
						return;
					}
				}
			}

			HideCardsWhenFriendsListOpen(PointFromScreen(_mousePos));
			GrayOutSecrets(_mousePos);
		}


		public async Task<bool> UnlockUi()
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
						Log.Info(ex.ToString());
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
			var grid = element as Grid;
			if (grid != null)
				return new Size(grid.ActualWidth, grid.ActualHeight);
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
			Log.Info("Enabled mouse hook");
		}

		public void UnHookMouse()
		{
			if (_uiMovable || _mouseInput == null)
				return;
			_mouseInput.Dispose();
			_mouseInput = null;
			Log.Info("Disabled mouse hook");
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
								var isPlayerPanel = panel.Equals(StackPanelPlayer);
								Log.Info("Friendslist is open! Hiding " + (isPlayerPanel ? "player" : "opponent") + " panel.");
								panel.Visibility = Visibility.Collapsed;
								if (isPlayerPanel)
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

	}
}
