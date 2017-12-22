#region

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
	public partial class ToastWindow
	{
		private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

		public ToastWindow(UserControl control)
		{
			InitializeComponent();
			ContentControl.Content = control;
		}

		public Task FadeOut()
		{
			var sb = (Storyboard)FindResource("StoryboardFadeOut");
			sb.Completed += (sender, args) =>
			{
				_tcs.SetResult(null);
				Close();
			};
			sb.Begin(this);
			return _tcs.Task;
		}

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			var hwnd = new WindowInteropHelper(this).Handle;
			User32.SetWindowExStyle(hwnd, User32.WsExToolWindow);
		}
	}
}
