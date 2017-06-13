using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls
{
	class TouchEnabledScrollViewer : ScrollViewer
	{
		public TouchEnabledScrollViewer()
		{
			PanningMode = PanningMode.VerticalOnly;
			ManipulationBoundaryFeedback += (sender, args) => args.Handled = true;
		}
	}
}
