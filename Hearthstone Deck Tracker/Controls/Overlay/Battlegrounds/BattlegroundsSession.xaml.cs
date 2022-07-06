using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsSession : INotifyPropertyChanged
	{
		private readonly BrushConverter _bc = new();

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(BattlegroundsSession));
		public static readonly DependencyProperty FinalBoardTooltipProperty = DependencyProperty.Register("FinalBoardTooltip", typeof(bool), typeof(BattlegroundsSession));

		public BattlegroundsSession()
		{
			InitializeComponent();
			CogBtnVisibility = Visibility.Hidden;
		}

		private Visibility _cogBtnVisibility;
		public Visibility CogBtnVisibility
		{
			get => _cogBtnVisibility;
			set
			{
				_cogBtnVisibility = value;
				OnPropertyChanged();
			}
		}

		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
			set
			{
				SetValue(CornerRadiusProperty, value);
			}
		}

		public bool FinalBoardTooltip
		{
			get { return (bool)GetValue(FinalBoardTooltipProperty); }
			set
			{
				SetValue(FinalBoardTooltipProperty, value);
			}
		}
		
		private void Panel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			CogBtnVisibility = Visibility.Visible;
		}

		private void Panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			CogBtnVisibility = Visibility.Hidden;
		}

		private void BtnOptions_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Core.MainWindow.ActivateWindow();
			Core.MainWindow.Options.TreeViewItemOverlayBattlegrounds.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
		}

		private void BtnOptions_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			BtnOptions.Background = (Brush)_bc.ConvertFromString("#22FFFFFF");
		}

		private void BtnOptions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			BtnOptions.Background = (Brush)_bc.ConvertFromString("#00FFFFFF");
		}

		public void Show()
		{
			if (Visibility == Visibility.Visible || !Config.Instance.ShowSessionRecap)
				return;

			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Hidden;
		}
	}
}
