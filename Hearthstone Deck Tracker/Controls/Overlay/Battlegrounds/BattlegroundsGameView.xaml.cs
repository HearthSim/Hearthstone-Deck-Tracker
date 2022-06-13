using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsGameView : INotifyPropertyChanged {

		private bool _minionsRendered;

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

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
			var scale = ((ScaleTransform)FinalBoardCanvas.RenderTransform).ScaleX;
			((BattlegroundsGameViewModel) DataContext).OnMouseEnter(finalBoardContainerActualWidth, scale);
		}

		private void Game_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(!FinalBoardTooltip)
				return;

			((BattlegroundsGameViewModel)DataContext).OnMouseLeave();
		}
	}
}
