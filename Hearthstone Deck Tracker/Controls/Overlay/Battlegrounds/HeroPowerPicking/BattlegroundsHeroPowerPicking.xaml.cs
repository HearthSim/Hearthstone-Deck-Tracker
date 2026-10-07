using System.Windows;
using Hearthstone_Deck_Tracker.Utility;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPowerPicking;

public sealed partial class BattlegroundsHeroPowerPicking
{
	public BattlegroundsHeroPowerPicking()
	{
		InitializeComponent();
	}

	private void OverlayVisibilityToggle_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is BattlegroundsHeroPowerPickingViewModel viewModel)
		{
			var newVisibility = viewModel.StatsVisibility == Visibility.Visible
				? Visibility.Collapsed
				: Visibility.Visible;
			viewModel.StatsVisibility = newVisibility;
			ConfigWrapper.ShowBattlegroundsHeroPicking = newVisibility == Visibility.Visible;
		}
	}
}
