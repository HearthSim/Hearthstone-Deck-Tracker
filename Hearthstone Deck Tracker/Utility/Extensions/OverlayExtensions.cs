using System;
using System.Collections.Generic;
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
			element.Unloaded += (_, _) =>
			{
				if(GetIsOverlayHitTestVisible(element))
					OnRegisterHitTestVisible?.Invoke(element, false);
			};
			element.Loaded += (_, _) =>
			{
				if(GetIsOverlayHitTestVisible(element))
					OnRegisterHitTestVisible?.Invoke(element, true);
			};
			OnRegisterHitTestVisible?.Invoke(element, true);
		}
		else
			OnRegisterHitTestVisible?.Invoke(element, false);
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
			element.Unloaded += (_, _) =>
			{
				if(GetIsOverlayHoverVisible(element))
					OnRegisterHoverVisible?.Invoke(element, false);
			};
			element.Loaded += (_, _) =>
			{
				if(GetIsOverlayHoverVisible(element))
					OnRegisterHoverVisible?.Invoke(element, true);
			};
			OnRegisterHoverVisible?.Invoke(element, true);
		}
		else
			OnRegisterHoverVisible?.Invoke(element, false);
	}

	#endregion
}
