using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class DeckTypeDialog : CustomDialog
	{
		private readonly TaskCompletionSource<DeckType?> _tcs = new TaskCompletionSource<DeckType?>();

		public DeckTypeDialog()
		{
			InitializeComponent();
		}

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			BtnCancel.IsEnabled = false;
			_tcs.SetResult(null);
		}

		private void BtnOk_OnClick(object sender, RoutedEventArgs e)
		{
			BtnOk.IsEnabled = false;
			if(RadioButtonArena.IsChecked == true)
				_tcs.SetResult(DeckType.Arena);
			else if(RadioButtonBrawl.IsChecked == true)
				_tcs.SetResult(DeckType.Brawl);
			else
				_tcs.SetResult(DeckType.Constructed);
		}

		internal Task<DeckType?> WaitForButtonPressAsync() => _tcs.Task;
	}

	public enum DeckType
	{
		Arena,
		Brawl,
		Constructed
	}
}