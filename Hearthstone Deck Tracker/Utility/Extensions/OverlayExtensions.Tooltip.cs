using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached("ToolTip", typeof(FrameworkElement), typeof(OverlayExtensions), new FrameworkPropertyMetadata(null, OnToolTipChange));

	public static FrameworkElement GetToolTip(DependencyObject obj) => (FrameworkElement)obj.GetValue(ToolTipProperty);

	/// <summary>
	/// Renders the value as a ToolTip in the OverlayWindow. Can be configured via <c>ToolTipService</c>.<br/>
	/// Supported Properties:<br/>
	/// - <c>ToolTipService.IsEnabled</c> Default: true<br/>
	/// - <c>ToolTipService.Placement</c> (Left, Top, Bottom, Right) - Default: Right<br/>
	/// - <c>ToolTipService.InitialShowDelay</c> Default: 0<br/>
	/// - <c>ToolTipService.HorizontalOffset</c> Default: 0<br/>
	/// - <c>ToolTipService.VerticalOffset</c> Default: 0
	/// </summary>
	public static void SetToolTip(DependencyObject obj, DependencyObject element) => obj.SetValue(ToolTipProperty, element);

	private static readonly Dictionary<DependencyObject, bool> _elementIsInOverlay = new ();
	private static readonly HashSet<DependencyObject> _waitingOnVisualTree = new ();

	private static async Task<bool> IsInOverlay(DependencyObject d)
	{
		if(!_elementIsInOverlay.TryGetValue(d, out var val))
		{
			val = await GetParentWindow(d) is OverlayWindow;
			_elementIsInOverlay[d] = val;
		}
		return val;
	}

	private static async Task<Window?> GetParentWindow(DependencyObject d)
	{
		var visual = d as Visual ?? Helper.GetLogicalParent<Visual>(d);
		if(visual == null)
			return null;
		var window = Helper.GetVisualParent<Window>(visual);
		if(window != null)
			return window;

		_waitingOnVisualTree.Add(visual);
		while(window == null)
		{
			// This is not great. In theory the visual tree should be constructed by the time element.Loaded is invoked,
			// or at least when window.Loaded is invoked. Neither seems to be the case. Using the dispatcher here with
			// a low priority invoke seems to come with the same issues where it still can't find the window on the first
			// try. Might as well just use a time delay.
			await Task.Delay(10);

			if(!_waitingOnVisualTree.Contains(visual))
				break;
			window = Helper.GetVisualParent<Window>(visual);
		}
		_waitingOnVisualTree.Remove(visual);
		return window;
	}

	public static event Action<FrameworkElement?, DependencyObject>? OnToolTipChanged;

	private static async void ShowTooltip(object sender, MouseEventArgs _)
	{
		if(sender is DependencyObject d && await IsInOverlay(d))
			OnToolTipChanged?.Invoke(GetToolTip(d), d);
	}

	private static async void HideTooltip(object sender, MouseEventArgs _)
	{
		if(sender is DependencyObject d && await IsInOverlay(d))
			OnToolTipChanged?.Invoke(null, d);
	}

	private static async void OnElementLoaded(object sender, RoutedEventArgs e)
	{
		if(sender is not FrameworkElement element)
			return;
		if(await IsInOverlay(element) || element.ToolTip is not null)
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

	private static void OnElementUnloaded(object sender, RoutedEventArgs routedEventArgs)
	{
		if(sender is DependencyObject d)
			UnregisterToolTip(d);
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
			UnregisterToolTip(d);
	}

	private static void UnregisterToolTip(DependencyObject d)
	{
		if(d is not IInputElement inputElement)
			return;
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

		if(_elementIsInOverlay.TryGetValue(d, out var isInOverlay) && isInOverlay)
			OnToolTipChanged?.Invoke(null, d);
		_elementIsInOverlay.Remove(d);
		_waitingOnVisualTree.Remove(d);
	}
}
