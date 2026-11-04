using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class PluginInstallDialog : CustomDialog
	{
		private readonly TaskCompletionSource<bool> _tcs = new();
		private readonly bool _requireAcknowledgement;

		public PluginInstallDialog(string title, bool requireAcknowledgement)
		{
			InitializeComponent();
			Title = title;
			_requireAcknowledgement = requireAcknowledgement;
			if(requireAcknowledgement)
			{
				TextBlockDesc.Visibility = Visibility.Collapsed;
				TextBlockDescAdmin.Visibility = Visibility.Visible;
				CheckBoxAcknowledge.Visibility = Visibility.Visible;
			}
			UpdateInstallEnabled();
			Loaded += (_, _) => BtnCancel.Focus();
		}

		internal Task<bool> WaitForButtonPressAsync() => _tcs.Task;

		private void UpdateInstallEnabled() => BtnInstall.IsEnabled = !_requireAcknowledgement || CheckBoxAcknowledge.IsChecked == true;

		private void CheckBoxAcknowledge_OnChanged(object sender, RoutedEventArgs e) => UpdateInstallEnabled();

		private void BtnInstall_OnClick(object sender, RoutedEventArgs e) => CloseDialog(true);

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e) => CloseDialog(false);

		private void CloseDialog(bool install)
		{
			BtnInstall.IsEnabled = false;
			BtnCancel.IsEnabled = false;
			CheckBoxAcknowledge.IsEnabled = false;
			_tcs.TrySetResult(install);
		}
	}
}
