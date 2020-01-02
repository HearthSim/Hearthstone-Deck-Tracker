using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class OverlayAnimationUtils
	{
		public static Timeline GetAnimation(AnimationType type, double to)
		{
			switch(type)
			{
				case AnimationType.Bounce:
					return new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(500)))
					{
						EasingFunction = new ElasticEase()
						{
							EasingMode = EasingMode.EaseOut,
							Oscillations = 2,
							Springiness = 5
						}
					};
				case AnimationType.Slide:
					return new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(200)));
					
			}

			return null;
		}

		public static Action<UIElement, double> GetCanvasSetter(Side side)
		{
			switch(side)
			{
				case Side.Top:
					return Canvas.SetTop;
				case Side.Right:
					return Canvas.SetRight;
				case Side.Bottom:
					return Canvas.SetBottom;
				case Side.Left:
					return Canvas.SetLeft;
				default:
					return null;
			}
		}
	}
}
