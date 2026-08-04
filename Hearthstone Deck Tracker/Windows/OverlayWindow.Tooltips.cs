#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

#endregion

namespace Hearthstone_Deck_Tracker.Windows;

public partial class OverlayWindow
{
	private readonly Dictionary<DependencyObject, FrameworkElement> _activeTooltips = new();
	private readonly HashSet<DependencyObject> _delayedTooltips = new();
	private async void SetTooltip(Func<FrameworkElement>? getTooltip, DependencyObject target)
	{
		if(_activeTooltips.TryGetValue(target, out var current))
		{
			_activeTooltips.Remove(target);
			OverlayTooltip.Children.Remove(current);
		}

		if(getTooltip == null)
		{
			_delayedTooltips.Remove(target);
			return;
		}

		if(!ToolTipService.GetIsEnabled(target))
			return;

		var delay = ToolTipService.GetInitialShowDelay(target);
		if(delay > 0 && delay != 1000) // 1000 is the default
		{
			_delayedTooltips.Add(target);
			await Task.Delay(delay);
			if(!_delayedTooltips.Remove(target))
			{
				// Already removed. This means SetTooltip was called again with null for the same element.
				// We no longer want to show it.
				return;
			}
		}

		if(OverlayTooltip.Children.Count > 0)
		{
			// We already have a tooltip. For now, we don't support more than one.
			// We could potentially decide which one to use based on z-level?
			return;
		}

		var feTarget = target as FrameworkElement ?? Helper.GetLogicalParent<FrameworkElement>(target);
		if(feTarget == null)
			return;

		Point targetPos;
		try
		{
			targetPos = feTarget.TransformToAncestor(this).Transform(new Point(0, 0));
		}
		catch(InvalidOperationException)
		{
			// Element was probably unloaded
			return;
		}

		var tooltip = getTooltip();

		// The content of OverlayExtensions.ToolTip is not part of the visual tree (OverlayExtensions.ToolTip is
		// a property, not a framework element), and does therefore not have a DataContext.For bindings to work
		// correctly we need to set it here to match the target.
		tooltip.DataContext = target is FrameworkContentElement fce ? fce.DataContext : feTarget.DataContext;

		try
		{
			OverlayTooltip.Children.Add(tooltip);
		}
		catch(InvalidOperationException e)
		{
			// Likely happens if the element is already a child of another element.
			// Maybe SetTooltip got called twice?
			Log.Error(e);
			return;
		}
		_activeTooltips[target] = tooltip;


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

		var targetScale = Helper.GetTotalScaleTransform(feTarget);
		var targetWidth = targetScale.X * feTarget.ActualWidth;
		var targetHeight = targetScale.Y * feTarget.ActualHeight;

		if(OverlayExtensions.GetAutoScaleToolTip(feTarget))
			tooltip.LayoutTransform = new ScaleTransform(targetScale.X, targetScale.Y);

		var tooltipScale = Helper.GetTotalScaleTransform(tooltip);
		var tooltipWidth = tooltipScale.X * tooltip.ActualWidth;
		var tooltipHeight = tooltipScale.Y * tooltip.ActualHeight;

		// Correct placement if tooltip would go outside of window, and it fit on the other side
		switch (placement)
		{
			case PlacementMode.Top:
				if(targetPos.Y - tooltipHeight < 0 && targetPos.Y + targetHeight + tooltipHeight <= ActualHeight)
					placement = PlacementMode.Bottom;
				break;
			case PlacementMode.Bottom:
				if(targetPos.Y + targetHeight + tooltipHeight > ActualHeight && targetPos.Y - tooltipHeight >= 0)
					placement = PlacementMode.Top;
				break;
			case PlacementMode.Left:
				if(targetPos.X - tooltipWidth < 0 && targetPos.X + targetWidth + tooltipWidth <= ActualWidth)
					placement = PlacementMode.Right;
				break;
			case PlacementMode.Right:
				if(targetPos.X + targetWidth + tooltipWidth > ActualWidth && targetPos.X - tooltipWidth >= 0)
					placement = PlacementMode.Left;
				break;
		}

		// Update placement, since it may have swapped sides. We don't recalculate the size/placement again.
		// Hopefully the layout of the tooltip should not affect the size when swapping left/right.
		placementAwareTooltip?.SetPlacement(placement);

		var offsetX = ToolTipService.GetHorizontalOffset(target);
		var offsetY = ToolTipService.GetVerticalOffset(target);

		var (left, top) = placement switch
		{
			PlacementMode.Top => (targetPos.X + targetWidth / 2 - tooltipWidth / 2 + offsetX, targetPos.Y - tooltipHeight - offsetY),
			PlacementMode.Bottom => (targetPos.X + targetWidth / 2 - tooltipWidth / 2 + offsetX, targetPos.Y + targetHeight + offsetY),
			PlacementMode.Left => (targetPos.X - tooltipWidth - offsetX, targetPos.Y + targetHeight / 2 - tooltipHeight / 2 + offsetY),
			PlacementMode.Right => (targetPos.X + targetWidth + offsetX, targetPos.Y + targetHeight / 2 - tooltipHeight / 2 + offsetY),
		};

		var alignment = OverlayExtensions.GetToolTipAlignment(feTarget);

		switch(alignment)
		{
			case AlignmentMode.Start:
				if(placement is PlacementMode.Top or PlacementMode.Bottom)
					left = targetPos.X + offsetX;
				else
					top = targetPos.Y + offsetY;
				break;
			case AlignmentMode.End:
				if(placement is PlacementMode.Top or PlacementMode.Bottom)
					left = targetPos.X + targetWidth - tooltipWidth + offsetX;
				else
					top = targetPos.Y + targetHeight - tooltipHeight + offsetY;
				break;
		}

		var actualLeft = Math.Max(0, Math.Min(left, ActualWidth - tooltipWidth));
		var actualTop = Math.Max(0, Math.Min(top, ActualHeight - tooltipHeight));

		if(tooltip is IScreenBoundaryAware boundaryAware)
			boundaryAware.SetScreenBoundaryOffset(left - actualLeft, top - actualTop);

		Canvas.SetLeft(OverlayTooltip, actualLeft);
		Canvas.SetTop(OverlayTooltip, actualTop);

		if(target is UIElement uiElement)
			uiElement.RaiseEvent(new RoutedEventArgs(OverlayExtensions.TooltipLoadedEvent));
		else if (target is ContentElement contentElement)
			contentElement.RaiseEvent(new RoutedEventArgs(OverlayExtensions.TooltipLoadedEvent));
	}

