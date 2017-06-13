using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls
{
	class TouchEnabledListView : ListView
	{
		public TouchEnabledListView()
		{
			Loaded += (sender, args) =>
			{
				var border = VisualTreeHelper.GetChild(this, 0);
				var scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				if (scrollViewer == null)
					return;
				scrollViewer.PanningMode = PanningMode.VerticalOnly;
				scrollViewer.ManipulationBoundaryFeedback += (s, a) => a.Handled = true;
			};
		}
	}
}
