#region

using System.Windows;
using System.Windows.Media;
using static System.Windows.Visibility;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Utility;
using System;
using System.Collections;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public class OverlayElementBehavior
	{
		private bool _animating;
		private double _currentScaling = 1;
		private double _currentCenterX = 1;
		private double _currentCenterY = 1;
		private Style? _baseTooltipStyle = null;

		public FrameworkElement Element { get; }

		public Func<double>? GetTop { get; set;  }
		public Func<double>? GetRight { get; set; }
		public Func<double>? GetBottom { get; set; }
		public Func<double>? GetLeft { get; set;  }

		public Func<double>? GetScaling { get; set; }

		public Side AnchorSide { get; set; }

		public AnimationType EntranceAnimation { get; set; }
		public AnimationType ExitAnimation { get; set; }
		public bool Fade { get; set; }
		public double? Distance { get; set; }

		public Action? HideCallback { get; set; }
		public Action? ShowCallback { get; set; }

		public OverlayElementBehavior(FrameworkElement element)
		{
			Element = element;
			foreach(var res in Element.Resources)
			{
				if(res is not DictionaryEntry entry)
					continue;
				if(entry.Value is not Style style)
					continue;
				if(style.TargetType != typeof(ToolTip))
					continue;
				_baseTooltipStyle = style;
				break;
			}
		}

		private Storyboard? CreateStoryboard(AnimationType type, double targetPos, double? targetOpacity)
		{
			var animation = OverlayAnimationUtils.GetAnimation(type, targetPos);
			if(animation == null)
				return null;
			Storyboard.SetTargetProperty(animation, new PropertyPath($"(Canvas.{AnchorSide})"));
			Storyboard.SetTarget(animation, Element);

			var sb = new Storyboard();
			sb.FillBehavior = FillBehavior.Stop;
			sb.Children.Add(animation);

			if(Fade && targetOpacity.HasValue)
			{
				var fade = OverlayAnimationUtils.GetAnimation(type, targetOpacity.Value);
				Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
				Storyboard.SetTarget(fade, Element);
				sb.Children.Add(fade);
			}

			return sb;
		}

		private double GetHiddenOffset()
		{
			if(Distance.HasValue)
				return GetAnchorSideOffset() - Distance.Value;

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
			if (_currentScaling != scaling || centerX != _currentCenterX || centerY != _currentCenterY)
			{
				_currentScaling = scaling;
				_currentCenterX = centerX;
				_currentCenterY = centerY;
				var transform = new ScaleTransform(scaling, scaling, centerX, centerY);
				Element.RenderTransform = transform;

				// To automatically scale tooltips, any tooltip styles need to
				// be defined in the Elements ResourceDictionary. This will not
				// work if any styled are defined in nested elements.
				var tooltipStyle = new Style(typeof(ToolTip), _baseTooltipStyle);
				tooltipStyle.Setters.Add(new Setter(FrameworkElement.LayoutTransformProperty, transform));
				Element.Resources[typeof(ToolTip)] = tooltipStyle;
			}
		}

		public void Show()
		{
			if(Element.Visibility == Visible)
				return;

			var finalPosition = GetAnchorSideOffset();
			var sb = CreateStoryboard(EntranceAnimation, finalPosition, Fade ? 1 : null);
			if(sb == null)
				return;

			var opacity = Element.Opacity;

			sb.Completed += (obj, args) =>
			{
				_animating = false;
				ShowCallback?.Invoke();
				if(Fade)
					Element.Opacity = opacity;
				UpdatePosition();
			};

			var hitTestVisible = Element.IsHitTestVisible;
			Element.Opacity = 0;
			Element.IsHitTestVisible = false;

			Element.Visibility = Visible;
			Element.UpdateLayout();
			UpdateScaling();
			UpdatePosition();

			OverlayAnimationUtils.GetCanvasSetter(AnchorSide)?.Invoke(Element, GetHiddenOffset());

			if(!Fade)
				Element.Opacity = opacity;
			Element.IsHitTestVisible = hitTestVisible;

			_animating = true;
			sb.Begin();
		}

		public void Hide()
		{
			if(Element.Visibility == Collapsed)
				return;

			var sb = CreateStoryboard(ExitAnimation, GetHiddenOffset(), Fade ? 0 : null);
			if(sb == null)
				return;
			sb.Completed += (obj, args) =>
			{
				_animating = false;
				Element.Visibility = Collapsed;
				HideCallback?.Invoke();
				UpdatePosition();
			};
			_animating = true;
			sb.Begin();
		}
	}
}
