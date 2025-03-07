using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsMinionTypeButton : UserControl, INotifyPropertyChanged
{
	public BattlegroundsMinionTypeButton()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static readonly DependencyProperty MinionTypeProperty = DependencyProperty.Register(
		nameof(MinionType),
		typeof(Race),
		typeof(BattlegroundsMinionTypeButton)
	);

	public static DependencyProperty ActiveProperty = DependencyProperty.Register(
		nameof(Active),
		typeof(bool),
		typeof(BattlegroundsMinionTypeButton),
		new PropertyMetadata(false, (d, e) => {
			var button = (BattlegroundsMinionTypeButton)d;
			button.OnPropertyChanged(nameof(RemoveIconVisibility));
			button.OnPropertyChanged(nameof(IconOpacity));
			button.OnPropertyChanged(nameof(GlowVisibility));
			button.OnPropertyChanged(nameof(GlowOpacity));
		})
	);

	public static DependencyProperty AvailableProperty = DependencyProperty.Register(
		nameof(Available),
		typeof(bool),
		typeof(BattlegroundsMinionTypeButton),
		new PropertyMetadata(VisualsChanged)
	);

	public static DependencyProperty FadedProperty = DependencyProperty.Register(
		nameof(Faded),
		typeof(bool),
		typeof(BattlegroundsMinionTypeButton),
		new PropertyMetadata(VisualsChanged)
	);

	public static readonly DependencyProperty ClickMinionTypeCommandProperty = DependencyProperty.Register(
		nameof(ClickMinionTypeCommand),
		typeof(Command<Race>),
		typeof(BattlegroundsMinionTypeButton)
	);

	public Command<Race>? ClickMinionTypeCommand
	{
		get { return (Command<Race>?)GetValue(ClickMinionTypeCommandProperty); }
		set { SetValue(ClickMinionTypeCommandProperty, value); }
	}


	public Race MinionType
	{
		get {
			return (Race)GetValue(MinionTypeProperty);
		}
		set
		{
			SetValue(MinionTypeProperty, value);
			OnPropertyChanged(nameof(TribeName));
		}
	}

	public bool Active
	{
		get { return (bool)GetValue(ActiveProperty); }
		set
		{
			SetValue(ActiveProperty, value);
		}
	}

	public bool Available
	{
		get { return (bool)GetValue(AvailableProperty); }
		set
		{
			SetValue(AvailableProperty, value);
			OnPropertyChanged(nameof(IconOpacity));
		}
	}

	public bool Faded
	{
		get { return (bool)GetValue(FadedProperty); }
		set
		{
			SetValue(FadedProperty, value);
			OnPropertyChanged(nameof(IconOpacity));
		}
	}

	private static void VisualsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var button = ((BattlegroundsMinionTypeButton)d);
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

	public string TribeName => (HearthDbConverter.GetUppercaseLocalizedRace(MinionType) ?? "");

	public Visibility RemoveIconVisibility => _hovering && Active ? Visibility.Visible : Visibility.Collapsed;

	public Visibility GlowVisibility => Active || _hovering ? Visibility.Visible : Visibility.Collapsed;

	public double GlowOpacity => Active ? 1 : 0.5;

	private bool _hovering;

	private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_hovering = true;
		OnPropertyChanged(nameof(IconOpacity));
		OnPropertyChanged(nameof(GlowVisibility));
		OnPropertyChanged(nameof(RemoveIconVisibility));
	}

	private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_hovering = false;
		OnPropertyChanged(nameof(IconOpacity));
		OnPropertyChanged(nameof(GlowVisibility));
		OnPropertyChanged(nameof(RemoveIconVisibility));
	}

	private void UserControl_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
	{
		ClickMinionTypeCommand?.Execute(MinionType);
	}
}
