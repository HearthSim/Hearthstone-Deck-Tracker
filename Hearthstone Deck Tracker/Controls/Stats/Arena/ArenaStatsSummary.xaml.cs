﻿#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaStatsSummary.xaml
	/// </summary>
	public partial class ArenaStatsSummary
	{
		public ArenaStatsSummary()
		{
			InitializeComponent();
		}

		//http://stackoverflow.com/questions/3498686/wpf-remove-scrollviewer-from-treeview
		private void ForwardScrollEvent(object sender, MouseWheelEventArgs e)
		{
			if(e.Handled)
				return;
			e.Handled = true;
			var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
			var parent = ((Control)sender).Parent as UIElement;
			parent?.RaiseEvent(eventArg);
		}

		private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
		{
			e.Handled = true;
		}

		private void ScrollViewerMain_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
		{
			e.Handled = true;
		}

		private void ScrollViewer_ManipulationBoundaryFeedback_1(object sender, ManipulationBoundaryFeedbackEventArgs e)
		{
			e.Handled = true;
		}

		private void ScrollViewer_ManipulationBoundaryFeedback_2(object sender, ManipulationBoundaryFeedbackEventArgs e)
		{
			e.Handled = true;
		}

		private void ScrollViewer_ManipulationBoundaryFeedback_3(object sender, ManipulationBoundaryFeedbackEventArgs e)
		{
			e.Handled = true;
		}
	}
}
