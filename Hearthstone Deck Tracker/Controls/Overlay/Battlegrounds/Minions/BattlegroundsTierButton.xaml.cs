using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static Hearthstone_Deck_Tracker.Windows.OverlayWindow;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsTierButton : UserControl, INotifyPropertyChanged
{
	public BattlegroundsTierButton()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static readonly DependencyProperty TierProperty = DependencyProperty.Register(
		nameof(Tier),
		typeof(int),
		typeof(BattlegroundsTierButton)
	);

	public static DependencyProperty ActiveProperty = DependencyProperty.Register(
		nameof(Active),
		typeof(bool),
		typeof(BattlegroundsTierButton),
		new PropertyMetadata(VisualsChanged)
	);

	public static DependencyProperty AvailableProperty = DependencyProperty.Register(
		nameof(Available),
		typeof(bool),
		typeof(BattlegroundsTierButton),
		new PropertyMetadata(VisualsChanged)
	);

	public static DependencyProperty FadedProperty = DependencyProperty.Register(
		nameof(Faded),
		typeof(bool),
		typeof(BattlegroundsTierButton),
		new PropertyMetadata(VisualsChanged)
	);

	public static readonly DependencyProperty ClickTierCommandProperty = DependencyProperty.Register(
		nameof(ClickTierCommand),
		typeof(Command<int>),
		typeof(BattlegroundsTierButton)
	);

	public Command<int>? ClickTierCommand
	{
		get { return (Command<int>?)GetValue(ClickTierCommandProperty); }
		set { SetValue(ClickTierCommandProperty, value); }
	}

	public int Tier
	{
		get { return (int)GetValue(TierProperty); }
		set
		{
			SetValue(TierProperty, value);
		}
	}

	public bool Active
	{
		get { return (bool)GetValue(ActiveProperty); }
		set
		{
			SetValue(ActiveProperty, value);
			Update();
		}
	}

	public bool Available
	{
		get { return (bool)GetValue(AvailableProperty); }
		set
		{
			SetValue(AvailableProperty, value);
			Update();
		}
	}

	public bool Faded
	{
		get { return (bool)GetValue(FadedProperty); }
		set
		{
			SetValue(FadedProperty, value);
			Update();
		}
	}

	private void Update()
	{
		ImageTierRemove.Visibility = _hovering && Active ? Visibility.Visible : Visibility.Collapsed;
		Glow.Visibility = Active || _hovering ? Visibility.Visible : Visibility.Collapsed;
		Glow.Opacity = Active ? 1 : 0.5;
	}

	private static void VisualsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var button = ((BattlegroundsTierButton)d);
		button.OnPropertyChanged(nameof(IconOpacity));
	}

	public double IconOpacity
	{
		get
		{
			if(Active)
				return 1;
			if(!Available)
				return _hovering ? 0.6 : 0.3;
			if(Faded && !_hovering)
				return 0.3;
			return 1;
		}
	}

	private bool _hovering;

	private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_hovering = true;
		Update();
	}

	private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_hovering = false;
		Update();
	}

	private void UserControl_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
	{
		ClickTierCommand?.Execute(Tier);
	}
}
