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
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Hearthstone;

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
		private const int MaxHandSize = 10;
		private const int MaxBoardSize = 7;
		private Point CenterOfHand => new Point((float)Width * 0.5 - Height * 0.035, (float)Height * 0.95);

		public Thickness MinionMargin
		{
			get
			{
				var side = Width * ScreenRatio * 0.0029;
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
				_oppBoard[i].Visibility = oppBoard.Count > i && !isGameOver ? Visibility.Visible : Visibility.Collapsed;
				_playerBoard[i].Visibility = playerBoard.Count > i && !isGameOver ? Visibility.Visible : Visibility.Collapsed;
				if(Config.Instance.Debug && !_game.IsInMenu)
				{
					if(i < oppBoard.Count)
						AddMinionDebugOverlay(oppBoard[i], _oppBoard[i]);
					if(i < playerBoard.Count)
						AddMinionDebugOverlay(playerBoard[i], _playerBoard[i]);
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

			var leaderboardTop = Height * 0.15;
			for(var i = 0; i < _leaderboardIcons.Count; i++)
			{
				_leaderboardIcons[i].Visibility = isGameOver || _game.CurrentGameType != GameType.GT_BATTLEGROUNDS
					? Visibility.Collapsed
					: Visibility.Visible;
				Canvas.SetTop(_leaderboardIcons[i], leaderboardTop + BattlegroundsTileHeight * i);
				Canvas.SetLeft(_leaderboardIcons[i], Helper.GetScaledXPos(0.001 * (_leaderboardIcons.Count - i - 1), (int)Width, ScreenRatio));
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

		private void AddMinionDebugOverlay(Entity entity, Ellipse entityEllipse)
		{
			entityEllipse.Stroke = new SolidColorBrush(Colors.Red);
			entityEllipse.StrokeThickness = 1;
			var lbl = new Label {Content = entity.Card.Name, Foreground = Brushes.White};
			_debugBoardObjects.Add(lbl);
			CanvasInfo.Children.Add(lbl);
			var pos = entityEllipse.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
			Canvas.SetTop(lbl, pos.Y + 10);
			Canvas.SetLeft(lbl, pos.X + 10);
		}

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

		private void DetectMouseOver(List<Entity> playerBoard, List<Entity> oppBoard)
		{
			if(playerBoard.Count == 0 && oppBoard.Count == 0 && _game.Player.HandCount == 0 || IsGameOver)
			{
				FlavorTextVisibility = Visibility.Collapsed;
				return;
			}
			var relativeCanvas = GetCursorPos();
			if(relativeCanvas.X == -1 && relativeCanvas.Y == -1)
				return;
			for(var i = 0; i < 7; i++)
			{
				if(oppBoard.Count > i && EllipseContains(_oppBoard[i], relativeCanvas))
				{
					var entity = oppBoard[i];
					_entityMouseOver.DelayedMouseOverDetection(oppBoard[i], () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnOpponentMinionMouseOver.Execute(entity.Card);
					}, () => FlavorTextVisibility = Visibility.Collapsed);
					return;
				}
				if(playerBoard.Count > i && EllipseContains(_playerBoard[i], relativeCanvas))
				{
					var entity = playerBoard[i];
					_entityMouseOver.DelayedMouseOverDetection(entity, () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnPlayerMinionMouseOver.Execute(entity.Card);
					}, () => FlavorTextVisibility = Visibility.Collapsed);
					return;
				}
			}
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
			var showMinions = false;
			var fadeBgsMinionsList = false;
			for(var i = 0; i < _leaderboardIcons.Count; i++)
			{
				if(ElementContains(_leaderboardIcons[i], cursorPos))
				{
					fadeBgsMinionsList = true;
					var entity = _game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) == i + 1).FirstOrDefault();
					if(entity == null)
						break;
					if(!_game.LastKnownBattlegroundsBoardState.TryGetValue(entity.CardId, out var state))
						break;
					BattlegroundsBoard.Children.Clear();
					foreach(var e in state.Entities)
						BattlegroundsBoard.Children.Add(new EntityControl(e));
					var age = _game.GetTurnNumber() - state.Turn;
					BattlegroundsAge.Text = string.Format(LocUtil.Get("Overlay_Battlegrounds_Turns"), age);
					BattlegroundsOpponent.Text = entity.Card.LocalizedName;
					showMinions = true;
					break;
				}
			}
			if(showMinions)
			{
				Canvas.SetTop(BattlegroundsLeaderboard, Height * 0.01);
				Canvas.SetLeft(BattlegroundsLeaderboard, Helper.GetScaledXPos(0.05, (int)Width, ScreenRatio));
				BattlegroundsLeaderboard.Visibility = Visibility.Visible;
				var scale = Math.Min(1.5, Height / 1080);
				BattlegroundsLeaderboard.RenderTransform = new ScaleTransform(scale, scale, 0, 0);
			}
			else
			{
				BattlegroundsBoard.Children.Clear();
				BattlegroundsLeaderboard.Visibility = Visibility.Collapsed;
			}
			// Only fade the minions, if we're out of mulligan
			if(_game.GameEntity?.GetTag(GameTag.STEP) <= (int)Step.BEGIN_MULLIGAN)
				fadeBgsMinionsList = false;
			BgsTopBar.Opacity = fadeBgsMinionsList ? 0.3 : 1;
			BobsBuddyDisplay.Opacity = fadeBgsMinionsList ? 0.3 : 1;
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

		private void UpdateClickableElements()
		{
			var cursorPos = GetCursorPos();
			if(cursorPos.X == -1 && cursorPos.Y == -1)
				return;
			var hoveredIndex = _clickableElements.FindIndex(e => ElementContains(e, cursorPos, AutoScaling));
			SetClickthrough(hoveredIndex < 0);
		}

		public bool EllipseContains(Ellipse ellipse, Point location)
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

		public bool ElementContains(FrameworkElement element, Point location, double scaling = 1)
		{
			if(!element.IsVisible)
				return false;
			var point = element.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
			var contains= location.X > point.X && location.X < point.X + element.ActualWidth * scaling && location.Y > point.Y
				   && location.Y < point.Y + element.ActualHeight * scaling;
			return contains;
		}
	}
}
