using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls
{
	class TouchEnabledListView : ListView
	{
		public static readonly DependencyProperty CanContentScrollProperty = DependencyProperty.Register("CanContentScroll", typeof(bool), typeof(TouchEnabledListView), new PropertyMetadata(false));

		public bool CanContentScroll
		{
			get => (bool)GetValue(CanContentScrollProperty);
			set => SetValue(CanContentScrollProperty, value);
		}

		public TouchEnabledListView()
		{
			Loaded += (sender, args) =>
			{
				var border = VisualTreeHelper.GetChild(this, 0);
				var scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
				if (scrollViewer == null)
					return;
				scrollViewer.CanContentScroll = CanContentScroll; // This may cause performance issues while scrolling on OLD touch devices when set to FALSE.
				scrollViewer.PanningMode = PanningMode.VerticalOnly;
				scrollViewer.ManipulationBoundaryFeedback += (s, a) => a.Handled = true;
			};
		}
	}
}
