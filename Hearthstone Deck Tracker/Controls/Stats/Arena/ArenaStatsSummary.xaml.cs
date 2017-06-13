#region

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
	}
}