	// Offset form top center secret by zone position.
	// Values are "percentage of height".
	private static readonly Point[] SecretZoneOffsets =
	{
		new(0, 0), // Zone Position 1
		new(-0.037, 0.024), // Zone Position 2
		new(0.034, 0.024), // Zone Position 3
		new(-0.062, 0.076), // Zone Position 4
		new(0.059, 0.076), // Zone Position 5
	};

	public void SetHeroGuidesTrigger(int zoneSize, int zonePosition, bool tooltipOnRight, string[] cards, bool buddiesEnabled = false)
	{
		var vm = (CardGridTooltipViewModel)GuidesTooltipTrigger.DataContext;
		vm.Reset();

		if(zoneSize != 4 || cards.Length == 0)
			return;

		vm.Cards = cards.Select(Database.GetCardFromId).WhereNotNull().ToList();

		if(vm.Cards == null || vm.Cards.Count == 0)
			return;

		GuidesTooltipTrigger.UpdateLayout();
		UpdateHoverable();

		var bgHeroPickHeroWidth = 0.165;
		var bgHeroPickHeroXSpacing = 0.075;
		var totalWidth = zoneSize * bgHeroPickHeroWidth + (zoneSize - 1) * bgHeroPickHeroXSpacing;
		var leftEdge = 0.5 - totalWidth / 2;

		var zoneIndex = Math.Max(zonePosition - 1, 0);
		var heroX =  leftEdge + zoneIndex * (bgHeroPickHeroWidth + bgHeroPickHeroXSpacing - 0.005) - (tooltipOnRight ?  0 : 0.165);

		vm.Scale = Height / 1080;
		vm.Left = Helper.GetScaledXPos(heroX, (int)Width, ScreenRatio);
		vm.Top = Height * 0.21;
		vm.Height = Height * 0.40;
		vm.Width = Height * 0.47;
		vm.TooltipPlacement = buddiesEnabled
			? (tooltipOnRight ? PlacementMode.Left : PlacementMode.Right)
			: (tooltipOnRight ? PlacementMode.Right : PlacementMode.Left);
	}

