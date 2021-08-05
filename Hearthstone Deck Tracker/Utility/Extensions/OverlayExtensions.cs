using System;
using System.Collections.Generic;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public class OverlayExtensions : DependencyObject
	{
		public static Dictionary<FrameworkElement, RoutedEventHandler> UnregisterCallbacks = new Dictionary<FrameworkElement, RoutedEventHandler>();

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

		public static event Action<FrameworkElement, bool> OnRegisterHitTestVisible;

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
	}
}
