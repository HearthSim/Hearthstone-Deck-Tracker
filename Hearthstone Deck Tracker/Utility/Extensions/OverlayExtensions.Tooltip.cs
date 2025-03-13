using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions;

partial class OverlayExtensions
{
	public static readonly DependencyProperty AutoScaleToolTipProperty = DependencyProperty.RegisterAttached("AutoScaleToolTip", typeof(bool), typeof(OverlayExtensions), new PropertyMetadata(false));
	public static bool GetAutoScaleToolTip(UIElement element) => (bool)element.GetValue(AutoScaleToolTipProperty);

	/// <summary>
	/// Automatically apply the total scaling of the target element, as well as the scaling of all parent elements
	/// as a LayoutTransform to the tooltip element
	/// </summary>
	public static void SetAutoScaleToolTip(UIElement element, bool value) => element.SetValue(AutoScaleToolTipProperty, value);

	public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(OverlayExtensions), new FrameworkPropertyMetadata(null, OnToolTipChange));

	public static object? GetToolTip(DependencyObject obj) => obj.GetValue(ToolTipProperty);

	/// <summary>
	/// Renders the value as a ToolTip in the OverlayWindow. Can be configured via <c>ToolTipService</c>.<br/>
	/// Supported Properties:<br/>
	/// - <c>ToolTipService.IsEnabled</c> Default: true<br/>
	/// - <c>ToolTipService.Placement</c> (Left, Top, Bottom, Right) - Default: Right<br/>
	/// - <c>ToolTipService.InitialShowDelay</c> Default: 0<br/>
	/// - <c>ToolTipService.HorizontalOffset</c> Default: 0<br/>
	/// - <c>ToolTipService.VerticalOffset</c> Default: 0
	/// </summary>
	public static void SetToolTip(DependencyObject obj, object tooltip) => obj.SetValue(ToolTipProperty, tooltip);

	public static readonly RoutedEvent TooltipLoadedEvent = EventManager.RegisterRoutedEvent("TooltipLoaded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OverlayExtensions));

	public static void AddTooltipLoadedHandler(DependencyObject d, RoutedEventHandler handler)
	{
		if(d is UIElement element)
			element.AddHandler(TooltipLoadedEvent, handler);
	}

	public static void RemoveTooltipLoadedHandler(DependencyObject d, RoutedEventHandler handler)
	{
		if(d is UIElement element)
			element.RemoveHandler(TooltipLoadedEvent, handler);
	}

	private static readonly Dictionary<DependencyObject, bool> _elementIsInOverlay = new ();

	private static bool IsInOverlay(DependencyObject d)
	{
		if(!_elementIsInOverlay.TryGetValue(d, out var val))
		{
			val = Window.GetWindow(d) is OverlayWindow;
			_elementIsInOverlay[d] = val;
		}
		return val;
	}

	public static event Action<Func<FrameworkElement>?, DependencyObject>? OnToolTipChanged;

	private static Func<FrameworkElement>? ResolveTooltip(DependencyObject element)
	{
		var tooltipData = GetToolTip(element);
		if(tooltipData is FrameworkElement e)
			return () => e;
		if(tooltipData is Type type && typeof(FrameworkElement).IsAssignableFrom(type))
		{
			// We may not actually render this tooltip for various reasons.
			// Only instantiate when we know for sure.
			return () => (FrameworkElement)Activator.CreateInstance(type);
		}
		return null;
	}

	private static void ShowTooltip(object sender, MouseEventArgs args)
	{
		if(sender is not DependencyObject d)
			return;
		if(GetIsOverlayHoverVisible(d) && args is not OverlayWindow.CustomMouseEventArgs)
		{
			// If the element defines IsOverlayHoverVisible, but the overlay is also non-transparent while the element
			// is hovered, this will be called twice, from the Overlay and the native event system. We only want to
			// process the event emitted by the Overlay. Any IsOverlayHoverVisible elements outside the overlay will be
			// shown using the native tooltip system, not mouse events, and therefore be unaffected by this.
			return;
		}
		if(IsInOverlay(d))
			OnToolTipChanged?.Invoke(ResolveTooltip(d), d);
	}

	private static void HideTooltip(object sender, MouseEventArgs args)
	{
		if(sender is not DependencyObject d)
			return;
		if(GetIsOverlayHoverVisible(d) && args is not OverlayWindow.CustomMouseEventArgs)
		{
			// See ShowTooltip for more details
			return;
		}
		if(IsInOverlay(d))
			OnToolTipChanged?.Invoke(null, d);
	}

	private static void OnElementLoaded(object sender, RoutedEventArgs e)
	{
		if(sender is not FrameworkElement element)
			return;
		if(IsInOverlay(element) || element.ToolTip is not null)
			return;
		// For anything not in the overlay, if we don't already have a normal ToolTip, set this as the ToolTip!
		ToolTipService.SetInitialShowDelay(element, 300);
		ToolTipService.SetShowDuration(element, 60_000);
		var tooltip = new ToolTip
		{
			Background = Brushes.Transparent,
			BorderBrush = Brushes.Transparent
		};
		tooltip.Initialized += (_, _) =>
		{
			// Lazy load content
			tooltip.Content = ResolveTooltip(element)?.Invoke();
		};
		element.ToolTip = tooltip;
	}

	private static void OnElementUnloaded(object sender, RoutedEventArgs routedEventArgs)
	{
		if(sender is DependencyObject d)
			UnloadTooltip(d);
	}

	private static void OnToolTipChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not IInputElement inputElement)
			return;

		if(e is { OldValue: not null, NewValue: not null })
			return;

		if(e.NewValue is not null)
		{
			inputElement.MouseEnter += ShowTooltip;
			inputElement.MouseLeave += HideTooltip;

			if(d is FrameworkContentElement fce)
			{
				fce.Loaded += OnElementLoaded;
				fce.Unloaded += OnElementUnloaded;
			}
			else
			{
				var fe = d as FrameworkElement ?? Helper.GetLogicalParent<FrameworkElement>(d);
				if(fe != null)
				{
					fe.Loaded += OnElementLoaded;
					fe.Unloaded += OnElementUnloaded;
				}
			}
		}
		else
		{
			inputElement.MouseEnter -= ShowTooltip;
			inputElement.MouseLeave -= HideTooltip;

			if(d is FrameworkContentElement fce)
			{
				fce.Loaded -= OnElementLoaded;
				fce.Unloaded -= OnElementUnloaded;
			}
			else
			{
				var fe = d as FrameworkElement ?? Helper.GetLogicalParent<FrameworkElement>(d);
				if(fe != null)
				{
					fe.Loaded -= OnElementLoaded;
					fe.Unloaded -= OnElementUnloaded;
				}
			}
			UnloadTooltip(d);
		}
	}

	private static void UnloadTooltip(DependencyObject d)
	{
		if(_elementIsInOverlay.TryGetValue(d, out var isInOverlay) && isInOverlay)
			OnToolTipChanged?.Invoke(null, d);
		_elementIsInOverlay.Remove(d);
	}
}
