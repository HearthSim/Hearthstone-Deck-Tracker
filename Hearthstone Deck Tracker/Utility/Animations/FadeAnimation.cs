using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Utility.Animations
{
	public class FadeAnimation
	{
		private static readonly Duration DefaultDuration = new(TimeSpan.FromMilliseconds(200));
		private const Direction DefaultDirection = Direction.Down;

		private static readonly Dictionary<FrameworkElement, Direction> _direction = new();
		private static readonly Dictionary<FrameworkElement, int> _distance = new();
		private static readonly Dictionary<FrameworkElement, Duration> _duration = new();

		private static readonly Dictionary<FrameworkElement, Storyboard> _currentStoryboard = new();

		private static DoubleAnimation GetTransformAnimation(FrameworkElement element, bool wasVisible, Direction direction, int distance, Duration duration)
		{
			double? offset = null; 
			if(element.RenderTransform is TranslateTransform tt)
			{
				var isX = direction == Direction.Right || direction == Direction.Left;
				var value = isX ? tt.X : tt.Y;
				if(value != 0)
					offset = value;
			}
			var transform = new TranslateTransform(0, 0);
			element.RenderTransform = transform;

			if(offset == null)
				offset = distance * ((direction == Direction.Right || direction == Direction.Down) ? -1.0 : 1.0);

			var (offsetFrom, offsetTo) = wasVisible ? (offset.Value, 0.0) : (0.0, offset.Value);
			var translateAnim = new DoubleAnimation(offsetTo, offsetFrom, duration);
				var axis = direction == Direction.Right || direction == Direction.Left ? "X" : "Y";

			Storyboard.SetTargetProperty(translateAnim, new PropertyPath($"RenderTransform.(TranslateTransform.{axis})"));
			Storyboard.SetTarget(translateAnim, element);

			return translateAnim;
		}

		#region Visibility

		public static readonly DependencyProperty VisibilityProperty =
			DependencyProperty.RegisterAttached("Visibility", typeof(Visibility), typeof(FadeAnimation), new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnVisibilityChange)));

		public static Visibility GetVisibility(DependencyObject obj) => (Visibility)obj.GetValue(VisibilityProperty);

		public static void SetVisibility(DependencyObject obj, Visibility value) => obj.SetValue(VisibilityProperty, value);

		private static void OnVisibilityChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(!(d is FrameworkElement element))
				return;

			if(_currentStoryboard.TryGetValue(element, out var current))
			{
				current.Stop();
				_currentStoryboard.Remove(element);
			}

			var oldVisibility = (Visibility)e.OldValue;
			var newVisibility = (Visibility)e.NewValue;
			var wasVisible = oldVisibility == Visibility.Visible;

			var sb = new Storyboard();
			sb.FillBehavior = FillBehavior.Stop;

			var (opacityFrom, opacityTo) = wasVisible ? (element.Opacity, 0) : (element.Opacity, 1);
			var duration = _duration.TryGetValue(element, out var durationVal) ? durationVal : DefaultDuration;
			var opacityAnim = new DoubleAnimation(opacityFrom, opacityTo, duration);
			Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
			Storyboard.SetTarget(opacityAnim, element);
			sb.Children.Add(opacityAnim);

			if(_distance.TryGetValue(element, out var distance))
			{
				var direction = _direction.TryGetValue(element, out var dirVal) ? dirVal : DefaultDirection;
				sb.Children.Add(GetTransformAnimation(element, wasVisible, direction, distance, duration));
			}

			sb.Completed += (_, _) =>
			{
				element.Visibility = newVisibility;
				element.Opacity = opacityTo;

				if(_currentStoryboard[element] == sb)
					_currentStoryboard.Remove(element);
			};

			element.Opacity = opacityFrom;

			// If we are animating hidden => visible we need to first make the element visible.
			if(oldVisibility != Visibility.Visible)
				element.Visibility = Visibility.Visible;

			_currentStoryboard[element] = sb;
			sb.Begin();
		}

		#endregion

		#region Direction

		public static readonly DependencyProperty DirectionProperty =
			DependencyProperty.RegisterAttached("Direction", typeof(Direction), typeof(FadeAnimation), new FrameworkPropertyMetadata(DefaultDirection, new PropertyChangedCallback(OnDirectionChange)));

		public static Direction GetDirection(DependencyObject obj) => (Direction)obj.GetValue(DirectionProperty);

		public static void SetDirection(DependencyObject obj, Direction value) => obj.SetValue(DirectionProperty, value);

		private static void OnDirectionChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is FrameworkElement element)
				_direction[element] = (Direction)e.NewValue;
		}

		public enum Direction
		{
			Up,
			Down,
			Left,
			Right,
		}

		#endregion

		#region Distance

		public static readonly DependencyProperty DistanceProperty =
			DependencyProperty.RegisterAttached("Distance", typeof(int), typeof(FadeAnimation), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnDistanceChange)));

		public static int GetDistance(DependencyObject obj) => (int)obj.GetValue(DistanceProperty);

		public static void SetDistance(DependencyObject obj, int value) => obj.SetValue(DistanceProperty, value);

		private static void OnDistanceChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is FrameworkElement element)
				_distance[element] = (int)e.NewValue;
		}

		#endregion

		#region Duration

		public static readonly DependencyProperty DurationProperty =
			DependencyProperty.RegisterAttached("Duration", typeof(Duration), typeof(FadeAnimation), new FrameworkPropertyMetadata(DefaultDuration, new PropertyChangedCallback(OnDurationChange)));

		public static Duration GetDuration(DependencyObject obj) => (Duration)obj.GetValue(DurationProperty);

		public static void SetDuration(DependencyObject obj, Duration value) => obj.SetValue(DurationProperty, value);

		private static void OnDurationChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is FrameworkElement element)
				_duration[element] = (Duration)e.NewValue;
		}

		#endregion
	}
}
