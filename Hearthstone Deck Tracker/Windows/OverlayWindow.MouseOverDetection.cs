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
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.TrinketPicking;
using Hearthstone_Deck_Tracker.Utility.Extensions;

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
		const double LeftAdjust = .0017;
		//Adjusts the OpponentDeadFor textblock of the next oponent by this to the right so it aligns correctly with the hero portrait.
		const double NextOpponentRightAdjust = .023;
		const double DuosNextOpponentRightAdjust = .015;
		private double LeaderboardTop => Height * 0.15;
		private const int MaxHandSize = 10;
		private const int MaxBoardSize = 7;
		public int? _leaderboardHoveredEntityId = null;
		private bool _mouseIsOverLeaderboardIcon = false;
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
		}

		internal void PositionDeadForText(int nextOpponentLeaderboardPosition)
		{
			if(Core.Game.IsBattlegroundsDuosMatch)
			{
				for(int i = 0; i < _leaderboardDeadForText.Count; i++)
				{
					int j = i / 2;
					var top = LeaderboardTop + BattlegroundsDuosTileHeight * i + BattlegroundsDuosSpacingHeight * j;
					var left = ((_leaderboardDeadForText.Count / 2) - i) * LeftAdjust;
					if(j == nextOpponentLeaderboardPosition - 1)
						left += DuosNextOpponentRightAdjust;
					Canvas.SetTop(_leaderboardDeadForText[i], top);
					Canvas.SetLeft(_leaderboardDeadForText[i], Helper.GetScaledXPos(left, (int)Width, ScreenRatio));
					Canvas.SetTop(_leaderboardDeadForTurnText[i], top);
					Canvas.SetLeft(_leaderboardDeadForTurnText[i], Helper.GetScaledXPos(left, (int)Width, ScreenRatio));
				}
			}
			else
			{
				for(int i = 0; i < _leaderboardDeadForText.Count; i++)
				{
					var top = LeaderboardTop + BattlegroundsTileHeight * i;
					var left = ((_leaderboardDeadForText.Count / 2) - i) * LeftAdjust;
					if(i == nextOpponentLeaderboardPosition - 1)
						left += NextOpponentRightAdjust;
					Canvas.SetTop(_leaderboardDeadForText[i], top);
					Canvas.SetLeft(_leaderboardDeadForText[i], Helper.GetScaledXPos(left, (int)Width, ScreenRatio));
					Canvas.SetTop(_leaderboardDeadForTurnText[i], top);
					Canvas.SetLeft(_leaderboardDeadForTurnText[i], Helper.GetScaledXPos(left, (int)Width, ScreenRatio));
				}
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

		private Point? GetCursorPos()
		{
			if(!IsVisible)
				return null;
			try
			{
				var pos = User32.GetMousePos();
				return CanvasInfo.PointFromScreen(new Point(pos.X, pos.Y));
			}
			catch(InvalidOperationException)
			{
				return null;
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
				_game.Metrics.IncrementMercenariesHoversOpponentMercToShowAbility();
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
			if(relativeCanvas == null)
				return;

			var opponentHoverTargets = GetBoardHoverTargets(OppBoardItemsControl);
			var playerHoverTargets = GetBoardHoverTargets(PlayerBoardItemsControl);
			for(var i = 0; i < 7; i++)
			{
				if(oppBoard.Count > i)
				{
					var ellipse = opponentHoverTargets.ElementAtOrDefault(i);
					if(ellipse != null && EllipseContains(ellipse, (Point)relativeCanvas))
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
					if(ellipse != null && EllipseContains(ellipse, (Point)relativeCanvas))
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
				if(RotatedRectContains(_playerHand[i], (Point)relativeCanvas))
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

		public void SetHoveredBattlegroundsLeaderboardEntityId(int? entityId)
		{
			_leaderboardHoveredEntityId = entityId;
		}

		private async void ShowBobsBuddyPanelDelayed()
		{
			await Task.Delay(300);
			if(_leaderboardHoveredEntityId == null &&
				_game.IsBattlegroundsMatch &&
				_game.GetTurnNumber() != 0 &&
				!_game.IsInMenu)
			{
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

		private bool _runInteractivityUpdates;
		private async void StartInteractivityUpdates()
		{
			if(_runInteractivityUpdates)
				return;
			const int sixtyHz = 1000 / 60;
			_runInteractivityUpdates = true;
			while(_runInteractivityUpdates)
			{
				UpdateHoverable();
				await Task.Delay(sixtyHz);
			}
		}

		private void StopInteractivityUpdates()
		{
			_runInteractivityUpdates = false;
		}

		/// <summary>
		/// Wrapper for MouseEventArgs, used to check whether an event was triggered by custom hover logic.
		/// </summary>
		public class CustomMouseEventArgs : MouseEventArgs
		{
			public CustomMouseEventArgs(MouseDevice mouse, int timestamp) : base(mouse, timestamp) { }
		}

		private HashSet<FrameworkElement> _mouseOverElements = new();


		private Dictionary<FrameworkElement, Vector> _scaleCache = new();
		private void UpdateHoverable()
		{
			var cursorPos = GetCursorPos();
			if(cursorPos == null)
				return;

			_scaleCache.Clear();

			var clickableMouseOver = _clickableElements.Where(e => ElementContains(e, (Point)cursorPos, _scaleCache)).ToList();
			SetClickthrough(clickableMouseOver.Count == 0);

			var hoverableMouseOver = _hoverableElements.Where(x => x.IsVisible && ElementContains(x, (Point)cursorPos, _scaleCache)).ToList();

			// for every previously mouse overed element, if it is no longer hovered, emit a MouseLeaveEvent
			foreach(var previousMouseOverElement in _mouseOverElements)
			{
				if(!hoverableMouseOver.Contains(previousMouseOverElement))
				{
					previousMouseOverElement?.RaiseEvent(new CustomMouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
				}
			}

			// We want to support emitting hover events for multiple elements if they are nested/related,
			// meaning, if the share the same "root" element in CanvasInfo, but not otherwise.
			// This prevents events from firing on elements that are "below" others. This is really only a problem
			// because we don't have proper event propagation here. Might be worth investing in in the future.
			if(hoverableMouseOver.Count > 0)
			{
				var rootDict = new Dictionary<DependencyObject, OverlayElement>();
				foreach(var element in hoverableMouseOver)
				{
					var root = GetCanvasInfoParentRoot(element);
					if(root == null)
						continue;
					if(!rootDict.TryGetValue(root, out var value))
						value = rootDict[root] = new OverlayElement();
					value.Hoverables.Add(element);
				}

				// Check all clickable elements as well. If the "top most" hovered element is clickable we don't
				// want to emit any custom hover event.
				foreach(var element in clickableMouseOver)
				{
					var root = GetCanvasInfoParentRoot(element);
					if(root == null)
						continue;
					if(!rootDict.TryGetValue(root, out var value))
						value = rootDict[root] = new OverlayElement();
					value.Clickables.Add(element);
				}

				if(rootDict.Count > 0)
				{
					// Since we only want to emit hovers for the "top most" element, we need to index and sort them.
					var rootChildrenCount = VisualTreeHelper.GetChildrenCount(CanvasInfo);
					for(var i = 0; i < rootChildrenCount; i++)
					{
						var child = VisualTreeHelper.GetChild(CanvasInfo, i);
						if(rootDict.TryGetValue(child, out var value))
							value.Index = i;
					}

					var newMouseOverElements = rootDict.Values.OrderByDescending(x => x.Index).First().Hoverables;
					foreach(var mouseOverElement in newMouseOverElements)
					{
						if(!_mouseOverElements.Contains(mouseOverElement))
						{
							mouseOverElement?.RaiseEvent(new CustomMouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
						}
					}

					// remember all elements that are currently hovered
					_mouseOverElements = new HashSet<FrameworkElement>(newMouseOverElements);
				}
				else
					_mouseOverElements.Clear();
			}
			else
				_mouseOverElements.Clear();

			return;

			DependencyObject? GetCanvasInfoParentRoot(FrameworkElement e)
			{
				DependencyObject element = e;
				var parent = VisualTreeHelper.GetParent(e);
				while(parent != CanvasInfo && parent != null)
				{
					element = parent;
					parent = VisualTreeHelper.GetParent(parent);
				}

				if(parent != null)
					return element;
				return null;
			}
		}

		private record OverlayElement()
		{
			public List<FrameworkElement> Hoverables { get; } = new();
			public List<FrameworkElement> Clickables { get; } = new();
			public int Index { get; set; } = -1;
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

		public bool ElementContains(FrameworkElement element, Point location, Dictionary<FrameworkElement, Vector>? scaleCache = null)
		{
			if(!element.IsVisible)
				return false;
			var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
			if(parent == null)
				return false;

			var scale = scaleCache == null ? Helper.GetTotalScaleTransform(element)
				: Helper.GetTotalScaleTransform(element, scaleCache);

			try
			{
				var point = element.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
				var contains= location.X > point.X && location.X < point.X + element.ActualWidth * scale.X && location.Y > point.Y
					   && location.Y < point.Y + element.ActualHeight * scale.Y;
				return contains;
			}
			catch(InvalidOperationException)
			{
				return false;
			}
		}
	}
}
