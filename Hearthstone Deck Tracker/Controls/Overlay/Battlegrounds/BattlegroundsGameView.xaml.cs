using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsGameView {

		private bool _minionsRendered;
		

		public static readonly DependencyProperty FinalBoardTooltipProperty = DependencyProperty.Register("FinalBoardTooltip", typeof(bool), typeof(BattlegroundsGameView));

		public BattlegroundsGameView()
		{
			InitializeComponent();
		}

		public bool FinalBoardTooltip
		{
			get { return (bool)GetValue(FinalBoardTooltipProperty); }
			set
			{
				SetValue(FinalBoardTooltipProperty, value);
			}
		}

		private void Game_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!FinalBoardTooltip)
				return;

			var viewModel = (BattlegroundsGameViewModel)DataContext;
			if (!_minionsRendered)
			{
				foreach(var m in viewModel.FinalBoardMinions)
					FinalBoard.Children.Add(new BattlegroundsMinion(m));

				_minionsRendered = true;
				UpdateLayout();
			}

			var finalBoardContainerActualWidth = FinalBoardContainer.ActualWidth;
			((BattlegroundsGameViewModel) DataContext).OnMouseEnter(finalBoardContainerActualWidth);
		}

		private void Game_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!FinalBoardTooltip)
				return;

			((BattlegroundsGameViewModel)DataContext).OnMouseLeave();
		}
	}
}
