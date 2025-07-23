using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class ArenaPreDraft : UserControl
{
	public ArenaPreDraft()
	{
		InitializeComponent();
	}

	private void Chevron_MouseUp(object sender, MouseButtonEventArgs e)
	{
		var viewModel = (ArenaPreDraftViewModel)DataContext;
		viewModel.IsCollapsed = !viewModel.IsCollapsed;
	}
}
