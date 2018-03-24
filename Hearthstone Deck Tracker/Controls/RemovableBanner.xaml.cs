using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class RemovableBanner
	{
		public static readonly DependencyProperty RemovableProperty =
			DependencyProperty.Register("Removable", typeof(bool), typeof(RemovableBanner), new PropertyMetadata(true));
		public event EventHandler Click;
		public event EventHandler Close;

		public RemovableBanner()
		{
			InitializeComponent();
		}

		public bool Removable
		{
			get => (bool) GetValue(RemovableProperty);
			set => SetValue(RemovableProperty, value);
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
