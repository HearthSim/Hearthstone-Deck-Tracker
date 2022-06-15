#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Rectangle = System.Windows.Shapes.Rectangle;
using Hearthstone_Deck_Tracker.Utility;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private DelayedMouseOver _entityMouseOver = new DelayedMouseOver(Config.Instance.OverlayMouseOverTriggerDelay);
		private DelayedMouseOver _battlegroundsTiersMouseOver = new DelayedMouseOver(30, 5);
		public double BoardWidth => Width;
		public double BoardHeight => Height * 0.158;
		public double MinionWidth => Width * 0.63 / 7 * ScreenRatio;
		public double CardWidth => Height * 0.125;
		public double CardHeight => Height * 0.189;
		public double MercAbilityHeight => Height * 0.3;
		//Adjusts OpponentDeadFor textblocks left by this amount depending on what position they represent on the leaderboard.
		const double LeftAdjust = .00075;
		//Adjusts the OpponentDeadFor textblock of the next oponent by this to the right so it aligns correctly with the hero portrait.
		const double NextOpponentRightAdjust = .025;
		private double LeaderboardTop => Height * 0.15;
		private const int MaxHandSize = 10;
		private const int MaxBoardSize = 7;
		private bool _mouseIsOverLeaderboardIcon = false;
		private int _nextOpponentLeaderboardPosition = -1;
		private const int MouseLeaveEventDelay = 200;

		private Point CenterOfHand => new Point((float)Width * 0.5 - Height * 0.035, (float)Height * 0.95);

		public Thickness MinionMargin
		{
			get
			{
				var side = Width * ScreenRatio * (_game.IsMercenariesMatch ? 0.01 : 0.0029);
				return new Thickness(side, 0, side, 0);
			}
		}

		private void UpdateMouseOverDetectionRegions(List<Entity> oppBoard, List<Entity> playerBoard)
		{
			if(Config.Instance.Debug)
			{
				foreach(var lbl in _debugBoardObjects)
					CanvasInfo.Children.Remove(lbl);
				_debugBoardObjects.Clear();
			}
			var isGameOver = IsGameOver;
			for(var i = 0; i < MaxBoardSize; i++)
			{
				OppBoard[i].Visibility = oppBoard.Count > i && !isGameOver ? Visibility.Visible : Visibility.Collapsed;
				PlayerBoard[i].Visibility = playerBoard.Count > i && !isGameOver ? Visibility.Visible : Visibility.Collapsed;
				if(Config.Instance.Debug && !_game.IsInMenu)
				{
					//if(i < oppBoard.Count)
					//AddMinionDebugOverlay(oppBoard[i], _oppBoard[i]);
					//if(i < playerBoard.Count)
					//AddMinionDebugOverlay(playerBoard[i], _playerBoard[i]);
				}
			}
			var playerHandCount = _game.Player.HandCount;
			for(var i = 0; i < MaxHandSize; i++)
			{
				if(isGameOver)
				{
					_playerHand[i].Visibility = Visibility.Collapsed;
					continue;
				}
				if(i < playerHandCount)
				{
					var pos = GetPlayerCardPosition(i, playerHandCount);
					var angle = GetCardAngle(playerHandCount, pos, i);
					_playerHand[i].RenderTransform = new RotateTransform(angle, _playerHand[i].Width / 2, _playerHand[i].Height / 2);
					Canvas.SetTop(_playerHand[i], pos.Y - _playerHand[i].Height / 2);
					Canvas.SetLeft(_playerHand[i], pos.X - _playerHand[i].Width / 2);
				}
				_playerHand[i].Visibility = playerHandCount > i ? Visibility.Visible : Visibility.Collapsed;
				if(Config.Instance.Debug)
					AddCardDebugOverlay(_playerHand[i], GetPlayerCardPosition(i, playerHandCount));
			}

			for(var i = 0; i < _leaderboardIcons.Count; i++)
			{
				_leaderboardIcons[i].Visibility = isGameOver || !_game.IsBattlegroundsMatch
					? Visibility.Collapsed
					: Visibility.Visible;
				Canvas.SetTop(_leaderboardIcons[i], LeaderboardTop + BattlegroundsTileHeight * i);
				Canvas.SetLeft(_leaderboardIcons[i], Helper.GetScaledXPos(0.001 * (_leaderboardIcons.Count - i - 1), (int)Width, ScreenRatio));
			}

			PositionDeadForText();
		}

		internal void ResetNextOpponentLeaderboardPosition() => _nextOpponentLeaderboardPosition = -1;

		internal void PositionDeadForText(int nextOpponentLeaderboardPosition = 0)
		{
			if(nextOpponentLeaderboardPosition > 0)
				_nextOpponentLeaderboardPosition = nextOpponentLeaderboardPosition;

			for(int i = 0; i < _leaderboardDeadForText.Count; i++)
			{
				Canvas.SetTop(_leaderboardDeadForText[i], LeaderboardTop + BattlegroundsTileHeight * i);
				Canvas.SetLeft(_leaderboardDeadForText[i], Helper.GetScaledXPos(LeftAdjust * (_leaderboardDeadForText.Count - i - 1), (int)Width, ScreenRatio));
				Canvas.SetTop(_leaderboardDeadForTurnText[i], LeaderboardTop + BattlegroundsTileHeight * i);
				Canvas.SetLeft(_leaderboardDeadForTurnText[i], Helper.GetScaledXPos(LeftAdjust * (_leaderboardDeadForTurnText.Count - i - 1), (int)Width, ScreenRatio));
			}

			if(_nextOpponentLeaderboardPosition > 0)
			{
				Canvas.SetLeft(_leaderboardDeadForText[_nextOpponentLeaderboardPosition - 1], Helper.GetScaledXPos(LeftAdjust * (_leaderboardDeadForText.Count - _nextOpponentLeaderboardPosition - 2) + NextOpponentRightAdjust, (int)Width, ScreenRatio));
				Canvas.SetLeft(_leaderboardDeadForTurnText[_nextOpponentLeaderboardPosition - 1], Helper.GetScaledXPos(LeftAdjust * (_leaderboardDeadForTurnText.Count - _nextOpponentLeaderboardPosition - 2) + NextOpponentRightAdjust, (int)Width, ScreenRatio));
			}
		}

		private bool IsGameOver => _game.IsInMenu || _game.GameEntity == null || _game.GameEntity.GetTag(GameTag.STATE) == (int)State.COMPLETE;

		private double GetCardAngle(int playerHandCount, Point pos, int i)
		{
			var extraRotation = playerHandCount == 7 ? 0 : playerHandCount > 4 ? ((playerHandCount) % 2) : 1;
			var direction = pos.X > CenterOfHand.X ? -1 + (extraRotation * 0.3 * (playerHandCount - i) * Math.Max(1, i - 7)) : 1;
			return (CenterOfHand.Y - pos.Y) / Height * 600 * direction * (1 + Math.Sqrt(10.0 / (i + 1)) * 0.08);
		}

		private void AddCardDebugOverlay(Rectangle cardRect, Point pos)
		{
			cardRect.Stroke = Brushes.Red;
			cardRect.StrokeThickness = 1;
			cardRect.Fill = new SolidColorBrush(Color.FromArgb(90, 255, 0, 0));
			var e = new Ellipse { Width = 5, Height = 5, Fill = Brushes.Red };
			Canvas.SetTop(e, pos.Y);
			Canvas.SetLeft(e, pos.X);
			CanvasInfo.Children.Add(e);
			_debugBoardObjects.Add(e);
		}

		/*
		private void AddMinionDebugOverlay(Entity entity, Ellipse entityEllipse)
		{
			entityEllipse.Stroke = new SolidColorBrush(Colors.Red);
			entityEllipse.StrokeThickness = 1;
			var lbl = new Label { Content = entity.Card.Name, Foreground = Brushes.White };
			_debugBoardObjects.Add(lbl);
			CanvasInfo.Children.Add(lbl);
			var pos = entityEllipse.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
			Canvas.SetTop(lbl, pos.Y + 10);
			Canvas.SetLeft(lbl, pos.X + 10);
		}
		*/

		private Point GetCursorPos()
		{
			try
			{
				var pos = User32.GetMousePos();
				return CanvasInfo.PointFromScreen(new Point(pos.X, pos.Y));
			}
			catch(InvalidOperationException)
			{
				return new Point(-1, -1);
			}
		}

		private void ShowMercHover(Entity entity, Player player)
		{
			if(player.IsLocalPlayer)
			{
				if(!Config.Instance.ShowMercsPlayerHover)
					return;
			}
			else
			{
				if(!Config.Instance.ShowMercsOpponentHover)
					return;
			}
			var id = entity?.Card?.Id;
			if(!string.IsNullOrEmpty(id))
			{
				var data = GetMercAbilities(player);
				var abilities = data.ElementAtOrDefault(entity!.ZonePosition - 1);

				var elements = new[] { MercAbility1, MercAbility2, MercAbility3 };
				var max = Math.Min(elements.Length, abilities?.Count ?? 3);
				for(var i = 0; i < max; i++)
				{
					var abilityData = abilities?.ElementAtOrDefault(i);
					var card = abilityData?.Entity?.Card ?? abilityData?.Card;
					if(card != null)
					{
						elements[i].SetCardIdFromCard(card);
						elements[i].ShowQuestionmark = abilityData!.Entity == null && abilityData.HasTiers;
					}
				}
			}
			else
				ClearMercHover();
		}

		private void ClearMercHover()
		{
			MercAbility1.SetCardIdFromCard(null);
			MercAbility2.SetCardIdFromCard(null);
			MercAbility3.SetCardIdFromCard(null);
		}

		private void UpdateAbilitesVisibility(int hoverIndex, int boardSize, List<BoardMinionOverlayViewModel> vms, bool excludeIndex)
		{
			var center = 0.5 * (boardSize + 1) - 1;
			for(var i = 0; i < MaxBoardSize; i++)
			{
				if(hoverIndex <= center)
				{
					// tooltip to right
					if(i <= hoverIndex)
						vms.ElementAt(i).AbilitiesVisibility = Visibility.Visible;
					else
						vms.ElementAt(i).AbilitiesVisibility = Visibility.Hidden;
				}
				else
				{
					// tooltip to left
					if(i >= hoverIndex)
						vms.ElementAt(i).AbilitiesVisibility = Visibility.Visible;
					else
						vms.ElementAt(i).AbilitiesVisibility = Visibility.Hidden;
				}
			}
			if(excludeIndex)
				vms.ElementAt(hoverIndex).AbilitiesVisibility = Visibility.Hidden;
		}

		private void ClearAbilitesVisibility()
		{
			foreach(var m in OppBoard)
				m.AbilitiesVisibility = Visibility.Visible;
			foreach(var m in PlayerBoard)
				m.AbilitiesVisibility = Visibility.Visible;
		}

		private void DetectMouseOver(List<Entity> playerBoard, List<Entity> oppBoard)
		{
			if(!_game.IsMercenariesMatch && (playerBoard.Count == 0 && oppBoard.Count == 0 && _game.Player.HandCount == 0 || IsGameOver))
			{
				FlavorTextVisibility = Visibility.Collapsed;
				return;
			}
			if(_game.IsMercenariesMatch && _entityMouseOver.HasCurrent && _game.GameEntity?.GetTag(GameTag.STEP) == (int)Step.MAIN_COMBAT)
			{
				_entityMouseOver.Clear();
				ClearMercHover();
				ClearAbilitesVisibility();
				return;
			}
			var relativeCanvas = GetCursorPos();
			if(relativeCanvas.X == -1 && relativeCanvas.Y == -1)
				return;

			var opponentHoverTargets = GetBoardHoverTargets(OppBoardItemsControl);
			var playerHoverTargets = GetBoardHoverTargets(PlayerBoardItemsControl);
			for(var i = 0; i < 7; i++)
			{
				if(oppBoard.Count > i)
				{
					var ellipse = opponentHoverTargets.ElementAtOrDefault(i);
					if(ellipse != null && EllipseContains(ellipse, relativeCanvas))
					{
						var entity = oppBoard[i];
						var index = i;
						_entityMouseOver.DelayedMouseOverDetection(oppBoard[i], () =>
						{
							if(_game.IsMercenariesMatch)
							{
								if(_game.GameEntity?.GetTag(GameTag.STEP) != (int)Step.MAIN_COMBAT)
									ShowMercHover(entity, _game.Opponent);
								UpdateAbilitesVisibility(index, oppBoard.Count, OppBoard, false);
							}
							else
								SetFlavorTextEntity(entity);
							GameEvents.OnOpponentMinionMouseOver.Execute(entity.Card);
						}, () => {
							FlavorTextVisibility = Visibility.Collapsed;
							ClearMercHover();
							ClearAbilitesVisibility();
						}, delayOverride: _game.IsMercenariesMatch ? (int?)200 : null);
						return;
					}
				}

				if(playerBoard.Count > i)
				{
					var ellipse = playerHoverTargets.ElementAtOrDefault(i);
					if(ellipse != null && EllipseContains(ellipse, relativeCanvas))
					{
						var entity = playerBoard[i];
						var index = i;
						_entityMouseOver.DelayedMouseOverDetection(entity, () =>
						{
							if(_game.IsMercenariesMatch)
							{
								if(_game.GameEntity?.GetTag(GameTag.STEP) != (int)Step.MAIN_COMBAT)
									ShowMercHover(entity, _game.Player);
								var hideOnIndex = _game.GameEntity?.HasTag(GameTag.ALLOW_MOVE_MINION) ?? false;
								UpdateAbilitesVisibility(index, playerBoard.Count, PlayerBoard, hideOnIndex);
							}
							else
								SetFlavorTextEntity(entity);
							GameEvents.OnPlayerMinionMouseOver.Execute(entity.Card);
						}, () => {
							FlavorTextVisibility = Visibility.Collapsed;
							ClearMercHover();
							ClearAbilitesVisibility();
						}, delayOverride: _game.IsMercenariesMatch ? (int?)200 : null);
						return;
					}
				}
			}

			ClearMercHover();
			ClearAbilitesVisibility();

			var handCount = Math.Min(_game.Player.HandCount, MaxHandSize);
			for(var i = handCount - 1; i >= 0; i--)
			{
				if(RotatedRectContains(_playerHand[i], relativeCanvas))
				{
					var entity = Core.Game.Player.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == i+1);
					if(entity == null)
						return;
					_entityMouseOver.DelayedMouseOverDetection(entity, () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnPlayerHandMouseOver.Execute(entity.Card);
					}, () => FlavorTextVisibility = Visibility.Collapsed);
					return;
				}
			}
			if(_entityMouseOver.HasCurrent)
				GameEvents.OnMouseOverOff.Execute();
			_entityMouseOver.Clear();
			FlavorTextVisibility = Visibility.Collapsed;
		}

		private void UpdateBattlegroundsOverlay()
		{
			var cursorPos = GetCursorPos();
			if(cursorPos.X == -1 && cursorPos.Y == -1)
				return;
			var shouldShowOpponentInfo = false;
			var fadeBgsMinionsList = false;
			_mouseIsOverLeaderboardIcon = false;
			var turn = _game.GetTurnNumber();
			_leaderboardDeadForText.ForEach(x => x.Visibility = Visibility.Collapsed);
			_leaderboardDeadForTurnText.ForEach(x => x.Visibility = Visibility.Collapsed);
			if(turn == 0)
				return;
			for(var i = 0; i < _leaderboardIcons.Count; i++)
			{
				if(ElementContains(_leaderboardIcons[i], cursorPos))
				{
					_mouseIsOverLeaderboardIcon = true;
					fadeBgsMinionsList = true;
					_leaderboardDeadForText.ForEach(x => x.Visibility = Visibility.Visible);
					_leaderboardDeadForTurnText.ForEach(x => x.Visibility = Visibility.Visible);
					var entity = _game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) == i + 1).FirstOrDefault();
					if(entity == null)
					{
						if(turn == 1 && i != 0)
						{
							BgsOpponentInfo.ShowNotFoughtOpponent();
							shouldShowOpponentInfo = true;
						}
						break;
					}
					var state = _game.GetBattlegroundsBoardStateFor(entity.CardId);
					shouldShowOpponentInfo = !(state == null && entity.CardId == Core.Game.Player.Board.FirstOrDefault(x => x.IsHero)?.CardId);
					BgsOpponentInfo.Update(entity, state, turn);
					break;
				}
			}
			if(shouldShowOpponentInfo)
			{
				BgsOpponentInfo.Visibility = Visibility.Visible;
				BgsOpponentInfo.UpdateLayout();
				_bgsBobsBuddyBehavior.Hide();
				_bgsPastOpponentBoardBehavior.Show();
			}
			else
			{
				BgsOpponentInfo.Visibility = Visibility.Collapsed;
				_bgsPastOpponentBoardBehavior.Hide();
				BgsOpponentInfo.ClearLastKnownBoard();
				ShowBobsBuddyPanelDelayed();
			}
			// Only fade the minions, if we're out of mulligan
			if(_game.GameEntity?.GetTag(GameTag.STEP) <= (int)Step.BEGIN_MULLIGAN)
				fadeBgsMinionsList = false;
			BgsTopBar.Opacity = fadeBgsMinionsList ? 0.3 : 1;
			BobsBuddyDisplay.Opacity = fadeBgsMinionsList ? 0.3 : 1;
		}

		private async void ShowBobsBuddyPanelDelayed()
		{
			await Task.Delay(300);
			if(!_mouseIsOverLeaderboardIcon)
			{
				if(_game.IsBattlegroundsMatch && !_game.IsInMenu)
					ShowBobsBuddyPanel();
			}
		}

		public Point GetPlayerCardPosition(int position, int count)
		{
			var cardWidth = 0.0f;
			var center = 0.0f;
			var setAngle = 0;
			if (count > 3)
			{
				setAngle = 1;
				var width = 40f + count * 2;
				cardWidth = width / count;
				center = -width / 2;
			}
			var rightOfCenter = cardWidth * position + center;
			var rightYOffset = 0.0f;
			if (rightOfCenter > 0.0)
				rightYOffset = (float)(Math.Sin((float)(Math.Abs(rightOfCenter) * Math.PI / 180)) * GetCardSpacing(count) / 2.0);
			var x = (float)CenterOfHand.X - GetCardSpacing(count) / 2 * (count - 1 - position * 2);
			var y = 1f;
			if (count > 1)
				y = y + (float)Math.Pow(Math.Abs(position - count / 2), 2) / (4 * count) * 0.11f * setAngle + rightYOffset * 0.0009f;
			return new Point(x, y * CenterOfHand.Y);
		}

		private float GetCardSpacing(int count)
		{
			var cardWidth = (float)Height / 10 * 1.27f;
			var maxHandWidth = (float)Width * (float)ScreenRatio * 0.36f;
			if (count * cardWidth > maxHandWidth)
				return maxHandWidth / count;
			return cardWidth;
		}

		private FrameworkElement? _currentlyHoveredElement;
		private void UpdateInteractiveElements()
		{
			var cursorPos = GetCursorPos();
			if(cursorPos.X == -1 && cursorPos.Y == -1)
				return;

			var clickableHoveredIndex = _clickableElements.FindIndex(e => ElementContains(e, cursorPos));
			SetClickthrough(clickableHoveredIndex < 0);
		}

		const int SixtyHz = 1000 / 60;
		private bool _runningHoverableUpdates;
		private async void RunHoverUpdates()
		{
			if(_runningHoverableUpdates)
				return;
			_runningHoverableUpdates = true;
			Log.Info("Starting overlay hover updates...");
			while(_hoverableElements.Count > 0 && IsVisible)
			{
				UpdateHoverable();
				await Task.Delay(SixtyHz);
			}
			Log.Info("Stopping overlay hover updates");
			_runningHoverableUpdates = false;
		}


		/// <summary>
		/// Wrapper for MouseEventArgs, used to check whether an event was triggered by custom hover logic.
		/// </summary>
		public class CustomMouseEventArgs : MouseEventArgs
		{
			public CustomMouseEventArgs(MouseDevice mouse, int timestamp) : base(mouse, timestamp) { }
		}

		private void UpdateHoverable()
		{
			var cursorPos = GetCursorPos();
			if(cursorPos.X == -1 && cursorPos.Y == -1)
				return;
			var hoveredElement = _hoverableElements.FirstOrDefault(x => x.IsVisible && ElementContains(x, cursorPos));
			if(hoveredElement != _currentlyHoveredElement)
			{
				if(_currentlyHoveredElement != null)
				{
					if(hoveredElement != _currentlyHoveredElement)
					{
						_currentlyHoveredElement?.RaiseEvent(new CustomMouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
						_currentlyHoveredElement = null;
					}
				}
				if(hoveredElement != null)
				{
					hoveredElement?.RaiseEvent(new CustomMouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
					_currentlyHoveredElement = hoveredElement;
				}
			}
		}

		public bool EllipseContains(Ellipse ellipse, Point location)
		{
			try
			{
				var pos = ellipse.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
				var center = new Point(pos.X + ellipse.Width / 2, pos.Y + ellipse.Height / 2);
				var radiusX = ellipse.Width / 2;
				var radiusY = ellipse.Height / 2;
				if (radiusX <= 0.0 || radiusY <= 0.0)
					return false;
				var normalized = new Point(location.X - center.X, location.Y - center.Y);
				return ((normalized.X * normalized.X) / (radiusX * radiusX)) + ((normalized.Y * normalized.Y) / (radiusY * radiusY)) <= 1.0;
			}
			catch(InvalidOperationException)
			{
				return false;
			}
		}

		public bool RotatedRectContains(Rectangle rect, Point location)
		{
			var rectCorner = new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));
			var rectRotation = rect.RenderTransform as RotateTransform;
			if(rectRotation == null)
				return false;
			var transform = new RotateTransform(-rectRotation.Angle, rectCorner.X + rectRotation.CenterX, rectCorner.Y + rectRotation.CenterY);
			var rotated = transform.Transform(location);
			return rotated.X > rectCorner.X && rotated.X < rectCorner.X + rect.Width && rotated.Y > rectCorner.Y
				   && rotated.Y < rectCorner.Y + rect.Height;
		}

		public bool ElementContains(FrameworkElement element, Point location)
		{
			if(!element.IsVisible)
				return false;
			var scaleTransform = GetScaleTransform(element);
			var scaleX = scaleTransform?.ScaleX ?? 1;
			var scaleY = scaleTransform?.ScaleY ?? 1;
			try
			{
				var point = element.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
				var contains= location.X > point.X && location.X < point.X + element.ActualWidth * scaleX && location.Y > point.Y
					   && location.Y < point.Y + element.ActualHeight * scaleY;
				return contains;
			}
			catch(InvalidOperationException)
			{
				return false;
			}
		}

		private ScaleTransform? GetScaleTransform(FrameworkElement element)
		{
			// Only BgTierIcons are marked as clickable but a wrapper is scaled by the OverlayElementBehavior
			if(element == BattlegroundsMinionsPanel.BgTierIcons)
				return BgsTopBar.RenderTransform as ScaleTransform;
			if(element == BattlegroundsSession.BattlegroundsSessionPanelTopGroup || element is BattlegroundsGameView)
				return BattlegroundsSession.RenderTransform as ScaleTransform;

			return element.RenderTransform as ScaleTransform;
		}
	}
}
