using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions;

public partial class OverlayExtensions : DependencyObject
{
	#region IsOverlayHitTestVisible

	public static readonly DependencyProperty IsOverlayHitTestVisibleProperty =
		DependencyProperty.RegisterAttached("IsOverlayHitTestVisible", typeof(bool), typeof(OverlayExtensions), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsOverlayHitTestVisibleChange)));

	public static bool GetIsOverlayHitTestVisible(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsOverlayHitTestVisibleProperty);
	}

	public static void SetIsOverlayHitTestVisible(DependencyObject obj, bool value)
	{
		obj.SetValue(IsOverlayHitTestVisibleProperty, value);
	}

	public static event Action<FrameworkElement, bool>? OnRegisterHitTestVisible;

	private static void OnIsOverlayHitTestVisibleChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not FrameworkElement element)
			return;
		if((bool)e.NewValue)
		{
			element.Unloaded += HitTestVisible_OnElementUnloaded;
			element.Loaded += HitTestVisible_OnElementLoaded;
			OnRegisterHitTestVisible?.Invoke(element, true);
		}
		else
		{
			element.Unloaded -= HitTestVisible_OnElementUnloaded;
			element.Loaded -= HitTestVisible_OnElementLoaded;
			OnRegisterHitTestVisible?.Invoke(element, false);
		}
	}

	private static void HitTestVisible_OnElementLoaded(object sender, RoutedEventArgs args)
	{
		if(sender is FrameworkElement e && GetIsOverlayHitTestVisible(e))
			OnRegisterHitTestVisible?.Invoke(e, true);
	}

	private static void HitTestVisible_OnElementUnloaded(object sender, RoutedEventArgs routedEventArgs)
	{
		if(sender is FrameworkElement e && GetIsOverlayHitTestVisible(e))
			OnRegisterHitTestVisible?.Invoke(e, false);
	}

	#endregion

	#region IsOverlayHoverVisible

	public static readonly DependencyProperty IsOverlayHoverVisibleProperty =
		DependencyProperty.RegisterAttached("IsOverlayHoverVisible", typeof(bool), typeof(OverlayExtensions), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsOverlayHoverVisibleChange)));

	public static bool GetIsOverlayHoverVisible(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsOverlayHoverVisibleProperty);
	}

	public static void SetIsOverlayHoverVisible(DependencyObject obj, bool value)
	{
		obj.SetValue(IsOverlayHoverVisibleProperty, value);
	}

	public static event Action<FrameworkElement, bool>? OnRegisterHoverVisible;

	private static void OnIsOverlayHoverVisibleChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not FrameworkElement element)
			return;
		if((bool)e.NewValue)
		{
			element.Unloaded += HoverVisible_OnElementUnloaded;
			element.Loaded += HoverVisible_OnElementLoaded;
			OnRegisterHoverVisible?.Invoke(element, true);
		}
		else
		{
			element.Unloaded -= HoverVisible_OnElementUnloaded;
			element.Loaded -= HoverVisible_OnElementLoaded;
			OnRegisterHoverVisible?.Invoke(element, false);
		}
	}

	private static void HoverVisible_OnElementLoaded(object sender, RoutedEventArgs args)
	{
		if(sender is FrameworkElement e && GetIsOverlayHoverVisible(e))
			OnRegisterHoverVisible?.Invoke(e, true);
	}

	private static void HoverVisible_OnElementUnloaded(object sender, RoutedEventArgs routedEventArgs)
	{
		if(sender is FrameworkElement e && GetIsOverlayHoverVisible(e))
			OnRegisterHoverVisible?.Invoke(e, false);
	}


	#endregion
}
