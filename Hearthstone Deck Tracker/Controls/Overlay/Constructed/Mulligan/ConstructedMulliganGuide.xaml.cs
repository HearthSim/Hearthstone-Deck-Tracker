using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan
{
	public sealed partial class ConstructedMulliganGuide
	{
		public ConstructedMulliganGuide()
		{
			InitializeComponent();
		}

		private void OverlayVisibilityToggle_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (DataContext is ConstructedMulliganGuideViewModel viewModel)
			{
				var newVisibility = viewModel.StatsVisibility == Visibility.Visible
					? Visibility.Collapsed
					: Visibility.Visible;
				viewModel.StatsVisibility = newVisibility;
				ConfigWrapper.AutoShowMulliganGuide = newVisibility == Visibility.Visible;
			}
		}
	}
}
