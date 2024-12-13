using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions;

partial class OverlayExtensions
{
	public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached("ToolTip", typeof(FrameworkElement), typeof(OverlayExtensions), new FrameworkPropertyMetadata(null, OnToolTipChange));

	public static FrameworkElement GetToolTip(DependencyObject obj) => (FrameworkElement)obj.GetValue(ToolTipProperty);

	/// <summary>
	/// Renders the value as a ToolTip in the OverlayWindow. Can be configured via <c>ToolTipService</c>.<br/>
	/// Supported Properties:<br/>
	/// - <c>ToolTipService.Placement</c> (Left, Top, Bottom, Right) - Default: Right
	/// </summary>
	public static void SetToolTip(DependencyObject obj, FrameworkElement element) => obj.SetValue(ToolTipProperty, element);

	private static readonly Dictionary<FrameworkElement, bool> _elementIsInOverlay = new ();

	private static bool IsInOverlay(FrameworkElement element)
	{
		if(!_elementIsInOverlay.TryGetValue(element, out var val))
		{
			val = Helper.GetVisualParent<Window>(element) is OverlayWindow;
			_elementIsInOverlay[element] = val;
		}
		return val;
	}

	public static event Action<FrameworkElement?, FrameworkElement>? OnToolTipChanged;

	private static void ShowTooltip(object sender, MouseEventArgs _)
	{
		if(sender is FrameworkElement element && IsInOverlay(element))
			OnToolTipChanged?.Invoke(GetToolTip(element), element);
	}

	private static void HideTooltip(object sender, MouseEventArgs _)
	{
		if(sender is FrameworkElement element && IsInOverlay(element))
			OnToolTipChanged?.Invoke(null, element);
	}

	private static void OnToolTipChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not FrameworkElement element)
			return;

		if(e is { OldValue: FrameworkElement, NewValue: FrameworkElement })
			return;

		if(e.NewValue is FrameworkElement)
		{
			element.MouseEnter += ShowTooltip;
			element.MouseLeave += HideTooltip;
		}
		else
		{
			element.MouseEnter -= ShowTooltip;
			element.MouseLeave -= HideTooltip;
			OnToolTipChanged?.Invoke(null, element);
		}
	}
}
