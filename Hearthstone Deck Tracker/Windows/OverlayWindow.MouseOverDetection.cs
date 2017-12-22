#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Point = System.Drawing.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private Entity _currentMouseOverTarget;
		public double BoardWidth => Width;
		public double BoardHeight => Height * 0.158;
		public double MinionWidth => Width * 0.63 / 7 * ScreenRatio;
		public double CardWidth => Height * 0.125;
		public double CardHeight => Height * 0.189;
		private const int MaxHandSize = 10;
		private const int MaxBoardSize = 7;
		private System.Windows.Point CenterOfHand => new System.Windows.Point((float)Width * 0.5 - Height * 0.035, (float)Height * 0.95);

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
		}

		private bool IsGameOver => _game.IsInMenu || _game.GameEntity == null || _game.GameEntity.GetTag(GameTag.STATE) == (int)State.COMPLETE;

		private double GetCardAngle(int playerHandCount, System.Windows.Point pos, int i)
		{
			var extraRotation = playerHandCount == 7 ? 0 : playerHandCount > 4 ? ((playerHandCount) % 2) : 1;
			var direction = pos.X > CenterOfHand.X ? -1 + (extraRotation * 0.3 * (playerHandCount - i) * Math.Max(1, i - 7)) : 1;
			return (CenterOfHand.Y - pos.Y) / Height * 600 * direction * (1 + Math.Sqrt(10.0 / (i + 1)) * 0.08);
		}

		private void AddCardDebugOverlay(Rectangle cardRect, System.Windows.Point pos)
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
			var pos = entityEllipse.TransformToAncestor(CanvasInfo).Transform(new System.Windows.Point(0, 0));
			Canvas.SetTop(lbl, pos.Y + 10);
			Canvas.SetLeft(lbl, pos.X + 10);
		}

		private async void DelayedMouseOverDetection(Entity entity, Action action)
		{
			var mousePos = User32.GetMousePos();
			await Task.Delay(Config.Instance.OverlayMouseOverTriggerDelay);
			if(Distance(User32.GetMousePos(), mousePos) > 3)
			{
				FlavorTextVisibility = Visibility.Collapsed;
				_currentMouseOverTarget = null;
				return;
			}
			if(_currentMouseOverTarget != entity)
				return;
			action?.Invoke();
		}

		private void DetectMouseOver(List<Entity> playerBoard, List<Entity> oppBoard)
		{
			if(playerBoard.Count == 0 && oppBoard.Count == 0 && _game.Player.HandCount == 0 || IsGameOver)
			{
				FlavorTextVisibility = Visibility.Collapsed;
				return;
			}
			var pos = User32.GetMousePos();
			System.Windows.Point relativeCanvas;
			try
			{
				relativeCanvas = CanvasInfo.PointFromScreen(new System.Windows.Point(pos.X, pos.Y));
			}
			catch(InvalidOperationException)
			{
				return;
			}
			for(var i = 0; i < 7; i++)
			{
				if(oppBoard.Count > i && EllipseContains(_oppBoard[i], relativeCanvas))
				{
					var entity = oppBoard[i];
					if(_currentMouseOverTarget == entity)
						return;
					_currentMouseOverTarget = entity;
					DelayedMouseOverDetection(entity, () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnOpponentMinionMouseOver.Execute(entity.Card);
					});
					return;
				}
				if(playerBoard.Count > i && EllipseContains(_playerBoard[i], relativeCanvas))
				{
					var entity = playerBoard[i];
					if(_currentMouseOverTarget == entity)
						return;
					_currentMouseOverTarget = entity;
					DelayedMouseOverDetection(entity, () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnPlayerMinionMouseOver.Execute(entity.Card);
					});
					return;
				}
			}
			var handCount = Math.Min(_game.Player.HandCount, MaxHandSize);
			for(var i = handCount - 1; i >= 0; i--)
			{
				if(RotatedRectContains(_playerHand[i], relativeCanvas))
				{
					var entity = Core.Game.Player.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == i+1);
					if(entity == null || _currentMouseOverTarget == entity)
						return;
					_currentMouseOverTarget = entity;
					DelayedMouseOverDetection(entity, () =>
					{
						SetFlavorTextEntity(entity);
						GameEvents.OnPlayerHandMouseOver.Execute(entity.Card);
					});
					return;
				}
			}
			if(_currentMouseOverTarget != null)
				GameEvents.OnMouseOverOff.Execute();
			_currentMouseOverTarget = null;
			FlavorTextVisibility = Visibility.Collapsed;
		}

		public System.Windows.Point GetPlayerCardPosition(int position, int count)
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
			return new System.Windows.Point(x, y * CenterOfHand.Y);
		}

		private float GetCardSpacing(int count)
		{
			var cardWidth = (float)Height / 10 * 1.27f;
			var maxHandWidth = (float)Width * (float)ScreenRatio * 0.36f;
			if (count * cardWidth > maxHandWidth)
				return maxHandWidth / count;
			return cardWidth;
		}

		private double Distance(Point p1, Point p2) => Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);

		public bool EllipseContains(Ellipse ellipse, System.Windows.Point location)
		{
			var pos = ellipse.TransformToAncestor(CanvasInfo).Transform(new System.Windows.Point(0, 0));
			var center = new System.Windows.Point(pos.X + ellipse.Width / 2, pos.Y + ellipse.Height / 2);
			var radiusX = ellipse.Width / 2;
			var radiusY = ellipse.Height / 2;
			if (radiusX <= 0.0 || radiusY <= 0.0)
				return false;
			var normalized = new System.Windows.Point(location.X - center.X, location.Y - center.Y);
			return ((normalized.X * normalized.X) / (radiusX * radiusX)) + ((normalized.Y * normalized.Y) / (radiusY * radiusY)) <= 1.0;
		}

		public bool RotatedRectContains(Rectangle rect, System.Windows.Point location)
		{
			var rectCorner = new System.Windows.Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));
			var rectRotation = rect.RenderTransform as RotateTransform;
			if(rectRotation == null)
				return false;
			var transform = new RotateTransform(-rectRotation.Angle, rectCorner.X + rectRotation.CenterX, rectCorner.Y + rectRotation.CenterY);
			var rotated = transform.Transform(location);
			return rotated.X > rectCorner.X && rotated.X < rectCorner.X + rect.Width && rotated.Y > rectCorner.Y
				   && rotated.Y < rectCorner.Y + rect.Height;
		}
	}
}
