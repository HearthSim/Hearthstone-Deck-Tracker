using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls;

/// <summary>
/// ScrollViewer Wrapper that only processes MouseWheel events if the ScrollViewer
/// is currently scrollable. This allows for better working nested ScrollViewers
/// where lower level ones may not always be scrollable.
/// </summary>
public class PassiveScrollViewer : ScrollViewer
{
	private bool IsScrollable => ExtentHeight > ViewportHeight;

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		if(IsScrollable)
			base.OnMouseWheel(e);
	}
}
