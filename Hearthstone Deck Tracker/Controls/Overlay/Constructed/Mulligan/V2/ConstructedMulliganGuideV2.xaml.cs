using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan.V2;

public partial class ConstructedMulliganGuideV2 : UserControl
{
	public ConstructedMulliganGuideV2()
	{
		InitializeComponent();
	}

	private void OverlayVisibilityToggle_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is ConstructedMulliganGuideV2ViewModel viewModel)
		{
			var newVisibility = viewModel.StatsVisibility == Visibility.Visible
				? Visibility.Collapsed
				: Visibility.Visible;
			viewModel.StatsVisibility = newVisibility;
			ConfigWrapper.AutoShowMulliganGuide = newVisibility == Visibility.Visible;
		}
	}
}

