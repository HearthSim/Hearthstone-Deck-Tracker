using System.Windows;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class PortableDiscontinued
	{
		public PortableDiscontinued()
		{
			InitializeComponent();
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
		{
			if(this.ParentMainWindow() is { } window)
				window.FlyoutPortableDiscontinued.IsOpen = false;
		}
	}
}
