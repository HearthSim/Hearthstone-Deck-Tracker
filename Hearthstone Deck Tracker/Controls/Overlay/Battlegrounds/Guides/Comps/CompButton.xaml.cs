using System;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;

public partial class CompButton : UserControl
{
	public CompButton()
	{
		InitializeComponent();
	}

	private void Comp_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { DataContext: BattlegroundsCompGuideViewModel selectedComp })
		{
			if (DataContext is BattlegroundsCompsGuidesViewModel compsGuidesViewModel)
			{
				compsGuidesViewModel.SelectedComp = selectedComp;
			}
		}
	}

	public event EventHandler? CompClicked;

	private void Button_OnClick(object sender, RoutedEventArgs e)
	{
		CompClicked?.Invoke(this, EventArgs.Empty);
	}
}

