using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds;

/// <summary>
/// Button-like overlay control that avoids WPF ButtonBase click routing.
///
/// The Battlegrounds in-game overlay uses a click-through WS_EX_TRANSPARENT window model.
/// Under Wine/Linux, standard WPF Button Click/Command routing can be unreliable in that
/// window configuration even when hover/rendering still works. OverlayButton keeps the
/// ergonomic Command/Click authoring model while internally using the Border +
/// MouseLeftButtonUp path that works reliably in the overlay.
/// </summary>
public class OverlayButton : Border
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
		nameof(Command),
		typeof(ICommand),
		typeof(OverlayButton),
		new PropertyMetadata(null));

	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
		nameof(CommandParameter),
		typeof(object),
		typeof(OverlayButton),
		new PropertyMetadata(null));

	public event RoutedEventHandler? Click;

	public ICommand? Command
	{
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public object? CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);

		if(!IsEnabled)
			return;

		var command = Command;
		var commandParameter = CommandParameter;
		if(command != null && !command.CanExecute(commandParameter))
			return;

		Click?.Invoke(this, new RoutedEventArgs());
		command?.Execute(commandParameter);
		e.Handled = true;
	}
}
