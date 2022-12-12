using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public partial class Tier7PreLobby : UserControl
	{
		public Tier7PreLobby()
		{
			InitializeComponent();
		}

		private void Settings_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Core.MainWindow.ActivateWindow();
			Core.MainWindow.Options.TreeViewItemOverlayBattlegrounds.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
		}
	}
}
