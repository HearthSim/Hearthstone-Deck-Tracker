using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.ActiveEffects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
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


			if(_selectedUiElement is Border border)
			{
				if(border.Equals(BorderStackPanelPlayer))
				{
					if(_resizeElement)
					{
						Config.Instance.PlayerDeckHeight += delta.Y / Height;
						_movableElements[border].Height = Height * Config.Instance.PlayerDeckHeight / 100;
						OnPropertyChanged(nameof(PlayerStackHeight));
					}
					else
					{
						Config.Instance.PlayerDeckTop += delta.Y / Height;
						Config.Instance.PlayerDeckLeft += delta.X / Width;
						Canvas.SetTop(_movableElements[border], Height * Config.Instance.PlayerDeckTop / 100);
						Canvas.SetLeft(_movableElements[border], Width * Config.Instance.PlayerDeckLeft / 100
						                                         - StackPanelPlayer.ActualWidth
						                                         * Config.Instance.OverlayPlayerScaling / 100);
					}

					return;
				}

				if(border.Equals(BorderStackPanelOpponent))
				{
					if(_resizeElement)
					{
						Config.Instance.OpponentDeckHeight += delta.Y / Height;
						_movableElements[border].Height = Height * Config.Instance.OpponentDeckHeight / 100;
						OnPropertyChanged(nameof(OpponentStackHeight));
					}
					else
					{
						Config.Instance.OpponentDeckTop += delta.Y / Height;
						Config.Instance.OpponentDeckLeft += delta.X / Width;
						Canvas.SetTop(_movableElements[border], Height * Config.Instance.OpponentDeckTop / 100);
						Canvas.SetLeft(_movableElements[border], Width * Config.Instance.OpponentDeckLeft / 100);
					}

					return;
				}
			}


			if(_selectedUiElement is Panel panel)
			{
				if(panel.Equals(StackPanelSecrets))
				{
					if(!_resizeElement)
					{
						Config.Instance.SecretsTop += delta.Y / Height * Config.Instance.SecretsPanelScaling;
						Config.Instance.SecretsLeft += delta.X / Width * Config.Instance.SecretsPanelScaling;
						Canvas.SetTop(_movableElements[panel], Height * Config.Instance.SecretsTop / 100);
						Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.SecretsLeft / 100);
					}
				}

				if(panel.Equals(BattlegroundsSessionStackPanel))
				{
					if(!_resizeElement)
					{
						Config.Instance.SessionRecapTop += delta.Y / Height;
						Config.Instance.SessionRecapLeft += delta.X / Width;
						Canvas.SetTop(_movableElements[panel], Height * Config.Instance.SessionRecapTop / 100);
						Canvas.SetLeft(_movableElements[panel], Width * Config.Instance.SessionRecapLeft / 100);
						return;
					}
				}

				if(panel.Equals(IconBoardAttackPlayer))
				{
					Config.Instance.AttackIconPlayerVerticalPosition += delta.Y / Height;
					Config.Instance.AttackIconPlayerHorizontalPosition += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[panel], Height * Config.Instance.AttackIconPlayerVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[panel], Helper.GetScaledXPos(Config.Instance.AttackIconPlayerHorizontalPosition / 100, (int)Width, ScreenRatio));
					return;
				}
				if(panel.Equals(IconBoardAttackOpponent))
				{
					Config.Instance.AttackIconOpponentVerticalPosition += delta.Y / Height;
					Config.Instance.AttackIconOpponentHorizontalPosition += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[panel], Height * Config.Instance.AttackIconOpponentVerticalPosition / 100);
					Canvas.SetLeft(_movableElements[panel], Helper.GetScaledXPos(Config.Instance.AttackIconOpponentHorizontalPosition / 100, (int)Width, ScreenRatio));
				}
			}

			if(_selectedUiElement is WotogCounter wotogIcons)
			{
				if(wotogIcons.Equals(WotogIconsPlayer))
				{
					Config.Instance.WotogIconsPlayerVertical += delta.Y / Height;
					Config.Instance.WotogIconsPlayerHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[wotogIcons], Height * Config.Instance.WotogIconsPlayerVertical / 100);
					Canvas.SetLeft(_movableElements[wotogIcons], Helper.GetScaledXPos(Config.Instance.WotogIconsPlayerHorizontal / 100, (int)Width, ScreenRatio));
					return;
				}
				if(wotogIcons.Equals(WotogIconsOpponent))
				{
					Config.Instance.WotogIconsOpponentVertical += delta.Y / Height;
					Config.Instance.WotogIconsOpponentHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[wotogIcons], Height * Config.Instance.WotogIconsOpponentVertical / 100);
					Canvas.SetLeft(_movableElements[wotogIcons], Helper.GetScaledXPos(Config.Instance.WotogIconsOpponentHorizontal / 100, (int)Width, ScreenRatio));
					return;
				}
			}

			if(_selectedUiElement is ActiveEffectsOverlay activeEffects)
			{
				if(activeEffects.Equals(PlayerActiveEffects))
				{
					Config.Instance.PlayerActiveEffectsVertical += delta.Y / Height;
					Config.Instance.PlayerActiveEffectsHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[activeEffects], Height * Config.Instance.PlayerActiveEffectsVertical / 100);
					Canvas.SetLeft(_movableElements[activeEffects], Helper.GetScaledXPos(Config.Instance.PlayerActiveEffectsHorizontal / 100, (int)Width, ScreenRatio));
					return;
				}
				if(activeEffects.Equals(OpponentActiveEffects))
				{
					Config.Instance.OpponentActiveEffectsVertical -= delta.Y / Height;
					Config.Instance.OpponentActiveEffectsHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[activeEffects], Height - (OpponentActiveEffects.ActualHeight * _activeEffectsScale + Height * Config.Instance.OpponentActiveEffectsVertical / 100));
					Canvas.SetLeft(_movableElements[activeEffects], Helper.GetScaledXPos(Config.Instance.OpponentActiveEffectsHorizontal / 100, (int)Width, ScreenRatio));
				}
			}

			if(_selectedUiElement is CountersOverlay counters)
			{
				if(counters.Equals(PlayerCounters))
				{
					Config.Instance.PlayerCountersVertical += delta.Y / Height;
					Config.Instance.PlayerCountersHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[counters], Height * Config.Instance.PlayerCountersVertical / 100);
					Canvas.SetLeft(_movableElements[counters], Helper.GetScaledXPos(Config.Instance.PlayerCountersHorizontal / 100, (int)Width, ScreenRatio));
					return;
				}
				if(counters.Equals(OpponentCounters))
				{
					Config.Instance.OpponentCountersVertical -= delta.Y / Height;
					Config.Instance.OpponentCountersHorizontal += delta.X / (Width * ScreenRatio);
					Canvas.SetTop(_movableElements[counters], Height - (OpponentCounters.ActualHeight * _activeEffectsScale + Height * Config.Instance.OpponentCountersVertical / 100));
					Canvas.SetLeft(_movableElements[counters], Helper.GetScaledXPos(Config.Instance.OpponentCountersHorizontal / 100, (int)Width, ScreenRatio));
				}
			}

			if (_selectedUiElement is HearthstoneTextBlock timer)
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
				foreach (var element in _movableElements)
				{
					var relativePos = element.Value.PointFromScreen(_mousePos);
					if(element.Key is Border && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						if(Math.Abs(relativePos.X - element.Value.ActualWidth) < 30
						   && Math.Abs(relativePos.Y - element.Value.ActualHeight) < 30)
							_resizeElement = true;

						_selectedUiElement = element.Key;
						return;
					}
					if(element.Key is Panel && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						_selectedUiElement = element.Key;
						return;
					}
					if(element.Key is HearthstoneTextBlock && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						_selectedUiElement = element.Key;
						return;
					}
					if(element.Key is WotogCounter && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						_selectedUiElement = element.Key;
						return;
					}
					if(element.Key is ActiveEffectsOverlay && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						_selectedUiElement = element.Key;
						return;
					}
					if(element.Key is CountersOverlay && PointInsideControl(relativePos, element.Value.ActualWidth, element.Value.ActualHeight))
					{
						_selectedUiElement = element.Key;
						return;
					}
				}
			}

			// Can be removed now that we have masking?
			HideCardsWhenFriendsListOpen(PointFromScreen(_mousePos));
		}

		public async Task<bool> UnlockUi(bool battlegroundsMode = false)
		{
			_uiMovable = !_uiMovable;
			Update(false);
			if (_uiMovable)
			{
				HookMouse();
				if (battlegroundsMode)
				{
					Config.Instance.HideDecksInOverlay = true;
					Config.Instance.HidePlayerFatigueCount = true;
					Config.Instance.HideOpponentFatigueCount = true;
				}
				else
				{
					if (StackPanelSecrets.Visibility != Visibility.Visible)
					{
						_secretsTempVisible = true;
						var secrets = CardIds.Secrets.Mage.All.Select(x => Database.GetCardFromId(x.Ids[0]))
							.WhereNotNull().ToList();
						ShowSecrets(secrets, true);
						//need to wait for panel to actually show up
						await Task.Delay(50);
					}
					if(LblTurnTime.Visibility != Visibility.Visible)
						ShowTimers();
					WotogIconsPlayer.ForceShow(true);
					WotogIconsOpponent.ForceShow(true);
				}

				PlayerActiveEffects.ForceShowExampleEffects(true);
				OpponentActiveEffects.ForceShowExampleEffects(false);

				PlayerCounters.ForceShowExampleCounters();
				OpponentCounters.ForceShowExampleCounters();

				// small delay to allow for elements to be added to overlay
				await Task.Delay(50);

				foreach (var movableElement in _movableElements)
				{
					try
					{
						if (!CanvasInfo.Children.Contains(movableElement.Value))
							CanvasInfo.Children.Add(movableElement.Value);

						movableElement.Value.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#4C0000FF");

						if(movableElement.Key == OpponentActiveEffects)
						{
							Canvas.SetTop(movableElement.Value, Height - (OpponentActiveEffects.MaxHeight * _activeEffectsScale + Height * Config.Instance.OpponentActiveEffectsVertical / 100));
							Canvas.SetLeft(movableElement.Value, Canvas.GetLeft(movableElement.Key));
						}
						else if(movableElement.Key == OpponentCounters)
						{
							Canvas.SetTop(movableElement.Value, Canvas.GetTop(movableElement.Key) - OpponentCounters.ActualHeight * _activeEffectsScale);
							Canvas.SetLeft(movableElement.Value, Canvas.GetLeft(movableElement.Key));
						}
						else
						{
							Canvas.SetTop(movableElement.Value, Canvas.GetTop(movableElement.Key));
							Canvas.SetLeft(movableElement.Value, Canvas.GetLeft(movableElement.Key));
						}

						var elementSize = GetUiElementSize(movableElement.Key);
						if (movableElement.Key == BorderStackPanelPlayer)
						{
							if (!TrySetResizeGripHeight(movableElement.Value, Config.Instance.PlayerDeckHeight * Height / 100))
							{
								Config.Instance.Reset("PlayerDeckHeight");
								TrySetResizeGripHeight(movableElement.Value, Config.Instance.PlayerDeckHeight * Height / 100);
							}
							movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width * Config.Instance.OverlayPlayerScaling/100 : 0;
						}
						else if (movableElement.Key == BorderStackPanelOpponent)
						{
							if (!TrySetResizeGripHeight(movableElement.Value, Config.Instance.OpponentDeckHeight * Height / 100))
							{
								Config.Instance.Reset("OpponentDeckHeight");
								TrySetResizeGripHeight(movableElement.Value, Config.Instance.OpponentDeckHeight * Height / 100);
							}
							movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width * Config.Instance.OverlayOpponentScaling / 100 : 0;
						}
						else if(movableElement.Key == StackPanelSecrets)
						{
							movableElement.Value.Height = StackPanelSecrets.ActualHeight > 0 ? StackPanelSecrets.ActualHeight : 0;
							movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width : 0;
						}
						else if(movableElement.Key == BattlegroundsSessionStackPanel)
						{
							movableElement.Value.Height = elementSize.Height > 0 ? elementSize.Height * Config.Instance.OverlaySessionRecapScaling / 100 : 0;
							movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width * Config.Instance.OverlaySessionRecapScaling / 100 : 0;
						}
						else
						{
							movableElement.Value.Height = elementSize.Height > 0 ? elementSize.Height : 0;
							movableElement.Value.Width = elementSize.Width > 0 ? elementSize.Width : 0;
						}

						var shouldBeVisible = battlegroundsMode
							? movableElement.Key == BattlegroundsSessionStackPanel
							: movableElement.Key != BattlegroundsSessionStackPanel;
						movableElement.Value.Visibility = shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
					}
					catch (Exception ex)
					{
						Log.Info(ex.ToString());
					}
				}
			}
			else
			{
				if(battlegroundsMode)
				{
					Config.Instance.HideDecksInOverlay = false;
					Config.Instance.HidePlayerFatigueCount = false;
					Config.Instance.HideOpponentFatigueCount = false;
				}
				UnHookMouse();
				if (_secretsTempVisible)
					HideSecrets();
				if (_game.IsInMenu)
					HideTimers();

				WotogIconsPlayer.ForceShow(false);
				WotogIconsOpponent.ForceShow(false);

				PlayerActiveEffects.ForceHideExampleEffects();
				OpponentActiveEffects.ForceHideExampleEffects();

				PlayerCounters.ForceHideExampleCounters();
				OpponentCounters.ForceHideExampleCounters();

				foreach(var movableElement in _movableElements)
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
			if (element is Border border)
				return new Size(border.ActualWidth, border.ActualHeight);
			if(element is Panel panel)
				return new Size(panel.ActualWidth, panel.ActualHeight);
			if (element is HearthstoneTextBlock block)
				return new Size(block.ActualWidth, block.ActualHeight);
			if(element is WotogCounter wotogIcons)
				return new Size(wotogIcons.IconWidth * _wotogSize, wotogIcons.ActualHeight * _wotogSize);
			if(element is ActiveEffectsOverlay activeEffects)
			{
				var width = activeEffects.MaxWidth * _activeEffectsScale;
				var height =  activeEffects.MaxHeight * _activeEffectsScale;

				return new Size(width, height);
			}
			if(element is CountersOverlay counters)
			{
				var width = counters.ActualWidth * _activeEffectsScale;
				var height = counters.ActualHeight * _activeEffectsScale;

				return new Size(width, height);
			}
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

		private async void HideCardsWhenFriendsListOpen(Point clickPos)
		{
			var panels = new List<Border>();
			if (Canvas.GetLeft(BorderStackPanelPlayer) - 200 < 500)
				panels.Add(BorderStackPanelPlayer);
			if (Canvas.GetLeft(BorderStackPanelOpponent) < 500)
				panels.Add(BorderStackPanelOpponent);

			_isFriendsListOpen = null;
			if(Config.Instance.HideDecksInOverlay)
				return;
			foreach (var panel in panels)
			{
				//if panel visible, only continue of click was in the button left corner
				if (!(clickPos.X < 150 && clickPos.Y > Height - 100) && panel.Visibility == Visibility.Visible)
					continue;

				var checkForFriendsList = true;
				if (panel.Equals(BorderStackPanelPlayer) && Config.Instance.HidePlayerCards)
					checkForFriendsList = false;
				else if (panel.Equals(BorderStackPanelOpponent) && Config.Instance.HideOpponentCards)
					checkForFriendsList = false;
				if(!checkForFriendsList)
					continue;
				if(_isFriendsListOpen == null)
				{
					await Task.Delay(500);
					_isFriendsListOpen = Reflection.Client.IsFriendsListVisible();
				}
				if (_isFriendsListOpen.Value)
				{
					var childPanel = Helper.FindVisualChildren<StackPanel>(panel).FirstOrDefault();
					if(childPanel == null)
						continue;
					var panelHeight = Canvas.GetTop(panel) + childPanel.ActualHeight;
					if(childPanel.VerticalAlignment == VerticalAlignment.Center)
						panelHeight += panel.ActualHeight / 2 - childPanel.ActualHeight / 2;
					var needToHide = panelHeight > Height * 0.3;
					if(needToHide)
					{
						var isPlayerPanel = panel.Equals(BorderStackPanelPlayer);
						Log.Info("Friendslist is open! Hiding " + (isPlayerPanel ? "player" : "opponent") + " panel.");
						panel.Visibility = Visibility.Collapsed;
						if(isPlayerPanel)
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
						if (panel.Equals(BorderStackPanelPlayer))
							_playerCardsHidden = false;
						else
							_opponentCardsHidden = false;
					}
				}
			}
		}

	}
}
