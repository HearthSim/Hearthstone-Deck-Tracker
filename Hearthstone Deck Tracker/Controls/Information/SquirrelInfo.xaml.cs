using System.Threading.Tasks;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class SquirrelInfo
	{
		public SquirrelInfo()
		{
			InitializeComponent();
		}

		private async void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
		{
			TabControl.SelectedIndex = 1;
			if(!Helper.TryOpenUrl("https://hsdecktracker.net/download/"))
				TextBlockManualDownload.Visibility = Visibility.Visible;
			else
			{
				await Task.Delay(500);
				Core.MainWindow.ActivateWindow();
			}
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;

		private void ButtonClose_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.Close();

		private void ButtonBack_OnClick(object sender, RoutedEventArgs e) => TabControl.SelectedIndex = 0;
	}
}
