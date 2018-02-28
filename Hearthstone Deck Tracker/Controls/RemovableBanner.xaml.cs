using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class RemovableBanner
	{
		public event EventHandler Click;
		public event EventHandler Close;

		public RemovableBanner()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
			e.Handled = true;
		}

		private void Banner_Click(object sender, RoutedEventArgs e)
			=> Click?.Invoke(this, EventArgs.Empty);
	}
}
