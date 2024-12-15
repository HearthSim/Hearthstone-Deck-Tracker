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
	public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached("ToolTip", typeof(FrameworkElement), typeof(OverlayExtensions), new FrameworkPropertyMetadata(null, OnToolTipChange));

	public static FrameworkElement GetToolTip(DependencyObject obj) => (FrameworkElement)obj.GetValue(ToolTipProperty);

	/// <summary>
	/// Renders the value as a ToolTip in the OverlayWindow. Can be configured via <c>ToolTipService</c>.<br/>
	/// Supported Properties:<br/>
	/// - <c>ToolTipService.Placement</c> (Left, Top, Bottom, Right) - Default: Right<br/>
	/// - <c>ToolTipService.InitialShowDelay</c> Default: 0<br/>
	/// - <c>ToolTipService.HorizontalOffset</c> Default: 0<br/>
	/// - <c>ToolTipService.VerticalOffset</c> Default: 0
	/// </summary>
	public static void SetToolTip(DependencyObject obj, DependencyObject element) => obj.SetValue(ToolTipProperty, element);

	private static readonly Dictionary<DependencyObject, bool> _elementIsInOverlay = new ();

	private static bool IsInOverlay(DependencyObject d)
	{
		if(!_elementIsInOverlay.TryGetValue(d, out var val))
		{
			var visual = d as Visual ?? Helper.GetLogicalParent<Visual>(d);
			val = visual != null && Helper.GetVisualParent<Window>(visual) is OverlayWindow;
			_elementIsInOverlay[d] = val;
		}
		return val;
	}

	public static event Action<FrameworkElement?, DependencyObject>? OnToolTipChanged;

	private static void ShowTooltip(object sender, MouseEventArgs _)
	{
		if(sender is DependencyObject d && IsInOverlay(d))
			OnToolTipChanged?.Invoke(GetToolTip(d), d);
	}

	private static void HideTooltip(object sender, MouseEventArgs _)
	{
		if(sender is DependencyObject d && IsInOverlay(d))
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
		element.ToolTip = new ToolTip
		{
			Content = GetToolTip(element),
			Background = Brushes.Transparent,
			BorderBrush = Brushes.Transparent
		};
	}

	private static void OnToolTipChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not IInputElement inputElement)
			return;

		if(e is { OldValue: IInputElement, NewValue: IInputElement })
			return;

		if(e.NewValue is IInputElement)
		{
			inputElement.MouseEnter += ShowTooltip;
			inputElement.MouseLeave += HideTooltip;

			if(d is FrameworkContentElement fce)
				fce.Loaded += OnElementLoaded;
			else
			{
				var fe = d as FrameworkElement ?? Helper.GetLogicalParent<FrameworkElement>(d);
				if(fe != null)
					fe.Loaded += OnElementLoaded;
			}
		}
		else
		{
			inputElement.MouseEnter -= ShowTooltip;
			inputElement.MouseLeave -= HideTooltip;

			if(d is FrameworkContentElement fce)
				fce.Loaded -= OnElementLoaded;
			else
			{
				var fe = d as FrameworkElement ?? Helper.GetLogicalParent<FrameworkElement>(d);
				if(fe != null)
					fe.Loaded -= OnElementLoaded;
			}

			OnToolTipChanged?.Invoke(null, d);
			_elementIsInOverlay.Remove(d);
		}
	}
}
