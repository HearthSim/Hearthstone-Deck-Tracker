using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;

public partial class BattlegroundsSession : INotifyPropertyChanged
{
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
}