	public void SetAnomalyGuidesTrigger(string cardId)
	{
		var vm = (CardGridTooltipViewModel)GuidesTooltipTrigger.DataContext;

		vm.Reset();

		if(cardId == "")
			return;

		var card = Database.GetCardFromId(cardId);

		if(card == null)
			return;

		if(card.TypeEnum != CardType.BATTLEGROUND_ANOMALY)
			return;

		vm.Cards = new List<Card> { card };

		GuidesTooltipTrigger.UpdateLayout();
		UpdateHoverable();

		vm.Scale = Height / 1080;
		vm.Left = Helper.GetScaledXPos(0.90, (int)Width, ScreenRatio);
		vm.Top = Height * 0.33;
		vm.Height = Height * 0.1;
		vm.Width = Height * 0.13;
		vm.TooltipHorizontalOffset = -340 * vm.Scale;
		vm.TooltipVerticalOffset = 100 * vm.Scale;
		vm.TooltipPlacement = PlacementMode.Bottom;
	}

	public void SetTrinketGuidesTrigger(int zoneSize, int zonePosition, string cardId)
	{
		var vm = (CardGridTooltipViewModel)DiscoveryGuidesTooltipTrigger.DataContext;

		vm.Reset();

		if(cardId == "")
			return;

		var card = Database.GetCardFromId(cardId);

		if(card == null)
			return;

		if(card.TypeEnum != CardType.BATTLEGROUND_TRINKET)
			return;

		vm.Cards = new List<Card> { card };

		DiscoveryGuidesTooltipTrigger.UpdateLayout();
		UpdateHoverable();

		var bgTrinketPickWidth = 0.192;
		var leftEdge = 0.122;

		var trinketX = leftEdge + zonePosition * (bgTrinketPickWidth);

		vm.Scale = Height / 1080;
		vm.Left = Helper.GetScaledXPos(trinketX, (int)Width, ScreenRatio);
		vm.Top = Height * 0.28;
		vm.Height = Height * 0.35;
		vm.Width = Height * 0.25;
		vm.TooltipPlacement = PlacementMode.Bottom;
		vm.TooltipVerticalOffset = 10 * vm.Scale;
	}

	public void SetQuestGuidesTrigger(DiscoverState state)
	{
		var vm = (CardGridTooltipViewModel)DiscoveryGuidesTooltipTrigger.DataContext;

		if(state.EntityId is null or 0)
			return;

		if(!Core.Game.Entities.TryGetValue((int)state.EntityId, out var entity))
			return;

		var questRewardCardId = entity.GetTag(GameTag.QUEST_REWARD_DATABASE_ID);
		var questRewardCard = Database.GetCardFromDbfId(questRewardCardId, false);

		if(questRewardCard == null)
			return;

		if(questRewardCard.TypeEnum != CardType.BATTLEGROUND_QUEST_REWARD)
			return;

		vm.Reset();
		vm.Cards = new List<Card> { questRewardCard };

		DiscoveryGuidesTooltipTrigger.UpdateLayout();
		UpdateHoverable();

		var bgQuestPickWidth = 0.270;
		var leftEdge = 0.117;

		var questX = leftEdge + state.ZonePosition * bgQuestPickWidth;

		var rewardCardId = entity.GetTag(GameTag.BACON_CARD_DBID_REWARD);
		var rewardCard = Database.GetCardFromDbfId(rewardCardId, false);
		var isRewardCardPresent = rewardCard != null;
		var isGameTooltipRight = state.ZonePosition == 0;

		if(isRewardCardPresent)
			isGameTooltipRight = state.ZonePosition + 1 == state.ZoneSize;

		vm.Scale = Height / 1080;
		vm.Left = Helper.GetScaledXPos(questX, (int)Width, ScreenRatio);
		vm.Top = Height * 0.22;
		vm.Height = Height * 0.55;
		vm.Width = Height * 0.30;
		vm.TooltipPlacement = isGameTooltipRight ? PlacementMode.Right : PlacementMode.Left;
		vm.TooltipVerticalOffset = 32 * vm.Scale;
		vm.TooltipAlignment = AlignmentMode.Start;
	}


	public void ResetAnomalyGuidesMulliganTrigger()
	{
		var vm = (CardGridTooltipViewModel)AnomalyGuidesMulliganTrigger.DataContext;
		vm.Reset();
	}

