using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public partial class Tier7PreLobby : UserControl
	{
		public Tier7PreLobby()
		{
			InitializeComponent();
		}

		private void Chevron_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var viewModel = (Tier7PreLobbyViewModel)DataContext;
			viewModel.IsCollapsed = !viewModel.IsCollapsed;
		}
	}
}
