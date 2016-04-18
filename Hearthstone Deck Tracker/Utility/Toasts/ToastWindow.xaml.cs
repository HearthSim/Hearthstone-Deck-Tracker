#region

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
	/// <summary>
	/// Interaction logic for ToastWindow.xaml
	/// </summary>
	public partial class ToastWindow : Window
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
	}
}