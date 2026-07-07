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
		new PropertyMetadata(null, OnCommandChanged));

	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
		nameof(CommandParameter),
		typeof(object),
		typeof(OverlayButton),
		new PropertyMetadata(null, OnCommandParameterChanged));

	public event RoutedEventHandler? Click;

	private bool _canExecute = true;

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

	// fold the command's executability into the enabled state, so a command reporting
	// CanExecute == false disables the button (like a real Button) without clobbering an
	// explicit IsEnabled binding - coercion ANDs this with the externally set value
	protected override bool IsEnabledCore => base.IsEnabledCore && _canExecute;

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var button = (OverlayButton)d;
		if(e.OldValue is ICommand oldCommand)
			CanExecuteChangedEventManager.RemoveHandler(oldCommand, button.OnCanExecuteChanged);
		if(e.NewValue is ICommand newCommand)
			CanExecuteChangedEventManager.AddHandler(newCommand, button.OnCanExecuteChanged);
		button.UpdateCanExecute();
	}

	private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((OverlayButton)d).UpdateCanExecute();

	private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateCanExecute();

	private void UpdateCanExecute()
	{
		var command = Command;
		var canExecute = command == null || command.CanExecute(CommandParameter);
		if(canExecute == _canExecute)
			return;
		_canExecute = canExecute;
		CoerceValue(IsEnabledProperty);
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
		// ponytail: unconditionally consume the click even with no handler attached -
		// prevents the raw mouse event from passing through the overlay to the game.
		e.Handled = true;
	}
}
