using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class WebImportingDialog : CustomDialog
	{
		private readonly TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

		public WebImportingDialog()
		{
			InitializeComponent();
		}

		public string Url { get; set; }

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			BtnCancel.IsEnabled = false;
			_tcs.SetResult(null);
		}

		private void BtnOk_OnClick(object sender, RoutedEventArgs e)
		{
			BtnOk.IsEnabled = false;
			_tcs.SetResult(Url);
		}

		internal Task<string> WaitForButtonPressAsync() => _tcs.Task;

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
