using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class HearthStatsLogoutInfo
	{
		public HearthStatsLogoutInfo()
		{
			InitializeComponent();
		}

		private void ButtonClose_OnClick(object sender, RoutedEventArgs e) => Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;

		private void ButtonEnableMenu_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.ShowHearthStatsMenu = true;
			Config.Save();
			Core.MainWindow.UpdateHearthStatsMenuItem();
			Core.MainWindow.FlyoutUpdateNotes.IsOpen = false;
		}
	}
}
