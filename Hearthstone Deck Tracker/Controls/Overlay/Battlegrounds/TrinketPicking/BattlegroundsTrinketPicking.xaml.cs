using System.Windows;
using Hearthstone_Deck_Tracker.Utility;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.TrinketPicking;

public sealed partial class BattlegroundsTrinketPicking
{
	public BattlegroundsTrinketPicking()
	{
		InitializeComponent();
	}

	private void OverlayVisibilityToggle_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is BattlegroundsTrinketPickingViewModel viewModel)
		{
			var newVisibility = viewModel.StatsVisibility == Visibility.Visible
				? Visibility.Collapsed
				: Visibility.Visible;
			viewModel.StatsVisibility = newVisibility;
			ConfigWrapper.AutoShowBattlegroundsTrinketStats = newVisibility == Visibility.Visible;
		}
	}
}
