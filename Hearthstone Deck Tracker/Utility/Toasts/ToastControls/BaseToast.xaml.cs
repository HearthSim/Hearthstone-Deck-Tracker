using System;
using System.Windows;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class BaseToast
	{
		public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.Register("CloseOnClick",
			typeof(bool), typeof(BaseToast), new PropertyMetadata(true));

		public event EventHandler Clicked;

		public BaseToast()
		{
			InitializeComponent();
		}

		public bool CloseOnClick
		{
			get => (bool) GetValue(CloseOnClickProperty);
			set => SetValue(CloseOnClickProperty, value);
		}

		private void BorderReplay_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(CloseOnClick)
				ToastManager.ForceCloseToast(this);
			Clicked?.Invoke(this, EventArgs.Empty);
		}

		private void BorderReplay_OnMouseEnter(object sender, MouseEventArgs e)
		{
			if(Cursor != Cursors.Wait)
				Cursor = Cursors.Hand;
		}

		private void BorderReplay_OnMouseLeave(object sender, MouseEventArgs e)
		{
			if(Cursor != Cursors.Wait)
				Cursor = Cursors.Arrow;
		}
	}
}
