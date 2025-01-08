using System;
using System.Collections.Generic;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions;

public delegate void MouseIntersectionChangedEventHandler(object sender, bool intersecting);

public partial class OverlayExtensions : DependencyObject
{
	public static Dictionary<FrameworkElement, RoutedEventHandler> UnregisterCallbacks = new Dictionary<FrameworkElement, RoutedEventHandler>();

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
		if(!(d is FrameworkElement element))
			return;
		if((bool)e.NewValue)
		{
			OnRegisterHitTestVisible?.Invoke(element, true);
			UnregisterCallbacks[element] = (object sender, RoutedEventArgs args) =>
			{
				if(UnregisterCallbacks.TryGetValue(element, out var callback))
				{
					element.Unloaded -= callback;
					OnRegisterHitTestVisible?.Invoke(element, false);
					UnregisterCallbacks.Remove(element);
				}
			};
			element.Unloaded += UnregisterCallbacks[element];
		}
		else
		{
			if(UnregisterCallbacks.TryGetValue(element, out var callback))
			{
				element.Unloaded -= callback;
				UnregisterCallbacks.Remove(element);
			}
			OnRegisterHitTestVisible?.Invoke(element, false);
		}
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
		if(!(d is FrameworkElement element))
			return;
		if((bool)e.NewValue)
		{
			OnRegisterHoverVisible?.Invoke(element, true);
			UnregisterCallbacks[element] = (object sender, RoutedEventArgs args) =>
			{
				if(UnregisterCallbacks.TryGetValue(element, out var callback))
				{
					element.Unloaded -= callback;
					OnRegisterHoverVisible?.Invoke(element, false);
					UnregisterCallbacks.Remove(element);
				}
			};
			element.Unloaded += UnregisterCallbacks[element];
		}
		else
		{
			if(UnregisterCallbacks.TryGetValue(element, out var callback))
			{
				element.Unloaded -= callback;
				UnregisterCallbacks.Remove(element);
			}
			OnRegisterHoverVisible?.Invoke(element, false);
		}
	}

	#endregion
}
