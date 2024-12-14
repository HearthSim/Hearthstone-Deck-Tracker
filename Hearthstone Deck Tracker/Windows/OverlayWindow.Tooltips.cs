#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using HearthDb;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.ActiveEffects;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;
using NuGet;
using static System.Windows.Visibility;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private readonly Dictionary<FrameworkElement, FrameworkElement> _activeTooltips = new();
		private void SetTooltip(FrameworkElement? tooltip, FrameworkElement target)
		{
			if(_activeTooltips.TryGetValue(target, out var current))
			{
				_activeTooltips.Remove(target);
				OverlayTooltip.Children.Remove(current);
			}

			if(tooltip == null)
				return;

			// The content of OverlayExtensions.ToolTip is not part of the visual tree (OverlayExtensions.ToolTip is
			// a property, not a framework element), and does therefore not have a DataContext.For bindings to work
			// correctly we need to set it here to match the target.
			tooltip.DataContext = target.DataContext;

			_activeTooltips[target] = tooltip;
			OverlayTooltip.Children.Add(tooltip);


			// Normalize placement to a direction. We don't support the other, more advanced, placement methods.
			var placement = ToolTipService.GetPlacement(target) switch
			{
				PlacementMode.Top => PlacementMode.Top,
				PlacementMode.Bottom => PlacementMode.Bottom,
				PlacementMode.Left => PlacementMode.Left,
				_ => PlacementMode.Right,
			};

			var placementAwareTooltip = tooltip as IPlacementAware;
			// Set placement before calculating layout
			placementAwareTooltip?.SetPlacement(placement);

			// Force a layout update so that ActualWidth and ActualHeight are seg
			tooltip.UpdateLayout();

			var point = target.TransformToAncestor(this).Transform(new Point(0, 0));

			// Correct placement if tooltip would go outside of window
			switch (placement)
			{
				case PlacementMode.Top:
					if(point.Y - tooltip.ActualHeight < 0)
						placement = PlacementMode.Bottom;
					break;
				case PlacementMode.Bottom:
					if(point.Y + tooltip.ActualHeight > ActualHeight)
						placement = PlacementMode.Top;
					break;
				case PlacementMode.Left:
					if(point.X - tooltip.ActualWidth < 0)
						placement = PlacementMode.Right;
					break;
				case PlacementMode.Right:
					if(point.X + tooltip.ActualWidth > ActualWidth)
						placement = PlacementMode.Left;
					break;
			}

			// Update placement, since it may have swapped sides. We don't recalculate the size/placement again.
			// Hopefully the layout of the tooltip should not affect the size when swapping left/right.
			placementAwareTooltip?.SetPlacement(placement);

			(double x, double y) GetScaledSize(FrameworkElement? element)
			{
				if(element == null)
					return (0, 0);
				double width = element.ActualWidth, height = element.ActualHeight;
				while(element != null)
				{
					if(element.RenderTransform is ScaleTransform sr)
					{
						width *= sr.ScaleX;
						height *= sr.ScaleY;
					}
					if(element.LayoutTransform is ScaleTransform sl)
					{
						width *= sl.ScaleX;
						height *= sl.ScaleY;
					}
					element = VisualTreeHelper.GetParent(element) as FrameworkElement;
				}
				return (width, height);
			}

			var (targetWidth, targetHeight) = GetScaledSize(target);
			var (tooltipWidth, tooltipHeight) = GetScaledSize(tooltip);

			double ClampX(double value) => Math.Max(0, Math.Min(value, ActualWidth - tooltipWidth));
			double ClampY(double value) => Math.Max(0, Math.Min(value, ActualHeight - tooltipHeight));

			// Actually set position of tooltip
			switch (placement)
			{
				case PlacementMode.Top:
					Canvas.SetLeft(OverlayTooltip, ClampX(point.X + targetWidth / 2 - tooltipWidth / 2));
					Canvas.SetTop(OverlayTooltip, ClampY(point.Y - tooltipHeight));
					break;
				case PlacementMode.Bottom:
					Canvas.SetLeft(OverlayTooltip, ClampX(point.X + targetWidth / 2 - tooltipWidth / 2));
					Canvas.SetTop(OverlayTooltip, ClampY(point.Y + targetHeight));
					break;
				case PlacementMode.Left:
					Canvas.SetLeft(OverlayTooltip, ClampX(point.X - tooltipWidth));
					Canvas.SetTop(OverlayTooltip, ClampY(point.Y + targetHeight / 2 - tooltipHeight/ 2));
					break;
				case PlacementMode.Right:
					Canvas.SetLeft(OverlayTooltip, ClampX(point.X + targetWidth));
					Canvas.SetTop(OverlayTooltip, ClampY(point.Y + targetHeight / 2 - tooltipHeight / 2));
					break;
			}
		}

		#region CardTooltips
		private const int TooltipDelayMilliseconds = 400;
		private DateTime? _tooltipHoverStart = null;

		private DateTime? _minionBrowserHoverStart = null;
		private string? _minionBrowserHoverCardId = null;

		public  BigCardState? HoveredCard;

		private void UpdateCardTooltip()
		{
			var pos = User32.GetMousePos();
			var relativePlayerDeckPos = ViewBoxPlayer.PointFromScreen(new Point(pos.X, pos.Y));
			var relativePlayerActiveEffectsPos = PlayerActiveEffects.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentActiveEffectsPos = OpponentActiveEffects.PointFromScreen(new Point(pos.X, pos.Y));
			var relativePlayerCountersPos = PlayerCounters.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentCountersPos = OpponentCounters.PointFromScreen(new Point(pos.X, pos.Y));
			var relativePlayerTopDeckPos = PlayerTopDeckLens.CardList.Items.Count > 0 ? PlayerTopDeckLens.CardList.PointFromScreen(new Point(pos.X, pos.Y)) : new Point(-1, -1);
			var relativePlayerBottomDeckPos = PlayerBottomDeckLens.CardList.Items.Count > 0 ? PlayerBottomDeckLens.CardList.PointFromScreen(new Point(pos.X, pos.Y)) : new Point(-1, -1);
			var relativePlayerSideboardsDeckPos = PlayerSideboards.CardList.Items.Count > 0 ? PlayerSideboards.CardList.PointFromScreen(new Point(pos.X, pos.Y)) : new Point(-1, -1);
			var relativeOpponentDeckPos = ViewBoxOpponent.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeOpponentRelatedCardsPos = OpponentRelatedCardsDeckLens.CardList.Items.Count > 0 ? OpponentRelatedCardsDeckLens.CardList.PointFromScreen(new Point(pos.X, pos.Y)) : new Point(-1, -1);
			var relativeSecretsPos = StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y));
			var relativeCardMark = _cardMarks.Select(x => new { Label = x, Pos = x.PointFromScreen(new Point(pos.X, pos.Y)) });
			var visibility = (Config.Instance.OverlayCardToolTips && !Config.Instance.OverlaySecretToolTipsOnly)
								 ? Visible : Hidden;

			var cardMark =
				relativeCardMark.FirstOrDefault(
					x =>
						x.Label.IsVisible &&
						PointInsideControl(
							x.Pos,
							x.Label.ActualWidth,
							x.Label.SourceCardBitmap != null ? x.Label.ActualHeight * 1.7 : x.Label.ActualHeight,
							new Thickness(2, 2, 2, 2)
						)
				);

			ToolTipCardBlock.CreatedByVisibility = Collapsed;
			if(!Config.Instance.HideOpponentCardMarks && cardMark != null)
			{
				var index = _cardMarks.IndexOf(cardMark.Label);
				var drawnEntity = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == index + 1 && x.Info.GetDrawerId() != null);
				var entity = _game.Opponent.Hand.FirstOrDefault(x => x.GetTag(GameTag.ZONE_POSITION) == index + 1 && x.HasCardId && !x.Info.Hidden);
				var creatorEntity = _game.Opponent.Hand.FirstOrDefault(x =>
					x.GetTag(GameTag.ZONE_POSITION) == index + 1
					&& x.Info.GetCreatorId() > 0
					&& _game.Entities.ContainsKey(x.Info.GetCreatorId()));
				var card = entity?.Card;
				var creatorCard = _cardMarks[index].SourceCard;
				if(card != null || creatorCard != null)
				{
					if((creatorEntity != null || drawnEntity != null) && card == null)
					{
						var creatorDescription = "Created By ";
						if(drawnEntity?.Info.GetDrawerId() != null && drawnEntity?.Info.GetDrawerId() > 0)
							creatorDescription = "Drawn By ";
						ToolTipCardBlock.CreatedByText =  $"{creatorDescription}{creatorCard?.Name}";
						ToolTipCardBlock.CreatedByVisibility = Visible;
					}
					ToolTipCardBlock.SetCardIdFromCard(card ?? creatorCard);
					var offset = _cardMarks[index].ActualHeight * 1.1;
					var topOffset = Canvas.GetTop(_cardMarks[index]) + offset;
					var leftOffset = Canvas.GetLeft(_cardMarks[index]) + offset;
					Canvas.SetTop(ToolTipCardBlock, topOffset);
					Canvas.SetLeft(ToolTipCardBlock, leftOffset);
					ToolTipCardBlock.Visibility = Config.Instance.OverlayCardMarkToolTips ? Visible : Hidden;
				}
				else
				{
					ToolTipCardBlock.Visibility = Hidden;
				}
			}
			// player or opponent active effects tooltip
			else if (
				PointInsideControl(relativeOpponentActiveEffectsPos, OpponentActiveEffects.ActualWidth, OpponentActiveEffects.ActualHeight) ||
				PointInsideControl(relativePlayerActiveEffectsPos, PlayerActiveEffects.ActualWidth, PlayerActiveEffects.ActualHeight))
			{
				var isOpponent = PointInsideControl(relativeOpponentActiveEffectsPos, OpponentActiveEffects.ActualWidth, OpponentActiveEffects.ActualHeight);
				var relativeActiveEffectsPos = relativePlayerActiveEffectsPos;
				var activeEffects = PlayerActiveEffects;

				if (isOpponent)
				{
					relativeActiveEffectsPos = relativeOpponentActiveEffectsPos;
					activeEffects = OpponentActiveEffects;
				}

				var outerMargin = ActiveEffectsOverlay.OuterMargin * _activeEffectsScale;
				var effectSize = (ActiveEffectsOverlay.EffectSize + ActiveEffectsOverlay.InnerMargin * 2) * _activeEffectsScale;
				var width = activeEffects.ActualWidth * _activeEffectsScale;
				var height = activeEffects.ActualHeight * _activeEffectsScale;
				var poxX = relativeActiveEffectsPos.X * _activeEffectsScale;
				var poxY = relativeActiveEffectsPos.Y * _activeEffectsScale;

				var columns = (int)(width / effectSize);

				// Check if the position is within the outer margin
				if (poxX < outerMargin || poxX > width - outerMargin ||
				    poxY < outerMargin || poxY > height - outerMargin)
				{
					ToolTipCardBlock.Visibility = Hidden;
					return;
				}

				// Adjust the position by subtracting the outer margin
				var adjustedPosX = poxX - outerMargin;
				var adjustedPosY = poxY - outerMargin;

				var effectIndexX = (int)(adjustedPosX / effectSize);
				var effectIndexY =  (int)(adjustedPosY / effectSize);
				var effectIndex = effectIndexY * columns + effectIndexX;

				if (isOpponent)
				{
					var totalCells = columns * (int)(height / effectSize);
					var rows = totalCells / columns;
					var row = effectIndex / columns;
					var col = effectIndex % columns;
					var mirroredRow = rows - row - 1;
					effectIndex = mirroredRow * columns + col;
				}

				if(effectIndex < 0 || effectIndex >= activeEffects.VisibleEffects.Count)
				{
					ToolTipCardBlock.Visibility = Hidden;
					return;
				}

				var effect = activeEffects.VisibleEffects[effectIndex];
				ToolTipCardBlock.SetCardIdFromCard(effect.Effect.CardToShowInUI);
				var leftOffset = Canvas.GetLeft(activeEffects) + effectIndexX * effectSize + effectSize;
				var maxLeftOffset = Canvas.GetLeft(activeEffects) + columns * effectSize;

				// Swap the side of the tooltip if it would go outside the overlay
				if (maxLeftOffset + ToolTipCardBlock.ActualWidth > Width)
					leftOffset -= ToolTipCardBlock.ActualWidth + effectSize / columns;

				var yOffset = effectIndexY * effectSize + effectSize / 2 - ToolTipCardBlock.ActualHeight / 2 + outerMargin / 2;
				Canvas.SetTop(ToolTipCardBlock, Canvas.GetTop(activeEffects) + yOffset);
				Canvas.SetLeft(ToolTipCardBlock, leftOffset);
				Panel.SetZIndex(ToolTipCardBlock, int.MaxValue);
				ToolTipCardBlock.Visibility = visibility;
			}
			else if(
				PointInsideControl(relativeOpponentCountersPos, OpponentCounters.ActualWidth,
					OpponentCounters.ActualHeight) ||
				PointInsideControl(relativePlayerCountersPos, PlayerCounters.ActualWidth,
					PlayerCounters.ActualHeight))
			{
				if (_tooltipHoverStart == null)
				{
					_tooltipHoverStart = DateTime.Now;
				}

				ToolTipGridCards.Visibility = Hidden;
				var isOpponent = PointInsideControl(relativeOpponentCountersPos, OpponentCounters.ActualWidth, OpponentCounters.ActualHeight);
				var relativeCountersPos = relativePlayerCountersPos;
				var counters = PlayerCounters;

				if (isOpponent)
				{
					relativeCountersPos = relativeOpponentCountersPos;
					counters = OpponentCounters;
				}

				var counterCards = new ObservableCollection<Hearthstone.Card>();
				var relativePosX = relativeCountersPos.X * _activeEffectsScale;

				int? counterIndex = null;
				var counterOffset = 0.0;

				var counterWidths = counters.GetWidths();
				for(int i = 0; i < counterWidths[i]; i++)
				{
					var width = (counterWidths[i] + CountersOverlay.InnerMargin * 2) * _activeEffectsScale;
						if(relativePosX >= counterOffset && relativePosX <= counterOffset + width)
						{
							counterIndex = i;
							break;
						}

						counterOffset += width;
				}

				if(counterIndex == null)
				{
					ToolTipGridCards.Visibility = Hidden;
					return;
				}

				// get the hovered counter
				var hoveredCounter = counters.VisibleCounters[(int)counterIndex];
				if(hoveredCounter == null)
				{
					ToolTipGridCards.Visibility = Hidden;
					return;
				}

				foreach(var cardId in hoveredCounter.GetCardsToDisplay())
				{
					var card = Database.GetCardFromId(cardId);
					if(card == null) continue;

					card.BaconCard = hoveredCounter.IsBattlegroundsCounter;
					counterCards.Add(card);
				}

				var counterWidth = counterWidths[(int)counterIndex];

				var yOffset = (counters.ActualHeight + 5) * _activeEffectsScale;
				var xOffset = (ToolTipGridCards.ActualWidth / 2 - counterWidth / 2) * _activeEffectsScale - counterOffset;

				var maxBottomOffset = Canvas.GetTop(counters) + (counters.ActualHeight + 5 + ToolTipGridCards.ActualHeight) * _activeEffectsScale;

				if (maxBottomOffset > Height)
					yOffset -= yOffset + (5 + ToolTipGridCards.ActualHeight) * _activeEffectsScale;

				var canvasLeftPosition = Canvas.GetLeft(counters) - xOffset;

				if(canvasLeftPosition < 0)
					canvasLeftPosition = 0;
				if(canvasLeftPosition + ToolTipGridCards.ActualWidth > Width)
					canvasLeftPosition = Width - ToolTipGridCards.ActualWidth;

				Canvas.SetTop(ToolTipGridCards, Canvas.GetTop(counters) + yOffset);
				Canvas.SetLeft(ToolTipGridCards, canvasLeftPosition);
				Panel.SetZIndex(ToolTipGridCards, int.MaxValue);

				var elapsed = DateTime.Now - _tooltipHoverStart.Value;
				if (elapsed.TotalMilliseconds >= TooltipDelayMilliseconds)
				{
					ToolTipGridCards.Visibility = Visible;
					ToolTipGridCards.SetCardIdsFromCards(counterCards);
					ToolTipGridCards.SetTitle(hoveredCounter.LocalizedName);
				}
			}
			//player card tooltips
			else if(ListViewPlayer.Visibility == Visible && StackPanelPlayer.Visibility == Visible
					&& PointInsideControl(relativePlayerDeckPos, ViewBoxPlayer.ActualWidth, ViewBoxPlayer.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = ViewBoxPlayer.ActualHeight / ListViewPlayer.Items.Count;
				var cardIndex = (int)(relativePlayerDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= ListViewPlayer.Items.Count)
					return;

				var card = ListViewPlayer.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				var centeredListOffset = Config.Instance.OverlayCenterPlayerStackPanel ? (BorderStackPanelPlayer.ActualHeight - StackPanelPlayer.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer) + centeredListOffset
								+ GetListViewOffset(StackPanelPlayer, ViewBoxPlayer) + cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Player, card.Id);
			}
			//player top card tooltips
			else if(PlayerTopDeckLens.Visibility == Visible && StackPanelPlayer.Visibility == Visible
					&& PointInsideControl(relativePlayerTopDeckPos, PlayerTopDeckLens.CardList.ActualWidth, PlayerTopDeckLens.CardList.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = PlayerTopDeckLens.CardList.ActualHeight / PlayerTopDeckLens.CardList.Items.Count;
				var cardIndex = (int)(relativePlayerTopDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= PlayerTopDeckLens.CardList.Items.Count)
					return;

				var card = PlayerTopDeckLens.CardList.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer)
								+ GetListViewOffset(StackPanelPlayer, PlayerTopDeckLens)
								+ PlayerTopDeckLens.Container.ActualHeight
								+ cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Player, card.Id);
			}
			//player bottom card tooltips
			else if(PlayerBottomDeckLens.Visibility == Visible && StackPanelPlayer.Visibility == Visible
					&& PointInsideControl(relativePlayerBottomDeckPos, PlayerBottomDeckLens.CardList.ActualWidth, PlayerBottomDeckLens.CardList.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = PlayerBottomDeckLens.CardList.ActualHeight / PlayerBottomDeckLens.CardList.Items.Count;
				var cardIndex = (int)(relativePlayerBottomDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= PlayerBottomDeckLens.CardList.Items.Count)
					return;

				var card = PlayerBottomDeckLens.CardList.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer)
								+ GetListViewOffset(StackPanelPlayer, PlayerBottomDeckLens)
								+ PlayerBottomDeckLens.Container.ActualHeight
								+ cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Player, card.Id);
			}
			//player sideboard card tooltips
			else if(PlayerSideboards.Visibility == Visible && StackPanelPlayer.Visibility == Visible
					&& PointInsideControl(relativePlayerSideboardsDeckPos, PlayerSideboards.CardList.ActualWidth, PlayerSideboards.CardList.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = PlayerSideboards.CardList.ActualHeight / PlayerSideboards.CardList.Items.Count;
				var cardIndex = (int)(relativePlayerSideboardsDeckPos.Y / cardSize);

				Log.Debug($"cardIndex: {cardIndex}, relativePlayerSideboardsDeckPos: {relativePlayerSideboardsDeckPos}");
				if(cardIndex < 0 || cardIndex >= PlayerSideboards.CardList.Items.Count)
					return;

				var card = PlayerSideboards.CardList.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelPlayer)
								+ GetListViewOffset(StackPanelPlayer, PlayerSideboards)
								+ PlayerSideboards.Container.ActualHeight
								+ cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelPlayer);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Player, card.Id);
			}
			//opponent card tooltips
			else if(ListViewOpponent.Visibility == Visible && StackPanelOpponent.Visibility == Visible
					&& PointInsideControl(relativeOpponentDeckPos, ViewBoxOpponent.ActualWidth, ViewBoxOpponent.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = ViewBoxOpponent.ActualHeight / ListViewOpponent.Items.Count;
				var cardIndex = (int)(relativeOpponentDeckPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= ListViewOpponent.Items.Count)
					return;

				var centeredListOffset = Config.Instance.OverlayCenterOpponentStackPanel ? (BorderStackPanelOpponent.ActualHeight - StackPanelOpponent.ActualHeight) / 2 : 0;
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelOpponent) + centeredListOffset
								+ GetListViewOffset(StackPanelOpponent, ViewBoxOpponent) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100 - ToolTipCardBlock.ActualHeight / 2;
				var card = ListViewOpponent.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);
				SetTooltipPosition(topOffset, BorderStackPanelOpponent);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Opponent, card.Id);
			}
			// opponent related cards tooltip
			else if(OpponentRelatedCardsDeckLens.Visibility == Visible && StackPanelOpponent.Visibility == Visible
			        && PointInsideControl(relativeOpponentRelatedCardsPos, OpponentRelatedCardsDeckLens.ActualWidth, OpponentRelatedCardsDeckLens.ActualHeight))

			{
				//card size = card list height / amount of cards
				var cardSize = OpponentRelatedCardsDeckLens.CardList.ActualHeight / OpponentRelatedCardsDeckLens.CardList.Items.Count;
				var cardIndex = (int)(relativeOpponentRelatedCardsPos.Y / cardSize);

				if(cardIndex < 0 || cardIndex >= OpponentRelatedCardsDeckLens.CardList.Items.Count)
					return;

				var card = OpponentRelatedCardsDeckLens.CardList.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card;
				ToolTipCardBlock.SetCardIdFromCard(card);
				//offset is affected by scaling
				var topOffset = Canvas.GetTop(BorderStackPanelOpponent)
					+ GetListViewOffset(StackPanelOpponent, OpponentRelatedCardsDeckLens)
					+ OpponentRelatedCardsDeckLens.Container.ActualHeight
					+ cardIndex * cardSize * Config.Instance.OverlayPlayerScaling / 100 - ToolTipCardBlock.ActualHeight/2;

				//prevent tooltip from going outside of the overlay
				if(topOffset + ToolTipCardBlock.ActualHeight > Height)
					topOffset = Height - ToolTipCardBlock.ActualHeight;
				topOffset = Math.Max(0, topOffset);

				SetTooltipPosition(topOffset, BorderStackPanelOpponent);

				ToolTipCardBlock.Visibility = visibility;
				SetRelatedCardsTooltip(Core.Game.Opponent, card.Id);
			}
			else if(StackPanelSecrets.Visibility == Visible
					&& PointInsideControl(relativeSecretsPos, StackPanelSecrets.ActualWidth, StackPanelSecrets.ActualHeight))
			{
				//card size = card list height / amount of cards
				var cardSize = StackPanelSecrets.ActualHeight / StackPanelSecrets.Children.Count;
				var cardIndex = (int)(relativeSecretsPos.Y / cardSize);
				if(cardIndex < 0 || cardIndex >= StackPanelSecrets.Children.Count)
					return;

				//offset is affected by scaling
				var topOffset = Canvas.GetTop(StackPanelSecrets) + cardIndex * cardSize * Config.Instance.OverlayOpponentScaling / 100 - ToolTipCardBlock.ActualHeight / 2;
				var card = StackPanelSecrets.Children.Cast<Controls.Card>().ElementAt(cardIndex);
				if(card.CardId != null)
				{
					ToolTipCardBlock.SetCardIdFromCard(new Hearthstone.Card() { Id = card.CardId, BaconCard = false });
					//prevent tooltip from going outside of the overlay
					if(topOffset + ToolTipCardBlock.ActualHeight > Height)
						topOffset = Height - ToolTipCardBlock.ActualHeight;
					topOffset = Math.Max(0, topOffset);
					SetTooltipPosition(topOffset, StackPanelSecrets);
				}

				ToolTipCardBlock.Visibility = Config.Instance.OverlaySecretToolTipsOnly ? Visible : visibility;
			}
			else if(BgsTopBar.Visibility == Visible && BattlegroundsMinionsPanel.Visibility == Visible && (BattlegroundsMinionsVM.ActiveTier != null || BattlegroundsMinionsVM.ActiveMinionType != null))
			{
				var found = false;
				for(var i = 0; i < BattlegroundsMinionsPanel.GroupsControl.Items.Count; i++)
				{
					var container = BattlegroundsMinionsPanel.GroupsControl.ItemContainerGenerator.ContainerFromIndex(i);
					if(VisualTreeHelper.GetChildrenCount(container) == 0)
						continue;
					var group = (BattlegroundsCardsGroup)VisualTreeHelper.GetChild(container, 0);
					var cardList = group.CardsList;
					if(!group.IsVisible || !cardList.IsVisible)
						continue;
					var relativePos = cardList.PointFromScreen(new Point(pos.X, pos.Y));
					if(PointInsideControl(relativePos, cardList.ActualWidth, cardList.ActualHeight))
					{
						var cards = cardList.ItemsControl.Items;
						var cardSize = cardList.ActualHeight / cards.Count;
						var cardIndex = (int)(relativePos.Y / cardSize);
						if(cardIndex < 0 || cardIndex >= cards.Count)
							return;
						var card = cards.GetItemAt(cardIndex) as AnimatedCard;
						if(card == null)
							return;

						ToolTipCardBlock.SetCardIdFromCard(card.Card);

						//offset is affected by scaling
						var cardListPos = cardList.TransformToAncestor(CanvasInfo).Transform(new Point(0, 0));
						var topOffset = cardListPos.Y + cardIndex * cardSize * AutoScaling - ToolTipCardBlock.ActualHeight / 2;
						topOffset = Math.Max(0, topOffset);
						//prevent tooltip from going outside of the overlay
						if(topOffset + ToolTipCardBlock.ActualHeight > Height)
							topOffset = Height - ToolTipCardBlock.ActualHeight;

						Canvas.SetTop(ToolTipCardBlock, topOffset);
						Canvas.SetLeft(ToolTipCardBlock, cardListPos.X - ToolTipCardBlock.ActualWidth + 22);

						ToolTipCardBlock.Visibility = visibility;

						if(_minionBrowserHoverCardId != card.Card.Id)
						{
							_minionBrowserHoverStart = DateTime.Now;
							_minionBrowserHoverCardId = card.Card.Id;
						}

						if(Cards.NormalToTripleCardIds.TryGetValue(card.Card.Id, out var goldenCardId) && goldenCardId != card.Card.Id)
						{
							var goldenCard = Database.GetCardFromId(goldenCardId);
							if(goldenCard != null)
							{
								goldenCard.BaconCard = true;
								goldenCard.BaconTriple = true;
								var hovered = (_minionBrowserHoverStart is DateTime start) ? DateTime.Now - start : TimeSpan.Zero;
								if(hovered >= TimeSpan.FromMilliseconds(800)) {
									ToolTipCardBlock2.SetCardIdFromCard(goldenCard);

									Canvas.SetTop(ToolTipCardBlock2, topOffset);
									Canvas.SetLeft(ToolTipCardBlock2, cardListPos.X - ToolTipCardBlock.ActualWidth + 22 - ToolTipCardBlock2.ActualWidth + 22);

									ToolTipCardBlock2.Visibility = Visible;
								}
								else
								{
									if(hovered >= TimeSpan.FromMilliseconds(250))
									{
										// preload golden card image
										AssetDownloaders.cardImageDownloader?.GetAssetData(goldenCard);
									}

									ToolTipCardBlock2.Visibility = Hidden;
								}
							}
						}

						found = true;
					}
				}

				if(!found)
				{
					ToolTipCardBlock.SetCardIdFromCard(null);
					ToolTipCardBlock2.SetCardIdFromCard(null);
					ToolTipGridCards.SetCardIdsFromCards(null);
					ToolTipGridCards.Visibility = Hidden;
					ToolTipCardBlock.Visibility = Hidden;
					ToolTipCardBlock2.Visibility = Hidden;
					_tooltipHoverStart = null;
					_minionBrowserHoverStart = null;
					_minionBrowserHoverCardId = null;
					HideAdditionalToolTips();
				}
			}
			else if(HoveredCard is { IsHand: true } && Core.Game.IsTraditionalHearthstoneMatch)
			{
				// Get related cards from cardId
				var relatedCards = GetRelatedCards(Core.Game.Player, HoveredCard.Value.CardId, inHand: true, handPosition: HoveredCard.Value.ZonePosition);

				if (_tooltipHoverStart == null)
				{
					_tooltipHoverStart = DateTime.Now;
				}

				var elapsed = DateTime.Now - _tooltipHoverStart.Value;
				if (relatedCards.Count > 0)
				{
					var nonNullableRelatedCards = relatedCards.Where(c => c != null).Cast<Hearthstone.Card>();

					ToolTipGridCards.SetTitle(LocUtil.Get("Related_Cards", useCardLanguage: true));
					ToolTipGridCards.SetCardIdsFromCards(nonNullableRelatedCards, 470);
					Canvas.SetTop(ToolTipGridCards, (480 - ToolTipGridCards.ActualHeight) * _activeEffectsScale);

					// find the left of the card
					double cardTotal = HoveredCard.Value.ZoneSize > 10 ? HoveredCard.Value.ZoneSize : 10;
					var baseOffsetX = 0.34;
					var centerPosition = (HoveredCard.Value.ZoneSize + 1) / 2.0;
					var relativePosition = HoveredCard.Value.ZonePosition - centerPosition;
					var offsetXScale = HoveredCard.Value.ZoneSize > 3 ? cardTotal / HoveredCard.Value.ZoneSize * 0.037 : 0.098;
					var offsetX = baseOffsetX + relativePosition * offsetXScale;
					var correctedOffsetX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio);

					// find the center of the card
					var cardHeight = 0.5;
					var cardHeightInPixels = cardHeight * Height;
					var cardWidth = cardHeightInPixels * 34 / (cardHeight * 100);

					Canvas.SetLeft(ToolTipGridCards,
						correctedOffsetX + cardWidth / 2 - ToolTipGridCards.ActualWidth / 2 * _activeEffectsScale);

					if(elapsed.TotalMilliseconds >= TooltipDelayMilliseconds)
					{
						ToolTipGridCards.Visibility = Config.Instance.HidePlayerRelatedCards ? Collapsed : Visible;
					}
					else
					{
						ToolTipGridCards.Visibility = Hidden;
					}

				}
				else
				{
					ToolTipGridCards.Visibility = Hidden;
					_tooltipHoverStart = null;
				}

			}
			// player secrets/objective zone
			else if(HoveredCard is { ZonePosition: > 0, IsHand: false, Side: (int)PlayerSide.FRIENDLY })
			{
				List<Hearthstone.Card?> relatedCards = new();
				var entity = Core.Game.Player.Objectives.ElementAtOrDefault(HoveredCard.Value.ZonePosition - 1);
				if (entity != null && entity.CardId == HoveredCard.Value.CardId)
					relatedCards.AddRange(entity.Info.StoredCardIds.Select(Database.GetCardFromId));

				if (_tooltipHoverStart == null)
				{
					_tooltipHoverStart = DateTime.Now;
				}

				var elapsed = DateTime.Now - _tooltipHoverStart.Value;

				if(relatedCards.Count > 0)
				{
					var nonNullableRelatedCards = relatedCards.Where(c => c != null).Cast<Hearthstone.Card>();

					ToolTipGridCards.SetTitle(LocUtil.Get("Related_Cards", useCardLanguage: true));
					ToolTipGridCards.SetCardIdsFromCards(nonNullableRelatedCards, 470);
					Canvas.SetTop(ToolTipGridCards, (480 - ToolTipGridCards.ActualHeight) * _activeEffectsScale);

					// find the left of the card
					var baseOffsetX = 0.57;
					var leftOffsetXByLayer =  new [] { 0.0, 0.037, 0.062 };
					var rightOffsetXByLayer =  new [] { 0.0, 0.034, 0.059 };
					var relativePosition = HoveredCard.Value.ZonePosition - 1;
					var isLeftSide = relativePosition % 2 != 0;
					var layer = (int)Math.Ceiling(relativePosition / 2.0);
					var offsetX = isLeftSide ? baseOffsetX - leftOffsetXByLayer[layer] : baseOffsetX + rightOffsetXByLayer[layer];
					var correctedOffsetX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio);

					// find the center of the card
					var cardHeight = 0.43;
					var cardHeightInPixels = cardHeight * Height;
					var cardWidth = cardHeightInPixels * 31 / (cardHeight * 100);

					Canvas.SetLeft(ToolTipGridCards,
						correctedOffsetX + cardWidth / 2 - ToolTipGridCards.ActualWidth / 2 * _activeEffectsScale);

					if(elapsed.TotalMilliseconds >= TooltipDelayMilliseconds)
					{
						ToolTipGridCards.Visibility = Config.Instance.HidePlayerRelatedCards ? Collapsed : Visible;
					}
					else
					{
						ToolTipGridCards.Visibility = Hidden;
					}
				}
				else
				{
					ToolTipGridCards.Visibility = Hidden;
					_tooltipHoverStart = null;
				}

			}
			// opponent secrets/objective zone
			else if(HoveredCard is { ZonePosition: > 0, IsHand: false } && HoveredCard.Value.Side != (int)PlayerSide.FRIENDLY)
			{
				List<Hearthstone.Card?> relatedCards = new();
				var entity = Core.Game.Opponent.Objectives.ElementAtOrDefault(HoveredCard.Value.ZonePosition - 1);
				if (entity != null && entity.CardId == HoveredCard.Value.CardId)
					relatedCards.AddRange(entity.Info.StoredCardIds.Select(Database.GetCardFromId));

				if (_tooltipHoverStart == null)
				{
					_tooltipHoverStart = DateTime.Now;
				}

				var elapsed = DateTime.Now - _tooltipHoverStart.Value;

				if(relatedCards.Count > 0)
				{
					var nonNullableRelatedCards = relatedCards.Where(c => c != null).Cast<Hearthstone.Card>();

					ToolTipGridCards.SetTitle(LocUtil.Get("Related_Cards", useCardLanguage: true));
					ToolTipGridCards.SetCardIdsFromCards(nonNullableRelatedCards, 470);
					Canvas.SetTop(ToolTipGridCards, Height / 2);

					// find the left of the card
					var baseOffsetX = 0.57;
					var leftOffsetXByLayer =  new [] { 0.0, 0.037, 0.062 };
					var rightOffsetXByLayer =  new [] { 0.0, 0.034, 0.059 };
					var relativePosition = HoveredCard.Value.ZonePosition - 1;
					var isLeftSide = relativePosition % 2 != 0;
					var layer = (int)Math.Ceiling(relativePosition / 2.0);
					var offsetX = isLeftSide ? baseOffsetX - leftOffsetXByLayer[layer] : baseOffsetX + rightOffsetXByLayer[layer];
					var correctedOffsetX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio);

					// find the center of the card
					var cardHeight = 0.43;
					var cardHeightInPixels = cardHeight * Height;
					var cardWidth = cardHeightInPixels * 31 / (cardHeight * 100);

					Canvas.SetLeft(ToolTipGridCards,
						correctedOffsetX + cardWidth / 2 - ToolTipGridCards.ActualWidth / 2 * _activeEffectsScale);

					if(elapsed.TotalMilliseconds >= TooltipDelayMilliseconds)
					{
						ToolTipGridCards.Visibility = Config.Instance.HideOpponentRelatedCards ? Collapsed : Visible;
					}
					else
					{
						ToolTipGridCards.Visibility = Hidden;
					}
				}
				else
				{
					ToolTipGridCards.Visibility = Hidden;
					_tooltipHoverStart = null;
				}
			}
			else
			{
				ToolTipCardBlock.SetCardIdFromCard(null);
				ToolTipCardBlock2.SetCardIdFromCard(null);
				ToolTipGridCards.SetCardIdsFromCards(null);
				ToolTipGridCards.Visibility = Hidden;
				ToolTipCardBlock.Visibility = Hidden;
				ToolTipCardBlock2.Visibility = Hidden;
				_tooltipHoverStart = null;
				_minionBrowserHoverStart = null;
				_minionBrowserHoverCardId = null;
				HideAdditionalToolTips();
			}

			if(!Config.Instance.ForceMouseHook)
			{
				if(Config.Instance.ExtraFeatures)
				{
					var relativePos = PointFromScreen(new Point(pos.X, pos.Y));
					if((StackPanelSecrets.IsVisible
						&& (PointInsideControl(StackPanelSecrets.PointFromScreen(new Point(pos.X, pos.Y)), StackPanelSecrets.ActualWidth,
											   StackPanelSecrets.ActualHeight, new Thickness(20))) || relativePos.X < 170 && relativePos.Y > Height - 120))
					{
						if(_mouseInput == null)
							HookMouse();
					}
					else if(_mouseInput != null && !((_isFriendsListOpen.HasValue && _isFriendsListOpen.Value) || Reflection.Client.IsFriendsListVisible()))
						UnHookMouse();
				}
				else if(_mouseInput != null)
					UnHookMouse();
			}
		}

		private double GetListViewOffset(Panel stackPanel, FrameworkElement target)
		{
			var offset = 0.0;
			foreach(var child in stackPanel.Children)
			{
				if(child is HearthstoneTextBlock text)
					offset += text.ActualHeight;
				else
				{
					if(child == target)
						break;
					if(child is FrameworkElement element)
						offset += element.ActualHeight;
				}
			}
			return offset;
		}

		private void HideAdditionalToolTips() => StackPanelAdditionalTooltips.Visibility = Hidden;

		private void SetTooltipPosition(double yOffset, FrameworkElement stackpanel)
		{
			Canvas.SetTop(ToolTipCardBlock, yOffset);

			if(yOffset + ToolTipGridCards.ActualHeight > Height)
			{
				Canvas.SetTop(ToolTipGridCards, Height - ToolTipGridCards.ActualHeight);
			}
			else
			{
				Canvas.SetTop(ToolTipGridCards, yOffset);
			}

			if(Canvas.GetLeft(stackpanel) < Width / 2)
			{
				Canvas.SetLeft(ToolTipCardBlock, Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100);
				Canvas.SetLeft(ToolTipGridCards,
					(Canvas.GetLeft(stackpanel) + stackpanel.ActualWidth * Config.Instance.OverlayOpponentScaling / 100) + ToolTipCardBlock.ActualWidth);
			}
			else
			{
				Canvas.SetLeft(ToolTipCardBlock, Canvas.GetLeft(stackpanel) - ToolTipCardBlock.ActualWidth);
				Canvas.SetLeft(ToolTipGridCards, Canvas.GetLeft(stackpanel) - ToolTipCardBlock.ActualWidth - ToolTipGridCards.ActualWidth * _activeEffectsScale);
			}
		}

		private void SetRelatedCardsTooltip(Player player, string cardId)
		{
			var relatedCards = GetRelatedCards(player, cardId);
			if (_tooltipHoverStart == null)
			{
				_tooltipHoverStart = DateTime.Now;
			}

			var elapsed = DateTime.Now - _tooltipHoverStart.Value;
			if (relatedCards.Count > 0 && elapsed.TotalMilliseconds >= TooltipDelayMilliseconds)
			{
				var nonNullableRelatedCards = relatedCards.Where(c => c != null).Cast<Hearthstone.Card>();
				ToolTipGridCards.SetCardIdsFromCards(nonNullableRelatedCards);
				ToolTipGridCards.SetTitle(LocUtil.Get("Related_Cards", useCardLanguage: true));
				ToolTipGridCards.Visibility = Visible;
			}
			else
			{
				ToolTipGridCards.Visibility = Hidden;
			}
		}

		private List<Hearthstone.Card?> GetRelatedCards(Player player, string cardId, bool inHand = false, int? handPosition = null)
		{
			var relatedCards = Core.Game.RelatedCardsManager.GetCardWithRelatedCards(cardId).GetRelatedCards(player);
			// Get related cards from Entity
			if (relatedCards.IsEmpty())
			{
				IEnumerable<Entity> entities;
				if(inHand)
				{
					entities = handPosition != null ? player.Hand.Where(e => e.ZonePosition == handPosition) : player.Hand.Where(e => e.CardId == cardId);
				}
				else
				{
					entities = player.Deck.Where(e => e.CardId == cardId);
				}
				foreach(var entity in entities)
				{
					relatedCards.AddRange(entity.Info.StoredCardIds.Select(Database.GetCardFromId));
				}
			}

			return relatedCards;
		}

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight)
			=> PointInsideControl(pos, actualWidth, actualHeight, new Thickness(0));

		public bool PointInsideControl(Point pos, double actualWidth, double actualHeight, Thickness margin)
			=> pos.X > 0 - margin.Left && pos.X < actualWidth + margin.Right && (pos.Y > 0 - margin.Top && pos.Y < actualHeight + margin.Bottom);

		#endregion

		#region FlavorText


		private Visibility _flavorTextVisibility = Collapsed;
		private string? _flavorTextCardName;
		private string? _flavorText;

		public string FlavorText
		{
			get
			{
				return string.IsNullOrEmpty(_flavorText) ? "-" : _flavorText!;
			}
			set
			{
				if(value != _flavorText)
				{
					_flavorText = value;
					OnPropertyChanged();
				}
			}
		}

		public string? FlavorTextCardName
		{
			get { return _flavorTextCardName; }
			set
			{
				if(value != _flavorTextCardName)
				{
					_flavorTextCardName = value;
					OnPropertyChanged();
				}
			}
		}

		public Visibility FlavorTextVisibility
		{
			get { return _flavorTextVisibility; }
			set
			{
				if(value != _flavorTextVisibility)
				{
					_flavorTextVisibility = value;
					OnPropertyChanged();
				}
			}
		}

		private void SetFlavorTextEntity(Entity entity)
		{
			try
			{
				if(!Config.Instance.ShowFlavorText || entity == null)
					return;
				var card = entity.Info.LatestCardId == entity.CardId
					? entity.Card
					: Database.GetCardFromId(entity.Info.LatestCardId);
				if(string.IsNullOrEmpty(card?.FormattedFlavorText))
					return;
				FlavorText = card!.FormattedFlavorText;
				FlavorTextCardName = card!.LocalizedName;
				FlavorTextVisibility = Visible;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		#endregion
	}
}
