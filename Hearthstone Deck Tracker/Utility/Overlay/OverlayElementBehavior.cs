#region

using System.Windows;
using System.Windows.Media;
using static System.Windows.Visibility;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Utility;
using System;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public class OverlayElementBehavior
	{
		private bool _animating;
		private double _currentScaling = 1;

		public FrameworkElement Element { get; }

		public Func<double> GetTop { get; set;  }
		public Func<double> GetRight { get; set; }
		public Func<double> GetBottom { get; set; }
		public Func<double> GetLeft { get; set;  }

		public Func<double> GetScaling { get; set; }

		public Side AnchorSide { get; set; }

		public AnimationType EntranceAnimation { get; set; }
		public AnimationType ExitAnimation { get; set; }

		public Action HideCallback { get; set; }
		public Action ShowCallback { get; set; }

		public OverlayElementBehavior(FrameworkElement element)
		{
			Element = element;
		}

		private Storyboard CreateStoryboard(AnimationType type, double to)
		{
			var animation = OverlayAnimationUtils.GetAnimation(type, to);
			if(animation == null)
				return null;
			Storyboard.SetTargetProperty(animation, new PropertyPath($"(Canvas.{AnchorSide})"));
			Storyboard.SetTarget(animation, Element);

			var sb = new Storyboard();
			sb.Children.Add(animation);
			return sb;
		}

		private double GetHiddenOffset()
		{
			switch(AnchorSide)
			{
				case Side.Top:
				case Side.Bottom:
					return -Element.ActualHeight;
				case Side.Left:
				case Side.Right:
					return -Element.ActualWidth;
				default:
					return 0;
			}
		}

		public double GetAnchorSideOffset()
		{
			switch(AnchorSide)
			{
				case Side.Top:
					return GetTop?.Invoke() ?? 0;
				case Side.Right:
					return GetRight?.Invoke() ?? 0;
				case Side.Bottom:
					return GetBottom?.Invoke() ?? 0;
				case Side.Left:
					return GetLeft?.Invoke() ?? 0;
				default:
					return 0;
			}
		}

		public void UpdatePosition()
		{
			if(_animating || !Element.IsVisible)
				return;
			if(GetTop != null)
				Canvas.SetTop(Element, GetTop());
			if(GetRight != null)
				Canvas.SetRight(Element, GetRight());
			if(GetBottom != null)
				Canvas.SetBottom(Element, GetBottom());
			if(GetLeft != null)
				Canvas.SetLeft(Element, GetLeft());
		}

		public void UpdateScaling()
		{
			if(_animating || !Element.IsVisible)
				return;
			var centerX = GetLeft == null ? Element.ActualWidth : 0;
			var centerY = GetTop == null ? Element.ActualHeight : 0;
			var scaling = GetScaling?.Invoke() ?? 1;
			if (_currentScaling != scaling)
			{
				_currentScaling = scaling;
				Element.RenderTransform = new ScaleTransform(scaling, scaling, centerX, centerY);
			}
		}

		public void Show()
		{
			if(Element.Visibility == Visible)
				return;

			var finalPosition = GetAnchorSideOffset();
			var sb = CreateStoryboard(EntranceAnimation, finalPosition);
			if(sb == null)
				return;

			sb.Completed += (obj, args) =>
			{
				_animating = false;
				ShowCallback?.Invoke();
			};

			Element.Visibility = Visible;
			Element.UpdateLayout();

			UpdateScaling();

			OverlayAnimationUtils.GetCanvasSetter(AnchorSide)?.Invoke(Element, GetHiddenOffset());

			_animating = true;
			sb.Begin();
		}

		public void Hide()
		{
			if(Element.Visibility == Collapsed)
				return;

			var sb = CreateStoryboard(ExitAnimation, GetHiddenOffset());
			sb.Completed += (obj, args) =>
			{
				_animating = false;
				Element.Visibility = Collapsed;
				HideCallback?.Invoke();
			};
			_animating = true;
			sb.Begin();
		}
	}
}