	public void SetAnomalyGuidesMulliganTrigger()
	{
		var vm = (CardGridTooltipViewModel)AnomalyGuidesMulliganTrigger.DataContext;

		vm.Reset();

		var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
		var anomalyCard = anomalyDbfId.HasValue ? Database.GetCardFromDbfId(anomalyDbfId.Value, false) : null;

		if(anomalyCard is not { TypeEnum: CardType.BATTLEGROUND_ANOMALY })
			return;

		vm.Cards = new List<Card> { anomalyCard };

		GuidesTooltipTrigger.UpdateLayout();
		UpdateHoverable();

		vm.Scale = Height / 1080;
		vm.Left = Helper.GetScaledXPos(0.635, (int)Width, ScreenRatio);
		vm.Top = Height * 0.0825;
		vm.Height = Height * 0.123;
		vm.Width = Height * 0.1188;
		vm.TooltipHorizontalOffset = -600 * vm.Scale;
		vm.TooltipVerticalOffset = 20 * vm.Scale;
		vm.TooltipPlacement = PlacementMode.Bottom;
	}

	/// <summary>
	/// Computes the summary for <paramref name="pool"/> and decides how much of it the grid shows.
	///
	/// The pool is passed in rather than read back off the view model on purpose. Assigning
	/// vm.Cards makes GridCardImages build a CardAssetViewModel per card, and each of those reads
	/// and decodes that card's full-size render synchronously on the UI thread. Discover pools run
	/// to several hundred cards, so assigning the full pool and clamping afterwards decoded the
	/// whole pool in order to then display none of it.
	/// vm.Cards is therefore only ever assigned what will actually be rendered.
	/// </summary>
	private void ApplyRelatedCardsSummary(CardGridTooltipViewModel vm, ICardWithRelatedCards? cardWithRelatedCards,
		string cardId, bool summaryEnabled, Entity? hoveredEntity, List<Card> pool, List<Card>? dynamicPool = null)
	{
		if(cardWithRelatedCards is ICardWithDynamicRelatedCardsSummary cardWithDynamicPool && summaryEnabled)
		{
			// Summary depends on live game state (evolve/devolve targets); recomputed per hover.
			// dynamicPool is the same pool used to build the card grid, reused here to avoid a
			// second filter-and-dedup pass.
			vm.RelatedCardsSummaryTotalNum =
				cardWithDynamicPool.ComputeSummary(Core.Game.Player, out var summary,
					out var statistics, Config.Instance.OutfinderUsePercentages, hoveredEntity, dynamicPool);
			vm.RelatedCardsSummary = summary;
			vm.PoolStatistics = statistics;
		}
		else if(cardWithRelatedCards is ICardWithRelatedCardsSummary cardWithGeneratedPool && summaryEnabled)
		{
			var pickConfig = new PickConfig(cardWithGeneratedPool.Picks(), cardWithGeneratedPool.EventCount(), cardWithGeneratedPool.IsWithReplacement());
			vm.RelatedCardsSummaryTotalNum =
				RelatedCardsManager.TryGetRelatedCardsSummary(pool!, pickConfig, out var summary,
					out var statistics, Config.Instance.OutfinderUsePercentages);
			vm.RelatedCardsSummary = summary;
			vm.PoolStatistics = statistics;
		}
		else
		{
			// No summary for this card, so the grid is the whole point: show the pool as-is.
			// Also the path for a null cardWithRelatedCards, where the pool came off the entity's
			// StoredCardIds instead of a registered class.
			vm.RelatedCardsSummary = null;
			vm.PoolStatistics = null;
			vm.Cards = pool;
			return;
		}

		if(vm.RelatedCardsSummary != null)
			Core.Game.Metrics.ShowedTheOutfinder = true;

		SetHoveredLargePool(Database.GetCardFromId(cardId), pool);
		if(pool.Count > RelatedCardsManager.LargePoolThreshold)
		{
			// Summarised only. Right-click opens the full pool in the (virtualized) panel.
			vm.HasLargePool = true;
			vm.Cards = null;
		}
		else
			vm.Cards = pool;
	}

