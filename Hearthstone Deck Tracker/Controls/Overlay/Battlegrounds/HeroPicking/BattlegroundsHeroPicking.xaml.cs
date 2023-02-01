using System.Windows;
using Hearthstone_Deck_Tracker.Utility;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public sealed partial class BattlegroundsHeroPicking
	{
		public BattlegroundsHeroPicking()
		{
			InitializeComponent();
		}

		private void OverlayVisibilityToggle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var viewModel = (BattlegroundsHeroPickingViewModel)DataContext;
			var newVisibility = viewModel.StatsVisibility == Visibility.Visible
				? Visibility.Collapsed
				: Visibility.Visible;
			viewModel.StatsVisibility = newVisibility;
			ConfigWrapper.ShowBattlegroundsHeroPicking = newVisibility == Visibility.Visible;
		}

		public ICommand SetSelectedHeroDbfIdCommand => new Command<int>(value => ((BattlegroundsHeroPickingViewModel)DataContext).SelectedHeroDbfId = value);
	}
}
