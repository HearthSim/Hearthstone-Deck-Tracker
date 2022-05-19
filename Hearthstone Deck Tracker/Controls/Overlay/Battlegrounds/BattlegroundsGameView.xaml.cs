using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsGameView
	{
		private bool minionsRendered;

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
				UpdateLayout();
			}

			var finalBoardContainerActualWidth = FinalBoardContainer.ActualWidth;
			var scale = ((ScaleTransform)FinalBoardCanvas.RenderTransform).ScaleX;
			((BattlegroundsGameViewModel) DataContext).OnMouseEnter(finalBoardContainerActualWidth, scale);
		}

		private void Game_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			((BattlegroundsGameViewModel)DataContext).OnMouseLeave();
		}
	}
}
