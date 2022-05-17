using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsGameView : UserControl
	{
		private bool minionsRendered = false;

		public BattlegroundsGameView()
		{
			InitializeComponent();
		}

		private void Game_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var viewModel = (BattlegroundsGameViewModel)DataContext;
			if (viewModel.FinalBoardTooltips && !minionsRendered)
			{
				foreach(var m in viewModel.FinalBoardMinions)
					FinalBoard.Children.Add(new BattlegroundsMinion(m));

				minionsRendered = true;
			}
			((BattlegroundsGameViewModel) DataContext).OnMouseEnter();
		}

		private void Game_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			((BattlegroundsGameViewModel)DataContext).OnMouseLeave();
		}
	}
}
