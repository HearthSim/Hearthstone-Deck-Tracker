using System;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Commands;

public class Command : ICommand
{
	private readonly Action _action;

	public Command(Action action)
	{
		_action = action;
	}

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter) => _action.Invoke();
}

public class Command<T> : ICommand
{
	private readonly Action<T> _action;

	public Command(Action<T> action)
	{
		_action = action;
	}

	public event EventHandler? CanExecuteChanged;

	// CanExecute will run when "Command" is evaluated. If "CommandParameter" is set after "Command" the parameter will
	// be null. It is not re-evaluated when "CommandParameter" changes. The workaround is to set "CommandParameter"
	// first, but this is easy to forget. Therefore, we always treat parameterized commands as executable.
	// (This is fixed in .NET 7)
	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		if(parameter is not T value)
			return;
		_action.Invoke(value);
	}
}
