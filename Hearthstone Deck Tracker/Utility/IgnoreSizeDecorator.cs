using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class IgnoreSizeDecorator : Decorator
	{
		protected override Size MeasureOverride(Size constraint)
		{
			Child.Measure(new Size(0, 0));
			return new Size(0, 0);
		}
	}
}
