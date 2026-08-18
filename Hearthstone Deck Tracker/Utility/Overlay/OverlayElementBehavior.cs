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
		private bool _hiding;
		private AnimationType? _hidingAnimation;
		private int _transition;
		private double _restingOpacity = 1;
		private double _currentScaling = double.NaN;
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
			if(!Element.IsVisible)
				return;

			// scale around the anchored corner. A relative origin follows the element through every
			// layout pass, an absolute CenterX/CenterY would keep using the size it was last given here
			Element.RenderTransformOrigin = new Point(GetLeft == null ? 1 : 0, GetTop == null ? 1 : 0);

			var scaling = GetScaling?.Invoke() ?? 1;
			if(_currentScaling == scaling)
				return;
			_currentScaling = scaling;

			var transform = new ScaleTransform(scaling, scaling);
			Element.RenderTransform = transform;

			// To automatically scale tooltips, any tooltip styles need to
			// be defined in the Elements ResourceDictionary. This will not
			// work if any styled are defined in nested elements.
			var tooltipStyle = new Style(typeof(ToolTip), _baseTooltipStyle);
			tooltipStyle.Setters.Add(new Setter(FrameworkElement.LayoutTransformProperty, transform));
			Element.Resources[typeof(ToolTip)] = tooltipStyle;
		}

		// Call this after swapping the content of an element that stays visible, so its position is
		// measured against the size the new content takes up.
		public void Refresh()
		{
			if(_animating || !Element.IsVisible)
				return;
			Element.UpdateLayout();
			UpdateScaling();
			UpdatePosition();
		}

		public void Show()
		{
			// Hide only collapses the element once its animation completes, so a Show during that
			// window has to interrupt it rather than treat the element as already shown
			if(Element.Visibility == Visible && !_hiding)
				return;

			// a zero-duration storyboard never raises Completed, so apply the final state directly
			if(EntranceAnimation == AnimationType.Instant)
			{
				var restingOpacity = CaptureRestingOpacity();
				++_transition;
				_animating = false;
				_hiding = false;
				_hidingAnimation = null;
				Element.Visibility = Visible;
				Element.UpdateLayout();
				UpdateScaling();
				UpdatePosition();
				Element.Opacity = restingOpacity;
				ShowCallback?.Invoke();
				return;
			}

			var finalPosition = GetAnchorSideOffset();
			var sb = CreateStoryboard(EntranceAnimation, finalPosition, Fade ? 1 : null);
			if(sb == null)
				return;

			var opacity = CaptureRestingOpacity();
			var transition = ++_transition;
			_hiding = false;
			_hidingAnimation = null;

			sb.Completed += (obj, args) =>
			{
				if(transition != _transition)
					return;
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

		// while an animation runs Element.Opacity reads the animated value, not the one to restore
		private double CaptureRestingOpacity()
		{
			if(!_animating)
				_restingOpacity = Element.Opacity;
			return _restingOpacity;
		}

		// keep the parameterless overload, plugins are compiled against it
		public void Hide() => Hide(null);

		public void Hide(AnimationType? animation)
		{
			if(Element.Visibility == Collapsed)
				return;

			// the overlay update calls this on every tick, and restarting the animation each time
			// would keep pushing back the point where the element actually collapses
			var exitAnimation = animation ?? ExitAnimation;
			if(_hiding && _hidingAnimation == exitAnimation)
				return;

			// a zero-duration storyboard never raises Completed, so apply the final state directly
			if(exitAnimation == AnimationType.Instant)
			{
				CaptureRestingOpacity();
				++_transition;
				_animating = false;
				_hiding = false;
				_hidingAnimation = null;
				Element.Visibility = Collapsed;
				HideCallback?.Invoke();
				return;
			}

			var sb = CreateStoryboard(exitAnimation, GetHiddenOffset(), Fade ? 0 : null);
			if(sb == null)
				return;

			CaptureRestingOpacity();
			var transition = ++_transition;
			_hiding = true;
			_hidingAnimation = exitAnimation;

			sb.Completed += (obj, args) =>
			{
				if(transition != _transition)
					return;
				_animating = false;
				_hiding = false;
				_hidingAnimation = null;
				Element.Visibility = Collapsed;
				HideCallback?.Invoke();
				UpdatePosition();
			};
			_animating = true;
			sb.Begin();
		}
	}
}
