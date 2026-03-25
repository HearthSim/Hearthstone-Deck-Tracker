using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public partial class ConstructedMulliganPreLobbyWidget : UserControl
{
	public ConstructedMulliganPreLobbyWidget()
	{
		InitializeComponent();
	}

	private void Chevron_MouseUp(object sender, MouseButtonEventArgs e)
	{
		var viewModel = (ConstructedMulliganPreLobbyWidgetViewModel)DataContext;
		viewModel.IsCollapsed = !viewModel.IsCollapsed;
	}
}

