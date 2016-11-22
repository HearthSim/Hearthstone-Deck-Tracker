using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class ImportingChoiceDialog : CustomDialog
	{
		private readonly TaskCompletionSource<ImportingChoice?> _tcs = new TaskCompletionSource<ImportingChoice?>();

		public ImportingChoiceDialog()
		{
			InitializeComponent();
		}

		internal Task<ImportingChoice?> WaitForButtonPressAsync() => _tcs.Task;

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e) => CloseDialog(null);

		private void BtnExport_OnClick(object sender, RoutedEventArgs e) => CloseDialog(ImportingChoice.ExportToHs);

		private void BtnSaveLocal_OnClick(object sender, RoutedEventArgs e) => CloseDialog(ImportingChoice.SaveLocal);

		private void CloseDialog(ImportingChoice? choice)
		{
			BtnExport.IsEnabled = false;
			BtnSaveLocal.IsEnabled = false;
			BtnCancel.IsEnabled = false;
			_tcs.SetResult(choice);
		}
	}
}