	public void SetRelatedCardsTrigger(BigCardState state)
	{
		// Note: To debug behavior here and/or implement new triggers set a translucent
		// Background (e.g. #40FF0000) on the RelatedCardsTrigger Grid in Overlay.xaml.

		var vm = (CardGridTooltipViewModel)RelatedCardsTrigger.DataContext;
		vm.Reset();
		SetHoveredLargePool(null, null);
		if(state.CardId == "")
			return;

		vm.Scale = Height / 1080;

		// Not ideal. Maybe we re-position the tooltip on size change and canvas.top/left change?
		RelatedCardsTrigger.UpdateLayout(); // After reset, force layout to update mouse event
		UpdateHoverable(); // Force mouse events to occur to update tooltip

		if(state is { IsHand: true } && Core.Game.IsTraditionalHearthstoneMatch && !Config.Instance.HidePlayerRelatedCards)
		{
			// May be null: some cards (Tidepool Pupil, Commander Sivara) have no registered
			// related-cards class and carry their cards on the entity instead - see the
			// StoredCardIds fallback below.
			var cardWithRelatedCards = Core.Game.RelatedCardsManager.GetCardWithRelatedCards(state.CardId);

			if(cardWithRelatedCards is ICardWithRelatedCardsSummary or ICardWithDynamicRelatedCardsSummary && (!Config.Instance.OutfinderEnabled || !Config.Instance.OutfinderInHand))
				return;

			var hoveredEntity = Core.Game.Player.Hand.FirstOrDefault(e => e.ZonePosition == state.ZonePosition);
			// Dynamic pools take the hovered entity so per-copy state (upgrade tags, costs)
			// picks the right pool when multiple copies are in hand. The pool is built once and
			// shared with the summary computation below.
			List<Card>? dynamicPool = null;
			List<Card?>? relatedCards;
			if(cardWithRelatedCards is ICardWithDynamicRelatedCardsSummary dynamicRelatedCards)
			{
				dynamicPool = dynamicRelatedCards.GetPool(Core.Game.Player, hoveredEntity);
				relatedCards = dynamicRelatedCards.GetRelatedCards(Core.Game.Player, hoveredEntity, dynamicPool);
			}
			else
				relatedCards = cardWithRelatedCards?.GetRelatedCards(Core.Game.Player);

			if(relatedCards == null || relatedCards.Count == 0)
			{
				relatedCards = hoveredEntity?.Info.StoredCardIds.Select(Database.GetCardFromId).ToList();
			}

			// Deliberately not assigned to vm.Cards yet - see ApplyRelatedCardsSummary.
			var pool = relatedCards?.WhereNotNull().ToList();
			if(pool == null || pool.Count == 0)
				return;

			vm.IsHandHover = true;
			vm.SummaryMaxHeight = Height * 0.4;

			ApplyRelatedCardsSummary(vm, cardWithRelatedCards, state.CardId,
				Config.Instance.OutfinderEnabled && Config.Instance.OutfinderInHand, hoveredEntity, pool, dynamicPool);

			vm.Top = Height * 0.47;
			vm.Height = Height * 0.53;
			vm.Width = Height * 0.34;

			double cardTotal = state.ZoneSize > 10 ? state.ZoneSize : 10;
			var centerPosition = (state.ZoneSize + 1) / 2.0;
			var relativePosition = state.ZonePosition - centerPosition;
			var offsetXScale = state.ZoneSize > 3 ? cardTotal / state.ZoneSize * 0.037 : 0.098;
			var offsetXConst = vm.RelatedCardsSummary == null || vm.Cards == null ? 0.34 : 0.22;
			var offsetX = offsetXConst + relativePosition * offsetXScale;

			vm.Left = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio);
		}
		// Player secrets/objective zone
		else if(state is { ZonePosition: > 0, IsHand: false, Side: (int)PlayerSide.FRIENDLY } && !Config.Instance.HidePlayerRelatedCards)
		{
			if(state.ZonePosition - 1 >= SecretZoneOffsets.Length)
				return;

			var entity = Core.Game.Player.SecretZone.ElementAtOrDefault(state.ZonePosition - 1);
			if(entity?.CardId != state.CardId)
			{
				// order / zone position does not seem to always be reliable
				entity = Core.Game.Player.SecretZone.FirstOrDefault(x => x.CardId == state.CardId);
			}
			vm.Cards = entity?.Info.StoredCardIds.Select(Database.GetCardFromId).WhereNotNull().ToList();
			if(vm.Cards == null || vm.Cards.Count == 0)
				return;

			var offset = SecretZoneOffsets[state.ZonePosition - 1];
			vm.Top = Height * (0.45 + offset.Y);
			vm.Width = Height * 0.56;
			vm.Height = Height - vm.Top; // All the way to the bottom. Does not matter.

			vm.Left = Helper.GetScaledXPos(0.47 + offset.X, (int)Width, ScreenRatio);
		}
		// Opponent secrets/objective zone
		else if(state is { ZonePosition: > 0, IsHand: false } && state.Side != (int)PlayerSide.FRIENDLY && !Config.Instance.HideOpponentRelatedCards)
		{
			if(state.ZonePosition - 1 >= SecretZoneOffsets.Length)
				return;

			var entity = Core.Game.Opponent.SecretZone.ElementAtOrDefault(state.ZonePosition - 1);
			if(entity?.CardId != state.CardId)
			{
				// order / zone position does not seem to always be reliable
				entity = Core.Game.Player.SecretZone.FirstOrDefault(x => x.CardId == state.CardId);
			}
			vm.Cards = entity?.Info.StoredCardIds.Select(Database.GetCardFromId).WhereNotNull().ToList();;
			if(vm.Cards == null || vm.Cards.Count == 0)
				return;
			vm.Top = 0; // Always top aligned
			vm.Width = Height * 0.56;
			vm.Height = Height * 0.44;

			var offset = SecretZoneOffsets[state.ZonePosition - 1];
			vm.Left = Helper.GetScaledXPos(0.48 + offset.X, (int)Width, ScreenRatio);
		}
	}

	public void SetRelatedCardsTrigger(DiscoverState state)
	{
		// Note: To debug behavior here and/or implement new triggers set a translucent
		// Background (e.g. #40FF0000) on the RelatedCardsTrigger Grid in Overlay.xaml.

		var vm = (CardGridTooltipViewModel)RelatedCardsTrigger.DataContext;
		vm.Reset();
		SetHoveredLargePool(null, null);
		if(state.CardId == "")
			return;

		vm.Scale = Height / 1080;

		// Not ideal. Maybe we re-position the tooltip on size change and canvas.top/left change?
		RelatedCardsTrigger.UpdateLayout(); // After reset, force layout to update mouse event
		UpdateHoverable(); // Force mouse events to occur to update tooltip

		if(!Config.Instance.HidePlayerRelatedCards)
		{
			var cardWithRelatedCards = Core.Game.RelatedCardsManager.GetCardWithRelatedCards(state.CardId);

			if(cardWithRelatedCards is ICardWithRelatedCardsSummary or ICardWithDynamicRelatedCardsSummary && (!Config.Instance.OutfinderEnabled || !Config.Instance.OutfinderInDeck))
				return;

			// Deck hovers have no hovered hand entity; dynamic pools fall back to the in-hand copy
			// so the grid reflects its live state. The pool is built once and shared with the
			// summary (which, as before, resolves its own targets from null entity).
			List<Card>? dynamicPool = null;
			List<Card?>? relatedCards;
			if(cardWithRelatedCards is ICardWithDynamicRelatedCardsSummary dynamicRelatedCards)
			{
				var handEntity = Core.Game.Player.Hand.FirstOrDefault(e => e.CardId == state.CardId);
				dynamicPool = dynamicRelatedCards.GetPool(Core.Game.Player, handEntity);
				relatedCards = dynamicRelatedCards.GetRelatedCards(Core.Game.Player, handEntity, dynamicPool);
			}
			else
				relatedCards = cardWithRelatedCards?.GetRelatedCards(Core.Game.Player);

			// Deliberately not assigned to vm.Cards yet - see ApplyRelatedCardsSummary.
			var pool = relatedCards?.WhereNotNull().ToList();
			if(pool == null || pool.Count == 0)
				return;

			ApplyRelatedCardsSummary(vm, cardWithRelatedCards, state.CardId,
				Config.Instance.OutfinderEnabled && Config.Instance.OutfinderInDeck, null, pool, dynamicPool);

			vm.Top = Height * 0.2;
			vm.Height = Height * 0.53;
			vm.Width = Height * 0.3;

			switch(state.ZoneSize)
			{
				case 4:
					vm.Left = (0.116 + state.ZonePosition * 0.2) * Width;
					vm.TooltipPlacement = state.ZonePosition < 2 ? PlacementMode.Right : PlacementMode.Left;
					break;
				case 3:
					const int centerPosition = 1;
					const double offsetXScale = 0.2;

					var relativePosition = state.ZonePosition - centerPosition;
					var offsetX = 0.5 - 0.088 + relativePosition * offsetXScale;
					vm.Left = offsetX * Width;
					vm.TooltipPlacement = PlacementMode.Right;
					break;
				case 2:
					vm.Left = state.ZonePosition == 0 ? 0.318 * Width : 0.518 * Width;
					vm.TooltipPlacement = state.ZonePosition == 0 ? PlacementMode.Left : PlacementMode.Right;
					break;
				case 1:
					vm.Left = (0.5 - 0.088) * Width;
					vm.TooltipPlacement = PlacementMode.Left;
					break;
			}
		}
	}

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

	// This is only needed for anomaly during mulligan because there is no way to retrieve the "hover action" by memory reading it.
	private void AnomalyGuidesMulliganTrigger_OnMouseEnter(object sender, MouseEventArgs e)
	{
		var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(_game.GameEntity);
		var anomalyCardId = anomalyDbfId.HasValue ? Database.GetCardFromDbfId(anomalyDbfId.Value, false) : null;

		SetMulliganAnomalyMask(anomalyCardId);
	}
	private void AnomalyGuidesMulliganTrigger_OnMouseLeave(object sender, MouseEventArgs e)
	{
		SetMulliganAnomalyMask(null);
	}
}

public class CardGridTooltipViewModel : ViewModel
{
	public double Width
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public double Height
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public double Top
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public double Left
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public double Scale
	{
		get => GetProp(1.0);
		set => SetProp(value);
	}

	public PlacementMode TooltipPlacement
	{
		get => GetProp(PlacementMode.Top);
		set => SetProp(value);
	}

	public double TooltipHorizontalOffset
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public double TooltipVerticalOffset
	{
		get => GetProp(0.0);
		set => SetProp(value);
	}

	public AlignmentMode TooltipAlignment
	{
		get => GetProp(AlignmentMode.Center);
		set => SetProp(value);
	}

	public List<Card>? Cards
	{
		get => GetProp<List<Card>?>(null);
		set => SetProp(value);
	}

	public Dictionary<string, string>? RelatedCardsSummary
	{
		get => GetProp<Dictionary<string, string>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(SummaryWidth));
		}
	}

	public int RelatedCardsSummaryTotalNum
	{
		get => GetProp(0);
		set => SetProp(value);
	}

	public PoolStatistics? PoolStatistics
	{
		get => GetProp<PoolStatistics?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(SummaryWidth));
		}
	}

	public bool HasLargePool
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public bool IsHandHover
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public double SummaryMaxHeight
	{
		get => GetProp(double.NaN);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(SummaryWidth));
		}
	}

	// Each keyword box is ~108px wide (100px + 4px margin each side) and ~73px tall.
	// The stats row (PoolStatistics) is a fixed 3-column grid that fits in 330px.
	// Default: 330px. For Hand hover with MaxHeight set, extra columns are added when
	// 3 keyword columns would overflow MaxHeight.
	public double SummaryWidth
	{
		get
		{
			const double itemWidth = 108.0;
			const double itemHeight = 73.0;
			const double defaultWidth = 330.0;

			var total = RelatedCardsSummary?.Count ?? 0;
			if(total == 0 || double.IsNaN(SummaryMaxHeight) || SummaryMaxHeight <= 0)
				return defaultWidth;

			var defaultItemsPerRow = (int)(defaultWidth / itemWidth); // 3
			var itemsPerColumn = Math.Max(1, (int)(SummaryMaxHeight / itemHeight));
			if(total <= defaultItemsPerRow * itemsPerColumn)
				return defaultWidth;

			var columnsNeeded = (int)Math.Ceiling((double)total / itemsPerColumn);
			return Math.Max(defaultWidth, columnsNeeded * itemWidth);
		}
	}

	public void Reset()
	{
		Width = 0;
		Height = 0;
		Top = -1;
		Left = -1;
		TooltipHorizontalOffset = 0;
		TooltipVerticalOffset = 0;
		Cards = null;
		RelatedCardsSummary = null;
		RelatedCardsSummaryTotalNum = 0;
		PoolStatistics = null;
		HasLargePool = false;
		IsHandHover = false;
		SummaryMaxHeight = double.NaN;
		TooltipAlignment = AlignmentMode.Center;
		TooltipPlacement = PlacementMode.Top;
	}
}
